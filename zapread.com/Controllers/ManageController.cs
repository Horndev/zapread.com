using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.API.Account;
using zapread.com.Models.API.Account.Transactions;
using zapread.com.Models.API.DataTables;
using zapread.com.Models.Database;
using zapread.com.Models.Database.Financial;
using zapread.com.Models.Manage;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Controller for manage pages
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        /// <summary>
        /// default constructor
        /// </summary>
        public ManageController() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Financial()
        {
            XFrameOptionsDeny();
            return View();
        }

        private void XFrameOptionsDeny()
        {
            try
            {
                Response.AddHeader("X-Frame-Options", "DENY");
            }
            catch
            {
                // TODO: add error handling - temp fix for unit test.
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Manage/APIKeys/")]
        public ActionResult APIKeys()
        {
            XFrameOptionsDeny();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetLNTransactions(DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var values = await db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.LNTransactions)
                    .OrderByDescending(t => t.TimestampCreated)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .Select(t => new
                    {
                        Time = t.TimestampCreated,
                        Type = t.IsDeposit,
                        t.Amount,
                        t.Memo,
                        t.IsSettled,
                        t.IsLimbo,
                    })
                    .ToListAsync().ConfigureAwait(true);

                int numrec = await db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.LNTransactions)
                    .CountAsync();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values.Select(v => new
                    {
                        Time = v.Time == null ? "" : v.Time.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        v.Type,
                        v.Amount,
                        v.Memo,
                        v.IsSettled,
                        v.IsLimbo,
                    })
                };
                return Json(ret);
            }
        }

        private string GetEarningURL(dynamic t, List<long> commentIds, List<Comment> comments)
        {
            if (t.Type == 1)
            {
                if (t.OriginId > 0)
                    return Url.Action(controllerName: "Group", actionName: "GroupDetail", routeValues: new { id = t.OriginId });
            }
            else if (t.Type == 0 && t.OriginType == 0)
            {
                if (t.OriginId > 0)
                    return Url.Action(controllerName: "Post", actionName: "Detail", routeValues: new { PostId = t.OriginId });
            }
            else if (t.Type == 0 && t.OriginType == 1)
            {
                var postId = commentIds.Contains(t.OriginId) ? comments.FirstOrDefault(c => c.CommentId == t.OriginId)?.Post.PostId : 0;
                if (postId > 0)
                    return Url.Action(controllerName: "Post", actionName: "Detail", routeValues: new { PostId = postId });
            }
            return t.OriginId.ToString();
        }

        private static string GetEarningMemo(dynamic t, List<int> groupIds, List<Group> groups, List<int> postIds, List<Post> posts, List<long> commentIds, List<Comment> comments)
        {
            if (t.Type == 1 && t.OriginId > 0)
            {
                return groupIds.Contains(t.OriginId) ? groups.FirstOrDefault(g => g.GroupId == t.OriginId)?.GroupName : "";
            }

            if (t.Type == 0 && t.OriginType == 0)
            {
                string memo = postIds.Contains(t.OriginId) ? posts.FirstOrDefault(p => p.PostId == t.OriginId)?.PostTitle : "";
                if (memo == null)
                    memo = "";
                if (memo.Length > 33)
                    memo = memo.Substring(0, 30) + "...";
                return memo;
            }

            if (t.Type == 0 && t.OriginType == 1 && t.OriginId > 0) // Comment
            {
                var postId = commentIds.Contains(t.OriginId) ? comments.FirstOrDefault(c => c.CommentId == t.OriginId)?.Post.PostId : 0;
                if (postId != null && postId > 0)
                {
                    string memo = postIds.Contains(postId.Value) ? posts.FirstOrDefault(p => p.PostId == postId)?.PostTitle : "";
                    if (memo == null)
                        memo = "";
                    if (memo.Length > 33)
                        memo = memo.Substring(0, 30) + "...";
                    return memo;
                }
                return postId.ToString();
            }
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> GetEarningEvents(DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "No parameters provided." });
            }

            var userAppId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                // Query data
                var pageData = await db.Users
                    .Where(us => us.AppId == userAppId)
                    .SelectMany(us => us.EarningEvents)
                    .OrderByDescending(e => e.TimeStamp)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .Select(t => new
                    {
                        t.Id,
                        TimeValue = t.TimeStamp.Value,
                        t.Amount,
                        t.Type,
                        t.OriginType,
                        t.OriginId,
                    })
                    .ToListAsync().ConfigureAwait(false);

                var commentIds = pageData
                    .Where(e => e.Type == 0 && e.OriginType == 1)
                    .Select(e => Convert.ToInt64(e.OriginId)).ToList();

                var groupIds = pageData
                    .Where(e => e.Type == 1)
                    .Select(e => e.OriginId).ToList();

                var comments = await db.Comments
                    .Include(c => c.Post)
                    .Where(c => commentIds.Contains(c.CommentId)).ToListAsync().ConfigureAwait(false);

                var postIds = pageData
                    .Where(e => e.Type == 0 && e.OriginType == 0)
                    .Select(e => e.OriginId).ToList();

                var cids = comments.Where(c => c.Post != null)
                    .Select(c => c.Post.PostId);

                postIds = postIds.Union(cids).ToList();

                var posts = await db.Posts.Where(p => postIds.Contains(p.PostId)).ToListAsync().ConfigureAwait(false);
                var groups = await db.Groups.Where(g => groupIds.Contains(g.GroupId)).ToListAsync().ConfigureAwait(false);

                //Format data
                var data = pageData.Select(t => new
                {
                    Id = t.Id,
                    Time = t.TimeValue.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    t.Amount,
                    Type = t.Type == 0 ? (t.OriginType == 0 ? "Post" : t.OriginType == 1 ? "Comment" : t.OriginType == 2 ? "Tip" : "Unknown") : t.Type == 1 ? "Group" : t.Type == 2 ? "Community" : t.Type == 4 ? "Referral Bonus" : "Unknown",
                    URL = GetEarningURL(t, commentIds, comments),
                    Memo = GetEarningMemo(t, groupIds, groups, postIds, posts, commentIds, comments),
                }).ToList();

                int numrec = await db.Users
                    .Where(us => us.AppId == userAppId)
                    .SelectMany(us => us.EarningEvents)
                    .CountAsync().ConfigureAwait(false);

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data,
                };
                return Json(ret);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> GetSpendingEvents(DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "No parameters provided."});
            }

            var userAppId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                int numrec = await db.Users
                    .Where(us => us.AppId == userAppId)
                    .SelectMany(usr => usr.SpendingEvents)
                    .CountAsync().ConfigureAwait(false);

                // Query data
                var pageData = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .SelectMany(usr => usr.SpendingEvents)
                    .OrderByDescending(e => e.TimeStamp)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .Select(t => new 
                    {
                        TimeValue = t.TimeStamp.Value,
                        @Type = t.Post != null ? "Post" : (t.Comment != null ? "Comment" : (t.Group != null ? "Group" : "Other")),
                        t.Amount,
                        TypeId = t.Post != null ? t.Post.PostId : (t.Comment != null ? t.Comment.Post.PostId : (t.Group != null ? t.Group.GroupId : 0))
                    })
                    .ToListAsync().ConfigureAwait(true);

                // Formatting (in-memory)
                var data = pageData.Select(t => new
                {
                    Time = t.TimeValue.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    t.Type,
                    t.Amount,
                    t.TypeId,
                }).ToList();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data,
                };
                return Json(ret);
            }
        }

        /// <summary>
        /// MVC API Call to tip a user
        /// </summary>
        /// <param name="id">the user-id of the user to tip</param>
        /// <param name="amount">amount to tip in Satoshi</param>
        /// <param name="tx">transaction id to use if anonymous tip</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult TipUser(int id, int? amount, int? tx)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new { success = false, Result = "Failure", message = "Tips Disabled" });

            // The following code needs to be re-written & cleaned up before tips can be re-enabled.
            //if (amount == null || amount.Value < 1)
            //{
            //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //    return Json(new { success = false, Result = "Failure", message = "Invalid amount" });
            //}
            //using (var db = new ZapContext())
            //{
            //    var receiver = await db.Users
            //        .Where(u => u.Id == id)
            //        .FirstOrDefaultAsync().ConfigureAwait(true);
            //    if (receiver == null)
            //    {
            //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //        return Json(new { success = false, Result = "Failure", message = "User not found." });
            //    }
            //    string senderName = "anonymous";
            //    bool saveAborted = false;
            //    UserFunds userFunds = null;
            //    LNTransaction vtx = null;
            //    var userAppId = User.Identity.GetUserId();
            //    if (tx == null) // Pay the user from the logged-in account balance
            //    {
            //        if (userAppId == null)
            //        {
            //            return Json(new { success = false, Result = "Failure", message = "User not found." });
            //        }
            //        var userName = await db.Users
            //            .Where(u => u.AppId == userAppId)
            //            .Select(u => u.Name)
            //            .FirstAsync().ConfigureAwait(true);
            //        // Notify receiver
            //        senderName = "<a href='"
            //            + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = userName })
            //            + "'>" + userName + "</a>";
            //    }
            //    else // Anonymous tip
            //    {
            //        vtx = await db.LightningTransactions
            //            .FirstOrDefaultAsync(txn => txn.Id == tx)
            //            .ConfigureAwait(true);
            //        // If trying to "re-use" an anonymous tip - fail it.
            //        if (vtx == null || vtx.IsSpent == true)
            //        {
            //            Response.StatusCode = (int)HttpStatusCode.Forbidden;
            //            return Json(new { Result = "Failure", message = "Transaction not found" });
            //        }
            //    }
            //    // Begin financial part
            //    var receiverFunds = await db.Users
            //        .Where(u => u.Id == id)
            //        .Select(u => u.Funds)
            //        .FirstOrDefaultAsync().ConfigureAwait(true);
            //    if (tx == null)
            //    {
            //        userFunds = await db.Users
            //        .Where(u => u.AppId == userAppId)
            //        .Select(u => u.Funds)
            //        .FirstOrDefaultAsync().ConfigureAwait(true);
            //    }
            //    int attempts = 0;
            //    bool saveFailed;
            //    saveAborted = false;
            //    do
            //    {
            //        attempts++;
            //        saveFailed = false;
            //        if (attempts < 50)
            //        {
            //            if (tx == null) // Do this only if tip was from a logged-in user
            //            {
            //                // Ensure user has the funds available.
            //                if (userFunds.Balance < amount)
            //                {
            //                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
            //                    return Json(new { success = false, Result = "Failure", message = "Not enough funds." });
            //                }
            //                userFunds.Balance -= amount.Value;
            //            }
            //            receiverFunds.Balance += amount.Value;
            //            try
            //            {
            //                db.SaveChanges(); // synchronous
            //            }
            //            catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
            //            {
            //                saveFailed = true;
            //                foreach(var entry in ex.Entries)
            //                {
            //                    entry.Reload();
            //                }
            //            }
            //        }
            //        else
            //        {
            //            saveAborted = true;
            //        }
            //    }
            //    while (saveFailed);
            //    if (saveAborted == false)
            //    {
            //        if (tx == null)
            //        {
            //            // Add spending event
            //            db.Users.Where(u => u.AppId == userAppId)
            //            .Select(u => u.SpendingEvents)
            //            .First().Add(new SpendingEvent()
            //            {
            //                Amount = amount.Value,
            //                TimeStamp = DateTime.UtcNow,
            //            });
            //        }
            //        else // anonymous tip
            //        {
            //            vtx.IsSpent = true;
            //            await db.SaveChangesAsync().ConfigureAwait(true);
            //        }
            //    }
            //    else
            //    {
            //        if (tx == null)
            //        {
            //            Response.StatusCode = (int)HttpStatusCode.Conflict;
            //            return Json(new 
            //            { 
            //                success = false, 
            //                Result = "Failure", 
            //                message = "Too many balance updates.  Please try again later." 
            //            });
            //        }
            //        else
            //        {
            //            // need to handle assignment of anonymous tx to user.
            //            // we don't want funds to be lost. This will be handled by 
            //            // cron process checking db sync with LND node.
            //        }
            //    }
            //    // Add Earning Event
            //    db.Users.First(u => u.Id == id).EarningEvents.Add(new EarningEvent()
            //    {
            //        Amount = amount.Value,
            //        OriginType = 2,
            //        TimeStamp = DateTime.UtcNow,
            //        Type = 0,
            //    });
            //    // Do notifications for the receiver
            //    var receiverSettings = await db.Users
            //        .Where(u => u.Id == id)
            //        .Select(u => new {
            //            DoNotify = u.Settings == null ? false : u.Settings.NotifyOnReceivedTip
            //        })
            //        .FirstOrDefaultAsync().ConfigureAwait(true);
            //    if (receiverSettings.DoNotify)
            //    {
            //        var alert = new UserAlert()
            //        {
            //            TimeStamp = DateTime.Now,
            //            Title = "You received a tip!",
            //            Content = "From: " + senderName + " <br/> Amount: " + amount.ToString() + " Satoshi.",
            //            IsDeleted = false,
            //            IsRead = false,
            //            To = receiver,
            //        };
            //        db.Users.First(u => u.Id == id).Alerts.Add(alert);
            //        string receiverEmail = UserManager.FindById(receiver.AppId).Email;
            //        // queue for sending the email
            //        BackgroundJob.Enqueue<MailingService>(x => x.SendI(
            //            new UserEmailModel()
            //            {
            //                Destination = receiverEmail,
            //                Body = "From: " + senderName + " <br/> Amount: "
            //                    + amount.ToString()
            //                    + " Satoshi.<br/><br/><a href='https://www.zapread.com'>zapread.com</a>",
            //                Email = "",
            //                Name = "ZapRead.com Notify",
            //                Subject = "You received a tip!",
            //            }, 
            //            "Notify", // account
            //            true // useSSL
            //            ));
            //    }
            //    await db.SaveChangesAsync().ConfigureAwait(true);
            //    return Json(new { success = true, Result = "Success" });
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected async Task<List<Post>> GetPosts(int start, int count, int userId = 0)
        {
            using (var db = new ZapContext())
            {
                var u = await db.Users
                        .AsNoTracking()
                        .Where(us => us.Id == userId).FirstOrDefaultAsync();

                if (u == null)
                {
                    return new List<Post>();
                }

                // These are the user ids which we are following
                var followingIds = u.Following.Select(f => f.Id).ToList();// db.Users.Where(usr => usr.Id == u.Id).Select(us => us.Id).ToList();

                // Our posts
                var userposts = db.Posts.Where(p => p.UserId.Id == u.Id)
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft)
                    .OrderByDescending(p => p.TimeStamp)
                    .Include(p => p.Group)
                    .Include(p => p.Comments)
                    .Include(p => p.Comments.Select(cmt => cmt.Parent))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId.ProfileImage))
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .AsNoTracking().Take(20);

                var userCommentedPosts = db.Comments
                    .Where(c => c.UserId.Id == u.Id)
                    //.Where(c => !c.IsDeleted)
                    .Select(c => c.Post)
                    .Distinct()
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .AsNoTracking().Take(20);

                // Followers posts
                var followposts = db.Posts.Where(p => followingIds.Contains(p.UserId.Id))
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft)
                    .Include(p => p.Group)
                    .Include(p => p.Comments)
                    .Include(p => p.Comments.Select(cmt => cmt.Parent))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId.ProfileImage))
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .AsNoTracking().Take(20);

                var activityposts = await userposts.Union(followposts).Union(userCommentedPosts).OrderByDescending(p => p.TimeStamp)
                    .Skip(start)
                    .Take(count)
                    .ToListAsync().ConfigureAwait(true);

                return activityposts;
            }
        }

        /// <summary>
        /// // GET: /Manage/Index
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [HttpGet]
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            XFrameOptionsDeny();
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userAppId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                // This query returns a single list of objects from the database with required information for the view.  This is much 
                //  faster than returning EF classes.
                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        u.Id,
                        u.AppId,
                        u.ProfileImage.Version,
                        u.AboutMe,
                        u.Name,
                        TopFollowing = u.Following.OrderByDescending(us => us.TotalEarned).Take(20)
                            .Select(us => new UserFollowView()
                            {
                                Name = us.Name,
                                AppId = us.AppId,
                                ProfileImageVersion = us.ProfileImage.Version,
                            }),
                        TopFollowers = u.Followers.OrderByDescending(us => us.TotalEarned).Take(20)
                            .Select(us => new UserFollowView()
                            {
                                Name = us.Name,
                                AppId = us.AppId,
                                ProfileImageVersion = us.ProfileImage.Version,
                            }),
                        UserGroups = u.Groups
                            .Select(g => new GroupInfo()
                            {
                                Id = g.GroupId,
                                Name = g.GroupName,
                                Icon = "fa-bolt",
                                Level = 1,
                                Progress = 36,
                                NumPosts = g.Posts.Where(p => !(p.IsDeleted || p.IsDraft)).Count(),
                                UserPosts = g.Posts.Where(p => !(p.IsDeleted || p.IsDraft)).Where(p => p.UserId.Id == u.Id).Count(),
                                IsMod = g.Moderators.Select(usr => usr.Id).Contains(u.Id),
                                IsAdmin = g.Administrators.Select(usr => usr.Id).Contains(u.Id),
                            }),
                        UserAchievements = u.Achievements.Select(ach => new UserAchievementViewModel()
                            {
                                Id = ach.Id,
                                ImageId = ach.Achievement.Id,
                                Name = ach.Achievement.Name,
                            }),
                        UserIgnoring = u.IgnoringUsers.Select(usr => usr.Id).Where(usrid => usrid != u.Id),
                        ColorTheme = u.Settings == null ? "" : u.Settings.ColorTheme,
                        NumPosts = u.Posts.Where(p => !p.IsDeleted).Where(p => !p.IsDraft).Count(),
                        NumFollowing = u.Following.Count,
                        NumFollowers = u.Followers.Count,
                        UserBalance = u.Funds.Balance,
                        u.Languages,
                        u.Settings,
                        u.Reputation,
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                ValidateClaims(new UserSettings() { ColorTheme = userInfo.ColorTheme });

                //var postViews = await QueryHelpers.QueryActivityPostsVm(0, 10, userInfo.Id).ConfigureAwait(true);

                var uavm = new UserAchievementsViewModel() { 
                    Achievements = userInfo.UserAchievements
                };

                // This is a mess - need to split it up
                var model = new ManageUserViewModel
                {
                    HasPassword = HasPassword(),
                    UserName = userInfo.Name,
                    UserAppId = userInfo.AppId,
                    UserId = userInfo.Id,
                    UserProfileImageVersion = userInfo.Version,
                    TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userAppId).ConfigureAwait(true),
                    EmailConfirmed = await UserManager.IsEmailConfirmedAsync(userAppId).ConfigureAwait(true),
                    AboutMe = new AboutMeViewModel() { AboutMe = userInfo.AboutMe == null ? "Nothing to tell." : userInfo.AboutMe },
                    UserGroups = new ManageUserGroupsViewModel() { Groups = userInfo.UserGroups },
                    NumPosts = userInfo.NumPosts,
                    NumFollowers = userInfo.NumFollowers,
                    NumFollowing = userInfo.NumFollowing,
                    IsFollowing = true, // Not actually used here
                    //ActivityPosts = postViews,
                    TopFollowingVm = userInfo.TopFollowing,
                    TopFollowersVm = userInfo.TopFollowers,
                    UserBalance = userInfo.UserBalance,
                    AchievementsViewModel = uavm,
                    Settings = userInfo.Settings,
                    Languages = userInfo.Languages == null ? new List<string>() : userInfo.Languages.Split(',').ToList(),
                    KnownLanguages = LanguageHelpers.GetLanguages(),
                    Reputation = userInfo.Reputation,
                };

                return View(model);
            }
        }

        /// <summary>
        /// Called when initially loading the activity posts
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetActivityPosts()
        {
            var userAppId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var postViews = await QueryHelpers.QueryActivityPostsVm(
                    start: 0, 
                    count: 10,
                    userAppId: userAppId).ConfigureAwait(true);

                var vm = new ManageActivityPostsPartialViewModel()
                {
                    ActivityPosts = postViews
                };

                return PartialView("_PartialManageActivityPosts", vm);
            }
        }

        private void ValidateClaims(UserSettings userSettings)
        {
            try
            {
                User.AddUpdateClaim("ColorTheme", userSettings.ColorTheme ?? "light");
            }
            catch (Exception)
            {
                //TODO: handle (or fix test for HttpContext.Current.GetOwinContext().Authentication mocking)
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languages"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateUserLanguages(string languages)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return Json(new { result = "error", success = false, message = "User not found" });
            }
            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Where(u => u.AppId == userId)
                    .SingleOrDefault();

                if (user == null)
                {
                    return Json(new { result = "error", success = false, message = "User not found" });
                }

                user.Languages = languages;

                db.SaveChanges();

                return Json(new { result = "success", success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult =
                ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext
                (ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// // POST: /Manage/RemoveLogin
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        /// <summary>
        /// // GET: /Manage/AddPhoneNumber
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddPhoneNumber()
        {
            XFrameOptionsDeny();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateAboutMe(AboutMeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                await EnsureUserExists(userId, db);

                db.Users.Where(u => u.AppId == userId).First().AboutMe = model.AboutMe.CleanUnicode().SanitizeXSS();
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { Message = ManageMessageId.UpdateAboutMeSuccess });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public async Task<JsonResult> RotateProfileImage()
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, message = "User not authorized." });
            }

            using (var db = new ZapContext())
            {
                var userImage = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.ProfileImage)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var img = await db.Images
                    .Where(i => i.UserAppId == userAppId)
                    .Where(i => i.Version == userImage.Version)
                    .Where(i => i.ImageId == userImage.ImageId)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(true);

                if (img == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { success = false, message = "Image not found." });
                }

                using (var ms = new MemoryStream(img.Image))
                {
                    Image png = Image.FromStream(ms);
                    png.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    byte[] data = png.ToByteArray(ImageFormat.Png);

                    img.Image = data;
                    img.Version += 1;

                    await db.SaveChangesAsync();

                    return Json(new { success = true, version = img.Version });
                }
            }
        }

        /// <summary>
        /// Updates the user profile image
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<JsonResult> UpdateProfileImage(HttpPostedFileBase file)
        {
            if (file == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "No file uploaded." });
            }

            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    Image img = Image.FromStream(file.InputStream);

                    // Images should retain aspect ratio
                    double ar = Convert.ToDouble(img.Width) / Convert.ToDouble(img.Height); // Aspect ratio
                    int max_wh = 512; // desired max width or height
                    int newHeight = img.Height;
                    int newWidth = img.Width;
                    if (img.Height > img.Width)
                    {
                        newHeight = max_wh;
                        newWidth = Convert.ToInt32(Convert.ToDouble(max_wh) * ar);
                    }
                    else
                    {
                        newWidth = max_wh;
                        newHeight = Convert.ToInt32(Convert.ToDouble(max_wh) / ar);
                    }

                    var bmp = new Bitmap(max_wh, max_wh);
                    var graph = Graphics.FromImage(bmp);
                    var brush = new SolidBrush(Color.Transparent);

                    graph.InterpolationMode = InterpolationMode.High;
                    graph.CompositingQuality = CompositingQuality.HighQuality;
                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    graph.FillRectangle(brush, new RectangleF(0, 0, max_wh, max_wh));
                    graph.DrawImage(img, ((int)max_wh - newWidth) / 2, ((int)max_wh - newHeight) / 2, newWidth, newHeight);
                    byte[] data = bmp.ToByteArray(ImageFormat.Png);

                    var user = db.Users.First(u => u.AppId == userId);
                    UserImage i = new UserImage() { 
                        UserAppId = user.AppId,
                        ContentType = "image/png",
                        XSize = max_wh,
                        YSize = max_wh,
                        Image = data, 
                        Version = user.ProfileImage.Version + 1 };
                    user.ProfileImage = i;
                    await db.SaveChangesAsync().ConfigureAwait(false);
                    return Json(new { success = true, result = "success", version = i.Version });
                }
                else
                {
                    return Json(new { success = false, message = "Image not received by server" });
                }
            }
        }

        /// <summary>
        /// Updates the user alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<JsonResult> UpdateUserAlias(string alias)
        {
            try
            {
                if (!Request.IsAuthenticated)
                {
                    return Json(new { success = false, result = "Failure", message = "You do not have the required privilages to do this." });
                }
            }
            catch
            {
                // This will happen with unit test - should fix it.
            }

            string cleanName = alias.CleanUnicode().Trim().SanitizeXSS();

            // This is how long the name is if it was printed out
            string printingName = cleanName.RemoveUnicodeNonPrinting();
            if (printingName.Length < 2)
            {
                return Json(new { success = false, result = "Failure", message = "Username must be at least 2 (printed) characters long." });
            }

            if (printingName.Contains("/") 
                || printingName.Contains(@"\") 
                || printingName.Contains("<")
                || printingName.Contains(">")
                || printingName.Contains("*")
                || printingName.Contains("%")
                || printingName.Contains(":"))
            {
                return Json(new { success = false, result = "Failure", message = @"The characters / \ < > * % & : are not permitted." });
            }

            // Check for spaces in username
            foreach (char c in cleanName)
            {
                if (Char.IsWhiteSpace(c))
                {
                    return Json(new { success = false, result = "Failure", message = "Username cannot contain spaces." });
                }
            }

            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return Json(new { success = false, result = "Failure", message = "Error updating user settings." });
            }

            string oldName = "";

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Include("Settings")
                    .Where(u => u.AppId == userId).First();

                var otherUser = await UserManager.FindByNameAsync(cleanName);

                if (otherUser != null)
                {
                    return Json(new { success = false, result = "Failure", message = "Username already taken." });
                }

                var aspUser = await UserManager.FindByIdAsync(userId);
                oldName = aspUser.UserName;
                aspUser.UserName = cleanName;
                await UserManager.UpdateAsync(aspUser);
                await SignInManager.SignInAsync(aspUser, true, true);
                user.Name = cleanName;
                await db.SaveChangesAsync();

                // Send a security notification to user
                var mailer = DependencyResolver.Current.GetService<MailerController>();
                await SendUpdateUserAliasEmailNotification(cleanName, oldName, user, aspUser, mailer);

                return Json(new { success = true, result = "Success" });
            }
        }

        private async Task SendUpdateUserAliasEmailNotification(string cleanName, string oldName, User user, ApplicationUser aspUser, MailerController mailer)
        {
            // Sets the mailer controller context for views to be rendered.
            mailer.ControllerContext = new ControllerContext(this.Request.RequestContext, mailer);

            string subject = "Your Zapread Username has been updated";
            string emailBody = await mailer.GenerateUpdatedUserAliasEmailBod(
                id: user.Id,
                userName: cleanName,
                oldUserName: oldName).ConfigureAwait(true);

            string userEmail = aspUser.Email;

            // Enqueue emails for sending out.  Don't need to wait for this to finish before returning client response
            BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                new UserEmailModel()
                {
                    Destination = userEmail,
                    Body = emailBody,
                    Email = "",
                    Name = "zapread.com",
                    Subject = subject,
                }, 
                "Notify", // account
                true // useSSL
                ));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> UpdateUserSetting(string setting, bool value)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return Json(new { success = false, result = "error", message = "Error updating user settings" });
            }
            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include("Settings")
                    .FirstOrDefaultAsync(u => u.AppId == userId);

                if (user.Settings == null)
                {
                    user.Settings = new UserSettings();
                }

                // These are the possible settings
                switch (setting)
                {
                    case "notifyPost":
                        user.Settings.NotifyOnOwnPostCommented = value;
                        break;
                    case "emailTwoFactor":
                        await UserManager.SetTwoFactorEnabledAsync(userId, value);
                        break;
                    default:
                        break;
                }

                if (setting == "notifyComment")
                {
                    user.Settings.NotifyOnOwnCommentReplied = value;
                }
                else if (setting == "notifyNewPostGroup")
                {
                    user.Settings.NotifyOnNewPostSubscribedGroup = value;
                }
                else if (setting == "notifyNewPostUser")
                {
                    user.Settings.NotifyOnNewPostSubscribedUser = value;
                }
                else if (setting == "notifyPrivateMessage")
                {
                    user.Settings.NotifyOnPrivateMessage = value;
                }
                else if (setting == "notifyReceivedTip")
                {
                    user.Settings.NotifyOnReceivedTip = value;
                }
                else if (setting == "notifyMentioned")
                {
                    user.Settings.NotifyOnMentioned = value;
                }
                else if (setting == "alertPost")
                {
                    user.Settings.AlertOnOwnPostCommented = value;
                }
                else if (setting == "alertComment")
                {
                    user.Settings.AlertOnOwnCommentReplied = value;
                }
                else if (setting == "alertNewPostGroup")
                {
                    user.Settings.AlertOnNewPostSubscribedGroup = value;
                }
                else if (setting == "alertNewPostUser")
                {
                    user.Settings.AlertOnNewPostSubscribedUser = value;
                }
                else if (setting == "alertPrivateMessage")
                {
                    user.Settings.AlertOnPrivateMessage = value;
                }
                else if (setting == "alertReceivedTip")
                {
                    user.Settings.AlertOnReceivedTip = value;
                }
                else if (setting == "alertMentioned")
                {
                    user.Settings.AlertOnMentioned = value;
                }
                else if (setting == "colorTheme")
                {
                    user.Settings.ColorTheme = value ? "dark" : "light";
                    User.AddUpdateClaim("ColorTheme", user.Settings.ColorTheme);
                }
                else if (setting == "ViewAllLanguages")
                {
                    user.Settings.ViewAllLanguages = value;
                }
                else if (setting == "ViewTranslatedLanguages")
                {
                    user.Settings.ViewTranslatedLanguages = value;
                }
                else if (setting == "showOnline")
                {
                    user.Settings.ShowOnline = value;
                }
                else if (setting == "showTours")
                {
                    user.Settings.ShowTours = value;
                }

                await db.SaveChangesAsync();
                return Json(new { success = true, result = "success" });
            }
        }

        private async Task EnsureUserExists(string userId, ZapContext db)
        {
            if (db.Users.Where(u => u.AppId == userId).Count() == 0)
            {
                // no user entry
                User u = new User()
                {
                    AboutMe = "Nothing to tell.",
                    AppId = userId,
                    Name = UserManager.FindById(userId).UserName,
                    ProfileImage = new UserImage(),
                    ThumbImage = new UserImage(),
                    Funds = new UserFunds(),
                    Settings = new UserSettings(),
                    DateJoined = DateTime.UtcNow,
                };
                db.Users.Add(u);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// // POST: /Manage/AddPhoneNumber
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        /// <summary>
        /// // POST: /Manage/EnableTwoFactorAuthentication
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        /// <summary>
        /// // POST: /Manage/DisableTwoFactorAuthentication
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        /// <summary>
        /// // GET: /Manage/VerifyPhoneNumber
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            XFrameOptionsDeny();
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        /// <summary>
        /// // POST: /Manage/VerifyPhoneNumber
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        /// <summary>
        /// // POST: /Manage/RemovePhoneNumber
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        /// <summary>
        /// // GET: /Manage/ChangePassword
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// // POST: /Manage/ChangePassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        /// <summary>
        /// // GET: /Manage/SetPassword
        /// </summary>
        /// <returns></returns>
        public ActionResult SetPassword()
        {
            return View();
        }

        /// <summary>
        /// // POST: /Manage/SetPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// // GET: /Manage/ManageLogins
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        /// <summary>
        /// POST: /Manage/LinkLogin
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        /// <summary>
        /// // GET: /Manage/LinkLoginCallback
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            XFrameOptionsDeny();
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId()).ConfigureAwait(true);
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        /// <summary>
        /// Not sure if these are still going to be used - mark for refactor.
        /// </summary>
        public enum ManageMessageId
        {
            /// <summary>
            /// 
            /// </summary>
            AddPhoneSuccess,
            /// <summary>
            /// 
            /// </summary>
            ChangePasswordSuccess,
            /// <summary>
            /// 
            /// </summary>
            SetTwoFactorSuccess,
            /// <summary>
            /// 
            /// </summary>
            SetPasswordSuccess,
            /// <summary>
            /// 
            /// </summary>
            RemoveLoginSuccess,
            /// <summary>
            /// 
            /// </summary>
            RemovePhoneSuccess,
            /// <summary>
            /// 
            /// </summary>
            UpdateAboutMeSuccess,
            /// <summary>
            /// 
            /// </summary>
            Error
        }

        #endregion
    }
}
