using Hangfire;
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
using zapread.com.Models.Database.Financial;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Controller for the /Vote route
    /// </summary>
    public class VoteController : Controller
    {
        /// <summary>
        /// This is the REST call model
        /// </summary>
        public class Vote
        {
            /// <summary>
            /// The post or comment Id
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// direction of vote. 0 = down 1 = up
            /// </summary>
            public int d { get; set; }

            /// <summary>
            /// the amount of the vote in Satoshi
            /// </summary>
            public int a { get; set; }

            /// <summary>
            /// The transaction id if anonymous vote
            /// </summary>
            public int tx { get; set; }
        }

        /// <summary>
        /// User voting on a post
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> Post(Vote v)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, result = "error", message = "Invalid" });
            }

            // Bad parameters check
            if (v == null || v.a < 1)
            {
                return Json(new { success = false, result = "error", message = "Invalid" });
            }

            // Do checks
            using (var db = new ZapContext())
            {
                var postInfo = await db.Posts
                    .Where(p => p.PostId == v.Id)
                    .Select(p => new
                    {
                        p.Score,
                        p.UserId.Reputation
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var userAppId = User.Identity.GetUserId();

                double scoreAdj = 0.0;

                if (userAppId == null) // Anonymous vote
                {
                    // Check if vote tx has been claimed
                    var txIsValid = await db.LightningTransactions
                        .Where(tx => tx.Id == v.tx)
                        .Select(tx => tx.IsSettled && !tx.IsSpent)
                        .FirstOrDefaultAsync().ConfigureAwait(true); // bool default is false

                    if (!txIsValid)
                    {
                        return Json(new { success = false, result = "error", message = "No transaction to vote with" });
                    }

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: v.a * (v.d == 1 ? 1 : -1),
                        targetRep: v.d == 1 ? 0 : postInfo.Reputation,
                        actorRep: 0);
                }
                else
                {
                    var userInfo = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => new { 
                            hasFunds = u.Funds.Balance > v.a,
                            u.Reputation
                        })
                        .FirstOrDefaultAsync().ConfigureAwait(true); // bool default is false

                    if (!userInfo.hasFunds)
                    {
                        return Json(new { success = false, result = "error", message = "Error with requesting user." });
                    }

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: v.a * (v.d == 1 ? 1 : -1),
                        targetRep: v.d == 1 ? 0 : postInfo.Reputation,
                        actorRep: userInfo.Reputation);
                }

                if (postInfo == null)
                {
                    return Json(new { success = false, result = "error", message = "Invalid Post" });
                }

                // All good - queue processing - this part is slower, so it will be done in a background job
                // Return the optimistic result to the user to improve UI response
                BackgroundJob.Enqueue<VoteService>(x => x.PostVote(
                    userAppId,
                    v.Id,       // postId
                    v.a,        // amount
                    v.d == 1,   // isUpvote
                    v.tx        // txid
                ));

                // Note that this is actually going to be saved in the background job
                // we are only sending the optimistic estimate here
                var newScore = postInfo.Score + scoreAdj;

                // Return quick results
                return Json(new
                {
                    success = true,
                    result = "success",
                    delta = 1,
                    score = newScore,
                    balance = 0,
                    scoreStr = newScore.ToAbbrString()
                });
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
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
                    var vtx = db.LightningTransactions.FirstOrDefault(tx => tx.Id == v.tx);

                    if (vtx == null || vtx.IsSpent == true)
                    {
                        return Json(new { result = "error", message = "No transaction to vote with" });
                    }

                    vtx.IsSpent = true;
                    await db.SaveChangesAsync();
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