using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    public class VoteController : Controller
    {
        /// <summary>
        /// This is the REST call model
        /// </summary>
        public class Vote
        {
            public int Id { get; set; }
            public int d { get; set; }
            public int a { get; set; }
            public int tx { get; set; }
        }

        private static void Doit()
        {
            MailingService.Send(user: "Notify",
                message: new UserEmailModel()
                {
                    Subject = "Async message from Hangfire",
                    Body = "Testing Hangfire",
                    Destination = "steven.horn.mail@gmail.com",
                    Email = "",
                    Name = "ZapRead.com Notify"
                });
        }

        /// <summary>
        /// User voting on a post
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post(Vote v)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            if (v.a < 1)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            var userId = User.Identity.GetUserId();

            bool IsUserOwner = false;
            bool IsUserAnonymous = false;

            if (userId == null) // Anonymous vote
            {
                IsUserAnonymous = true;
            }

            using (var db = new ZapContext())
            {
                var website = await db.ZapreadGlobals.FirstOrDefaultAsync(i => i.Id == 1);

                User user = null;

                var post = await db.Posts
                    .Include(p => p.VotesUp)
                    .Include(p => p.VotesDown)
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.Funds)
                    .FirstOrDefaultAsync(p => p.PostId == v.Id);

                if (post == null)
                {
                    return Json(new { result = "error", message = "Invalid Post" });
                }

                if (userId == null)// Anonymous vote
                {
                    // Check if vote tx has been claimed
                    if (v.tx != -1337) //debugging secret
                    {
                        var vtx = db.LightningTransactions.FirstOrDefault(tx => tx.Id == v.tx);

                        if (vtx == null || vtx.IsSpent == true)
                        {
                            return Json(new { result = "error", message = "No transaction to vote with" });
                        }

                        vtx.IsSpent = true;
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    user = await db.Users
                        .Include(usr => usr.Funds)
                        .Include(usr => usr.EarningEvents)
                        .Include(usr => usr.SpendingEvents)
                        .FirstOrDefaultAsync(u => u.AppId == userId);

                    if (user == null)
                    {
                        return Json(new { result = "error", message = "Invalid User" });
                    }

                    if (user.Funds.Balance < v.a)
                    {
                        return Json(new { result = "error", message = "Insufficient Funds." });
                    }

                    user.Funds.Balance -= v.a;
                }

                var spendingEvent = new SpendingEvent()
                {
                    Amount = v.a,
                    Post = post,
                    TimeStamp = DateTime.UtcNow,
                };

                double userBalance = 0.0;
                if (user != null)
                {
                    userBalance = user.Funds.Balance;
                    user.SpendingEvents.Add(spendingEvent);
                }

                long authorRep = post.UserId.Reputation;
                long userRep = 0;

                if (v.d == 1) // Voted up
                {
                    if (user != null && post.VotesUp.Contains(user))
                    {
                        // Already voted - remove upvote?
                        //post.VotesUp.Remove(user);
                        //user.PostVotesUp.Remove(post);
                        //post.Score = post.VotesUp.Count() - post.VotesDown.Count();
                        //return Json(new { result = "success", message = "Already Voted", delta = 0, score = post.Score, balance = user.Funds.Balance });
                    }
                    else if (user != null)
                    {
                        post.VotesUp.Add(user);
                        user.PostVotesUp.Add(post);
                        userRep = user.Reputation;
                    }

                    var adj = ReputationService.GetReputationAdjustedAmount(v.a, 0, userRep);
                    post.Score += Convert.ToInt32(adj);// v.a;

                    // Record and assign earnings
                    // Related to post owner
                    post.TotalEarned += 0.6 * v.a;

                    var ea = new EarningEvent()
                    {
                        Amount = 0.6 * v.a,
                        OriginType = 0,
                        TimeStamp = DateTime.UtcNow,
                        Type = 0,
                        OriginId = post.PostId,
                    };

                    var webratio = 0.1;     // Website income
                    var comratio = 0.1;     // Community pool

                    var owner = post.UserId;

                    if (user != null && owner.Id == user.Id)
                    {
                        IsUserOwner = true;
                    }

                    if (owner != null)
                    {
                        // If user is not anonymous, and user is not owner, add reputation
                        if (!IsUserAnonymous && !IsUserOwner)
                        {
                            owner.Reputation += v.a;
                        }

                        owner.EarningEvents.Add(ea);
                        owner.TotalEarned += 0.6 * v.a;

                        if (owner.Funds == null)
                        {
                            owner.Funds = new UserFunds() { Balance = 0.6 * v.a, TotalEarned = 0.6 * v.a };
                        }
                        else
                        {
                            owner.Funds.Balance += 0.6 * v.a;
                        }
                    }
                    else
                    {
                        ; // TODO: log this error
                    }

                    var postGroup = post.Group;
                    if (postGroup != null)
                    {
                        postGroup.TotalEarnedToDistribute += 0.2 * v.a;
                    }
                    else
                    {
                        // not in group - send to community
                        comratio += 0.2;
                    }

                    if (website != null)
                    {
                        // Will be distributed to all users
                        website.CommunityEarnedToDistribute += comratio * v.a;

                        // And to the website
                        website.ZapReadTotalEarned += webratio * v.a;
                        website.ZapReadEarnedBalance += webratio * v.a;
                    }
                    else
                    {
                        throw new Exception("Unable to load Zapread DB globals.");
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "error", message = "Error" });
                    }

                    NotificationService.SendIncomeNotification(0.6 * v.a, owner.AppId, "Post upvote", Url.Action("Detail", "Post", new { post.PostId }));

                    return Json(new { result = "success", delta = 1, score = post.Score, balance = userBalance, scoreStr = post.Score.ToAbbrString() });
                }
                else
                {
                    // Voted down
                    if (user != null && post.VotesDown.Contains(user))
                    {
                        //post.VotesDown.Remove(user);
                        //user.PostVotesDown.Remove(post);
                        //post.Score = post.VotesUp.Count() - post.VotesDown.Count();
                        //return Json(new { result = "success", message = "Already Voted", delta = 0, score = post.Score, balance = user.Funds.Balance });
                    }
                    else if (user != null)
                    {
                        post.VotesDown.Add(user);
                        user.PostVotesDown.Add(post);
                        userRep = user.Reputation;
                    }
                    //post.VotesUp.Remove(user);
                    //user.PostVotesUp.Remove(post);
                    var adj = ReputationService.GetReputationAdjustedAmount(-1 * v.a, authorRep, userRep);

                    post.Score += Convert.ToInt32(adj);// v.a;// post.VotesUp.Count() - post.VotesDown.Count();

                    // Record and assign earnings
                    // Related to post owner
                    var webratio = 0.1;
                    var comratio = 0.1;

                    var owner = post.UserId;

                    if (user != null && owner.Id == user.Id)
                    {
                        IsUserOwner = true;
                    }

                    if (!IsUserAnonymous && !IsUserOwner)
                    {
                        owner.Reputation -= v.a;
                    }

                    var postGroup = post.Group;
                    if (postGroup != null)
                    {
                        postGroup.TotalEarnedToDistribute += 0.8 * v.a;
                    }
                    else
                    {
                        comratio += 0.8;
                    }

                    if (website != null)
                    {
                        // Will be distributed to all users
                        website.CommunityEarnedToDistribute += comratio * v.a;

                        // And to the website
                        website.ZapReadTotalEarned += webratio * v.a;
                        website.ZapReadEarnedBalance += webratio * v.a;
                    }

                    await db.SaveChangesAsync();
                    return Json(new { result = "success", delta = -1, score = post.Score, balance = userBalance, scoreStr = post.Score.ToAbbrString() });
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> Comment(Vote v)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            if (v.a < 1)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            bool IsUserOwner = false;
            bool IsUserAnonymous = false;

            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                IsUserAnonymous = true;
            }

            using (var db = new ZapContext())
            {
                var comment = db.Comments
                    .Include(c => c.VotesUp)
                    .Include(c => c.VotesDown)
                    .Include(c => c.UserId)
                    .Include(c => c.UserId.Funds)
                    .Include(c => c.Post)
                    .FirstOrDefault(c => c.CommentId == v.Id);

                if (comment == null)
                {
                    return Json(new { result = "error", message = "Invalid Comment" });
                }

                User user = null;

                if (userId == null) // Anonymous vote
                {
                    // Check if vote tx has been claimed
                    if (v.tx != -1337) //debugging secret
                    {
                        var vtx = db.LightningTransactions.FirstOrDefault(tx => tx.Id == v.tx);

                        if (vtx == null || vtx.IsSpent == true)
                        {
                            return Json(new { result = "error", message = "No transaction to vote with" });
                        }

                        vtx.IsSpent = true;
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    user = db.Users
                        .Include(usr => usr.Funds)
                        .Include(usr => usr.EarningEvents)
                        .FirstOrDefault(u => u.AppId == userId);

                    if (user == null)
                    {
                        return Json(new { result = "error", message = "Invalid User" });
                    }

                    if (user.Funds.Balance < v.a)
                    {
                        return Json(new { result = "error", message = "Insufficient Funds." });
                    }

                    user.Funds.Balance -= v.a;
                }

                var spendingEvent = new SpendingEvent()
                {
                    Amount = v.a,
                    Comment = comment,
                    TimeStamp = DateTime.UtcNow,
                };

                double userBalance = 0.0;
                if (user != null)
                {
                    userBalance = user.Funds.Balance;
                    user.SpendingEvents.Add(spendingEvent);
                }

                long authorRep = comment.UserId.Reputation;
                long userRep = 0;

                if (v.d == 1)
                {
                    if (comment.VotesUp.Contains(user))
                    {
                        // Already voted
                    }
                    else if (user != null)
                    {
                        comment.VotesUp.Add(user);
                        user.CommentVotesUp.Add(comment);
                        userRep = user.Reputation;
                    }

                    var adj = ReputationService.GetReputationAdjustedAmount(v.a, 0, userRep);

                    comment.Score += Convert.ToInt32(adj);// v.a;
                    comment.TotalEarned += 0.6 * v.a;

                    var ea = new Models.EarningEvent()
                    {
                        Amount = 0.6 * v.a,
                        OriginType = 1,                                 // Comment
                        TimeStamp = DateTime.UtcNow,
                        Type = 0,                                       // Direct earning
                        OriginId = Convert.ToInt32(comment.CommentId),  // For linking back to comment
                    };

                    var webratio = 0.1;
                    var comratio = 0.1;

                    var owner = comment.UserId;

                    if (user != null && user.Id == owner.Id)
                    {
                        IsUserOwner = true;
                    }

                    if (owner != null)
                    {
                        if (!IsUserAnonymous && !IsUserOwner)
                        {
                            owner.Reputation += v.a;
                        }
                        owner.EarningEvents.Add(ea);
                        owner.TotalEarned += 0.6 * v.a;
                        if (owner.Funds == null)
                        {
                            owner.Funds = new UserFunds() { Balance = 0.6 * v.a, TotalEarned = 0.6 * v.a };
                        }
                        else
                        {
                            owner.Funds.Balance += 0.6 * v.a;
                        }
                    }

                    if (comment.Post != null)
                    {
                        var group = comment.Post.Group;
                        if (group != null)
                        {
                            group.TotalEarnedToDistribute += 0.2 * v.a;
                        }
                        else
                        {
                            // not in group - send to community
                            comratio += 0.2;
                        }
                    }
                    else
                    {
                        comratio += 0.2;
                    }

                    var website = db.ZapreadGlobals.FirstOrDefault(i => i.Id == 1);

                    if (website != null)
                    {
                        // Will be distributed to all users
                        website.CommunityEarnedToDistribute += comratio * v.a;

                        // And to the website
                        website.ZapReadTotalEarned += webratio * v.a;
                        website.ZapReadEarnedBalance += webratio * v.a;
                    }
                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "error", message = "Error" });
                    }

                    NotificationService.SendIncomeNotification(0.6 * v.a, owner.AppId, "Comment upvote", Url.Action("Detail", "Post", new { PostId = comment.Post.PostId }));

                    return Json(new { result = "success", delta = 1, score = comment.Score, balance = userBalance, scoreStr = comment.Score.ToAbbrString() });
                }
                else
                {
                    if (comment.VotesDown.Contains(user))
                    {
                        // Already voted
                    }
                    else if (user != null)
                    {
                        comment.VotesDown.Add(user);
                        user.CommentVotesDown.Add(comment);
                        userRep = user.Reputation;
                    }

                    var adj = ReputationService.GetReputationAdjustedAmount(-1 * v.a, authorRep, userRep);
                    comment.Score += Convert.ToInt32(adj);// v.a;

                    // Record and assign earnings
                    // Related to post owner
                    var webratio = 0.1;
                    var comratio = 0.1;

                    var owner = comment.UserId;

                    if (user != null && user.Id == owner.Id)
                    {
                        IsUserOwner = true;
                    }

                    if (!IsUserAnonymous && !IsUserOwner)
                    {
                        owner.Reputation -= v.a;
                    }

                    if (comment.Post != null)
                    {
                        var postGroup = comment.Post.Group;
                        if (postGroup != null)
                        {
                            postGroup.TotalEarnedToDistribute += 0.8 * v.a;
                        }
                        else
                        {
                            comratio += 0.8;
                        }
                    }
                    else
                    {
                        comratio += 0.8;
                    }

                    var website = db.ZapreadGlobals.FirstOrDefault(i => i.Id == 1);

                    if (website != null)
                    {
                        // Will be distributed to all users
                        website.CommunityEarnedToDistribute += comratio * v.a;

                        // And to the website
                        website.ZapReadTotalEarned += webratio * v.a;
                        website.ZapReadEarnedBalance += webratio * v.a;
                    }

                    await db.SaveChangesAsync();
                    return Json(new { result = "success", delta = -1, score = comment.Score, balance = userBalance, scoreStr = comment.Score.ToAbbrString() });
                }
            }
        }
    }
}