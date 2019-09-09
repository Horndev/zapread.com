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
using zapread.com.Models.Database;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

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

        public ActionResult Financial()
        {
            return View();
        }

        public PartialViewResult WithdrawModal()
        {
            return PartialView("_PartialModalWithdraw");
        }

        // https://gist.github.com/ChinhP/9b4dc1df1b12637b99a420aa268ae32b
        // https://www.codeproject.com/Tips/1011531/Using-jQuery-DataTables-with-Server-Side-Processin

        public class DataItem
        {
            public string Time { get; set; }
            public string Type { get; set; }
            public string Amount { get; set; }
            public string URL { get; set; }
            public string Memo { get; set; }
        }

        public async Task<ActionResult> GetLNTransactions(DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                User u;
                u = await db.Users
                        .Include(usr => usr.LNTransactions)
                        .Where(us => us.AppId == userId)
                        .SingleOrDefaultAsync();

                var pageTxns = u.LNTransactions
                    .AsParallel()
                    .Where(tx => tx.TimestampSettled != null)
                    .OrderByDescending(tx => tx.TimestampSettled)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToList();

                var values = pageTxns.AsParallel()
                    .Select(t => new DataItem()
                    {
                        Time = t.TimestampSettled.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        Type = t.IsDeposit ? "Deposit" : "Withdrawal",
                        Amount = Convert.ToString(t.Amount),
                        Memo = t.Memo,
                    }).ToList();

                int numrec = u.LNTransactions.Where(tx => tx.TimestampSettled != null).Count();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values
                };
                return Json(ret);
            }
        }

        public async Task<ActionResult> GetEarningEvents(DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var pageEarnings = await db.Users
                    .Where(us => us.AppId == userId)
                    .SelectMany(us => us.EarningEvents)
                    .OrderByDescending(e => e.TimeStamp)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToListAsync();





                var commentIds = pageEarnings
                    .Where(e => e.Type == 0 && e.OriginType == 1)
                    .Select(e => Convert.ToInt64(e.OriginId)).ToList();

                var groupIds = pageEarnings
                    .Where(e => e.Type == 1)
                    .Select(e => e.OriginId).ToList();

                var comments = await db.Comments
                    .Include(c => c.Post)
                    .Where(c => commentIds.Contains(c.CommentId)).ToListAsync();

                var postIds = pageEarnings
                    .Where(e => e.Type == 0 && e.OriginType == 0)
                    .Select(e => e.OriginId).ToList();

                var cids = comments.Where(c => c.Post != null)
                    .Select(c => c.Post.PostId);

                postIds = postIds.Union(cids).ToList();

                var posts = await db.Posts.Where(p => postIds.Contains(p.PostId)).ToListAsync();
                var groups = await db.Groups.Where(g => groupIds.Contains(g.GroupId)).ToListAsync();


                var values = pageEarnings
                    .AsParallel()
                    .Select(t => new DataItem()
                    {
                        Time = t.TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        Amount = t.Amount.ToString("0.##"),
                        Type = t.Type == 0 ? (t.OriginType == 0 ? "Post" : t.OriginType == 1 ? "Comment" : t.OriginType == 2 ? "Tip" : "Unknown") : t.Type == 1 ? "Group" : t.Type == 2 ? "Community" : "Unknown",
                        URL = GetEarningURL(t, commentIds, comments),
                        Memo = GetEarningMemo(t, groupIds, groups, postIds, posts, commentIds, comments),
                    }).ToList();

                //var postEarning = await db.Users
                //    .Where(us => us.AppId == userId)
                //    .SelectMany(us => us.EarningEvents)
                //    .OrderByDescending(e => e.TimeStamp)
                //    .Skip(dataTableParameters.Start)
                //    .Take(dataTableParameters.Length)
                //    .Where(t => t.Type == 0 && t.OriginType == 0)
                //    .Join(
                //        inner: db.Posts, 
                //        outerKeySelector: e => e.OriginId, 
                //        innerKeySelector: p => p.PostId,
                //        resultSelector: (e, p) => new { e.TimeStamp, e.Amount, p.PostId, p.PostTitle })
                //    .ToListAsync();

                //var postValues = postEarning
                //    .AsParallel()
                //    .Select(t => new DataItem()
                //    {
                //        Time = t.TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                //        Amount = t.Amount.ToString("0.##"),
                //        Type = "Post",
                //        URL = Url.Action(actionName: "Detail", controllerName: "Post", routeValues: new { id = t.PostId }),
                //        Memo = t.PostTitle,
                //    }).ToList();

                int numrec = await db.Users
                    .Where(us => us.AppId == userId)
                    .SelectMany(us => us.EarningEvents)
                    .CountAsync();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values
                };
                return Json(ret);
            }
        }

        private string GetEarningURL(EarningEvent t, List<long> commentIds, List<Comment> comments)
        {
            if (t.Type == 1)
            {
                if (t.OriginId > 0)
                    return Url.Action(controllerName: "Group", actionName: "GroupDetail", routeValues: new { id = t.OriginId });
            }
            else if (t.Type == 0 && t.OriginType == 0)
            {
                if (t.OriginId > 0)
                    return Url.Action(controllerName: "Post", actionName: "Detail", routeValues: new { id = t.OriginId });
            }
            else if (t.Type == 0 && t.OriginType == 1)
            {
                var postId = commentIds.Contains(t.OriginId) ? comments.FirstOrDefault(c => c.CommentId == t.OriginId)?.Post.PostId : 0;
                if (postId > 0)
                    return Url.Action(controllerName: "Post", actionName: "Detail", routeValues: new { id = postId });
            }
            return t.OriginId.ToString();
        }

        private static string GetEarningMemo(EarningEvent t, List<int> groupIds, List<Group> groups, List<int> postIds, List<Post> posts, List<long> commentIds, List<Comment> comments)
        {
            if (t.Type == 1)
            {
                if (t.OriginId > 0)
                    return groupIds.Contains(t.OriginId) ? groups.FirstOrDefault(g => g.GroupId == t.OriginId)?.GroupName : "";
            }
            else if (t.Type == 0 && t.OriginType == 0)
            {
                string memo = postIds.Contains(t.OriginId) ? posts.FirstOrDefault(p => p.PostId == t.OriginId)?.PostTitle : "";
                if (memo == null)
                    memo = "";
                if (memo.Length > 33)
                    memo = memo.Substring(0, 30) + "...";
                return memo;
            }
            else if (t.Type == 0 && t.OriginType == 1) // Comment
            {
                if (t.OriginId > 0)
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
            }
            return "";
        }

        public async Task<ActionResult> GetSpendingEvents(DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                int numrec = await db.Users
                    .Where(us => us.AppId == userId)
                    .SelectMany(usr => usr.SpendingEvents)
                    .CountAsync();

                var pageSpendings = await db.Users
                        .Include(usr => usr.LNTransactions)
                        .Include(usr => usr.SpendingEvents)
                        .Include(usr => usr.SpendingEvents.Select(s => s.Post))
                        .Include(usr => usr.SpendingEvents.Select(s => s.Group))
                        .Include(usr => usr.SpendingEvents.Select(s => s.Comment))
                        .Include(usr => usr.SpendingEvents.Select(s => s.Comment).Select(c => c.Post))
                        .Where(us => us.AppId == userId)
                        .SelectMany(usr => usr.SpendingEvents)
                        .OrderByDescending(e => e.TimeStamp)
                        .Skip(dataTableParameters.Start)
                        .Take(dataTableParameters.Length)
                        .ToListAsync();

                var values = pageSpendings.AsParallel()
                    .Select(t => new DataItem()
                    {
                        Time = t.TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        Type = t.Post != null ? "Post " + t.Post.PostId.ToString() : (
                            t.Comment != null ? "Comment " + t.Comment.CommentId.ToString() : (
                            t.Group != null ? "Group " + t.Group.GroupId.ToString() :
                            "Other")),
                        Amount = Convert.ToString(t.Amount),
                        URL = t.Post != null ? Url.Action("Detail", "Post") + "/" + Convert.ToString(t.Post.PostId) : (
                            t.Comment != null ? Url.Action("Detail", "Post") + "/" + t.Comment.Post.PostId.ToString() : (
                            t.Group != null ? Url.Action("Index", "Group") :
                            "")),
                    }).ToList();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values
                };
                return Json(ret);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> TipUser(int id, int? amount, int? tx)
        {
            using (var db = new ZapContext())
            {
                var receiver = await db.Users
                    .Include(usr => usr.Funds)
                    .Include(usr => usr.EarningEvents)
                    .Include(usr => usr.Settings)
                    .Where(u => u.Id == id).FirstOrDefaultAsync();

                if (receiver == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { Result = "Failure", Message = "User not found." });
                }

                if (tx == null)
                {
                    var userId = User.Identity.GetUserId();
                    if (userId == null)
                    {
                        return Json(new { Result = "Failure", Message = "User not found." });
                    }

                    var user = await db.Users
                        .Include(usr => usr.Funds)
                        .Include(usr => usr.SpendingEvents)
                        .Where(u => u.AppId == userId).FirstOrDefaultAsync();

                    // Ensure user has the funds available.
                    if (user.Funds.Balance < amount)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return Json(new { Result = "Failure", Message = "Not enough funds." });
                    }

                    // All requirements are met - make payment
                    user.Funds.Balance -= amount.Value;
                    receiver.Funds.Balance += amount.Value;

                    // Add Earning Event

                    var ea = new EarningEvent()
                    {
                        Amount = amount.Value,
                        OriginType = 2,
                        TimeStamp = DateTime.UtcNow,
                        Type = 0,
                    };

                    receiver.EarningEvents.Add(ea);

                    var spendingEvent = new SpendingEvent()
                    {
                        Amount = amount.Value,
                        TimeStamp = DateTime.UtcNow,
                    };

                    user.SpendingEvents.Add(spendingEvent);

                    // Notify receiver
                    var alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "You received a tip!",
                        Content = "From: <a href='" + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = user.Name }) + "'>" + user.Name + "</a><br/> Amount: " + amount.ToString() + " Satoshi.",
                        IsDeleted = false,
                        IsRead = false,
                        To = receiver,
                    };

                    receiver.Alerts.Add(alert);
                    await db.SaveChangesAsync();

                    try
                    {
                        if (receiver.Settings == null)
                        {
                            receiver.Settings = new UserSettings();
                        }

                        if (receiver.Settings.NotifyOnReceivedTip)
                        {
                            string receiverEmail = UserManager.FindById(receiver.AppId).Email;
                            MailingService.Send(user: "Notify",
                                message: new UserEmailModel()
                                {
                                    Subject = "You received a tip!",
                                    Body = "From: " + user.Name + "<br/> Amount: " + amount.ToString() + " Satoshi.<br/><br/><a href='http://www.zapread.com'>zapread.com</a>",
                                    Destination = receiverEmail,
                                    Email = "",
                                    Name = "ZapRead.com Notify"
                                });
                        }
                    }
                    catch (Exception e)
                    {
                        // Send an email.
                        MailingService.Send(new UserEmailModel()
                        {
                            Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                            Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n user: " + userId,
                            Email = "",
                            Name = "zapread.com Exception",
                            Subject = "Send NotifyOnReceivedTip error.",
                        });
                    }

                    return Json(new { Result = "Success" });
                }
                else
                {
                    // Anonymous tip

                    var vtx = await db.LightningTransactions.FirstOrDefaultAsync(txn => txn.Id == tx);

                    if (vtx == null || vtx.IsSpent == true)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return Json(new { Result = "Failure", Message = "Transaction not found" });
                    }

                    vtx.IsSpent = true;

                    receiver.Funds.Balance += amount.Value;// vtx.Amount;//amount;

                    // Notify receiver
                    var alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "You received a tip!",
                        Content = "From: anonymous <br/> Amount: " + amount.ToString() + " Satoshi.",
                        IsDeleted = false,
                        IsRead = false,
                        To = receiver,
                    };

                    receiver.Alerts.Add(alert);
                    await db.SaveChangesAsync();

                    if (receiver.Settings == null)
                    {
                        receiver.Settings = new UserSettings();
                    }

                    if (receiver.Settings.NotifyOnReceivedTip)
                    {
                        string receiverEmail = UserManager.FindById(receiver.AppId).Email;
                        MailingService.Send(user: "Notify",
                            message: new UserEmailModel()
                            {
                                Subject = "You received a tip!",
                                Body = "From: anonymous <br/> Amount: " + amount.ToString() + " Satoshi.<br/><br/><a href='http://www.zapread.com'>zapread.com</a>",
                                Destination = receiverEmail,
                                Email = "",
                                Name = "ZapRead.com Notify"
                            });
                    }
                    return Json(new { Result = "Success" });
                }
            }
        }

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
                    .Include("UserId")
                    .AsNoTracking().Take(20);

                var userCommentedPosts = db.Comments
                    .Where(c => c.UserId.Id == u.Id)
                    //.Where(c => !c.IsDeleted)
                    .Select(c => c.Post)
                    .Distinct()
                    .Include("UserId")
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
                    .Include("UserId")
                    .AsNoTracking().Take(20);

                var activityposts = await userposts.Union(followposts).Union(userCommentedPosts).OrderByDescending(p => p.TimeStamp)
                    .Skip(start)
                    .Take(count)
                    .ToListAsync();

                return activityposts;
            }
        }


        //
        // GET: /Manage/Index
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                string aboutMe = "Nothing to tell.";
                User u;
                if (await db.Users.Where(us => us.AppId == userId).CountAsync() == 0)
                {
                    // no user entry
                    u = new User()
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
                else
                {
                    u = db.Users
                        .Include(usr => usr.LNTransactions)
                        .Include(usr => usr.EarningEvents)
                        .Include(usr => usr.IgnoringUsers)
                        .Include(usr => usr.Funds)
                        .Include(usr => usr.Settings)
                        .AsNoTracking()
                        .Where(us => us.AppId == userId).First();
                    aboutMe = u.AboutMe;
                }

                ValidateClaims(u);

                ViewBag.UserName = u.Name;
                ViewBag.UserId = u.Id;

                if (u.Settings == null)
                {
                    u.Settings = new UserSettings();
                }

                var activityposts = await GetPosts(0, 10, userId: u.Id);
                List<LNTxViewModel> txnView = GetRecentTransactions(u);
                List<SpendingsViewModel> spendingsView = GetRecentSpending(u);
                List<EarningsViewModel> earningsView = GetRecentEarnings(u);
                List<GroupInfo> gi = GetUserGroups(u);
                int numUserPosts = await db.Posts.Where(p => p.UserId.AppId == userId).CountAsync();
                int numFollowers = await db.Users.Where(p => p.Following.Select(f => f.Id).Contains(u.Id)).CountAsync();
                int numFollowing = u.Following.Count();
                bool isFollowing = false;
                var topFollowing = u.Following.OrderByDescending(us => us.TotalEarned).Take(20).ToList();
                var topFollowers = u.Followers.OrderByDescending(us => us.TotalEarned).Take(20).ToList();
                List<int> viewerIgnoredUsers = GetUserIgnored(u);
                List<PostViewModel> postViews = await GetUserActivtiesView(db, u, activityposts, viewerIgnoredUsers);
                List<string> languages = GetLanguages();

                var model = new ManageUserViewModel
                {
                    HasPassword = HasPassword(),
                    User = u,
                    PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                    TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                    EmailConfirmed = await UserManager.IsEmailConfirmedAsync(userId),
                    Logins = await UserManager.GetLoginsAsync(userId),
                    //BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),  // This is an extension method which can't be mocked for test
                    AboutMe = new AboutMeViewModel() { AboutMe = aboutMe },
                    Financial = new FinancialViewModel() { Transactions = txnView, Earnings = earningsView, Spendings = spendingsView },
                    UserGroups = new ManageUserGroupsViewModel() { Groups = gi },
                    NumPosts = numUserPosts,
                    NumFollowers = numFollowers,
                    NumFollowing = numFollowing,
                    IsFollowing = isFollowing,
                    ActivityPosts = postViews,
                    TopFollowing = topFollowing,
                    TopFollowers = topFollowers,
                    UserBalance = u.Funds.Balance,
                    Settings = u.Settings,
                    Languages = u.Languages == null ? new List<string>() : u.Languages.Split(',').ToList(),
                    KnownLanguages = languages,
                };

                return View(model);
            }
        }

        private static List<int> GetUserIgnored(User u)
        {
            List<int> viewerIgnoredUsers = new List<int>();

            if (u != null && u.IgnoringUsers != null)
            {
                viewerIgnoredUsers = u.IgnoringUsers.Select(usr => usr.Id).Where(usrid => usrid != u.Id).ToList();
            }

            return viewerIgnoredUsers;
        }

        private static async Task<List<PostViewModel>> GetUserActivtiesView(ZapContext db, User u, List<Post> activityposts, List<int> viewerIgnoredUsers)
        {
            List<PostViewModel> postViews = new List<PostViewModel>();
            var groups = await db.Groups
                    .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                    .AsNoTracking()
                    .ToListAsync();

            foreach (var p in activityposts)
            {
                postViews.Add(new PostViewModel()
                {
                    Post = p,
                    ViewerIsMod = u != null ? u.GroupModeration.Contains(p.Group) : false,
                    ViewerUpvoted = u != null ? u.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    ViewerDownvoted = u != null ? u.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    NumComments = 0,
                    ViewerIgnoredUsers = viewerIgnoredUsers,
                    GroupMemberCounts = groups.ToDictionary(i => i.GroupId, i => i.mc),
                    GroupPostCounts = groups.ToDictionary(i => i.GroupId, i => i.pc),
                    GroupLevels = groups.ToDictionary(i => i.GroupId, i => i.l),
                });
            }

            return postViews;
        }

        private static List<LNTxViewModel> GetRecentTransactions(User u)
        {
            // Get record of recent LN transactions
            var recentTxs = u.LNTransactions
                .Where(tx => tx.TimestampSettled != null)
                .OrderByDescending(tx => tx.TimestampSettled)
                .Take(5);

            var txnView = new List<LNTxViewModel>();

            foreach (var tx in recentTxs)
            {
                txnView.Add(new LNTxViewModel()
                {
                    Timestamp = (DateTime)tx.TimestampSettled,
                    Type = tx.IsDeposit ? "Deposit" : "Withdrawal",
                    Value = tx.Amount,
                });
            }

            return txnView;
        }

        private static List<SpendingsViewModel> GetRecentSpending(User u)
        {
            var recentSpendings = u.SpendingEvents
                                .OrderByDescending(s => s.TimeStamp)
                                .Take(5);

            var spendingsView = new List<SpendingsViewModel>();

            foreach (var s in recentSpendings)
            {
                string link = "";
                if (s.Post != null)
                {
                    link = "Post " + s.Post.PostId.ToString();
                }
                else if (s.Comment != null)
                {
                    link = "Comment " + s.Comment.CommentId.ToString();
                }
                else if (s.Group != null)
                {
                    link = "Group " + s.Group.GroupId.ToString();
                }

                spendingsView.Add(new SpendingsViewModel()
                {
                    TimeStamp = s.TimeStamp.Value,
                    Value = Convert.ToString(s.Amount),
                    Link = link,
                });
            }

            return spendingsView;
        }

        private static List<EarningsViewModel> GetRecentEarnings(User u)
        {
            var earningsView = new List<EarningsViewModel>();

            var recentEarnings = u.EarningEvents
                .OrderByDescending(e => e.TimeStamp)
                .Take(5);

            foreach (var e in recentEarnings)
            {
                earningsView.Add(new EarningsViewModel()
                {
                    TimeStamp = e.TimeStamp.Value,
                    Value = e.Amount.ToString("0.#"),
                    Type = e.Type == 0 ? (e.OriginType == 0 ? "Post" : "Comment") : e.Type == 1 ? "Group" : e.Type == 2 ? "Community" : "Unknown",
                    ItemId = 0,
                });
            }

            return earningsView;
        }

        private static List<GroupInfo> GetUserGroups(User u)
        {
            var gi = new List<GroupInfo>();

            var userGroups = u.Groups.ToList();

            foreach (var g in userGroups)
            {
                gi.Add(new GroupInfo()
                {
                    Id = g.GroupId,
                    Name = g.GroupName,
                    Icon = "fa-bolt",
                    Level = 1,
                    Progress = 36,
                    NumPosts = g.Posts.Count(),
                    UserPosts = g.Posts.Where(p => p.UserId.Id == u.Id).Count(),
                    IsMod = g.Moderators.Contains(u),
                    IsAdmin = g.Administrators.Contains(u),
                });
            }

            return gi;
        }

        private static List<string> GetLanguages()
        {
            // List of languages known
            var languagesEng = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                .GroupBy(ci => ci.TwoLetterISOLanguageName)
                .Select(g => g.First())
                .Select(ci => ci.Name + ":" + ci.EnglishName).ToList();

            var languagesNat = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                .GroupBy(ci => ci.TwoLetterISOLanguageName)
                .Select(g => g.First())
                .Select(ci => ci.Name + ":" + ci.NativeName).ToList();

            var languages = languagesEng.Concat(languagesNat).ToList();
            return languages;
        }

        private void ValidateClaims(User u)
        {
            try
            {
                User.AddUpdateClaim("ColorTheme", u.Settings.ColorTheme ?? "light");
            }
            catch (Exception)
            {
                //TODO: handle (or fix test for HttpContext.Current.GetOwinContext().Authentication mocking)
            }
        }

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

        [HttpPost]
        public async Task<ActionResult> InfiniteScroll(int BlockNumber)
        {
            int BlockSize = 10;

            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.AppId == uid);

                var posts = await GetPosts(BlockNumber, BlockSize, user != null ? user.Id : 0);

                string PostsHTMLString = "";

                var groups = await db.Groups
                        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .AsNoTracking()
                        .ToListAsync();

                foreach (var p in posts)
                {
                    var pvm = new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        NumComments = 0,
                        GroupMemberCounts = groups.ToDictionary(i => i.GroupId, i => i.mc),
                        GroupPostCounts = groups.ToDictionary(i => i.GroupId, i => i.pc),
                        GroupLevels = groups.ToDictionary(i => i.GroupId, i => i.l),
                    };

                    var PostHTMLString = RenderPartialViewToString("_PartialPostRender", pvm);
                    PostsHTMLString += PostHTMLString;
                }
                return Json(new
                {
                    NoMoreData = posts.Count < BlockSize,
                    HTMLString = PostsHTMLString,
                });
            }
        }

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

        //
        // POST: /Manage/RemoveLogin
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

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

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

                db.Users.Where(u => u.AppId == userId).First().AboutMe = model.AboutMe.CleanUnicode();
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { Message = ManageMessageId.UpdateAboutMeSuccess });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<JsonResult> UpdateProfileImage(HttpPostedFileBase file)
        {
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

                    var bmp = new Bitmap((int)max_wh, (int)max_wh);
                    var graph = Graphics.FromImage(bmp);
                    var brush = new SolidBrush(Color.Transparent);

                    graph.InterpolationMode = InterpolationMode.High;
                    graph.CompositingQuality = CompositingQuality.HighQuality;
                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    graph.FillRectangle(brush, new RectangleF(0, 0, max_wh, max_wh));
                    graph.DrawImage(img, ((int)max_wh - newWidth) / 2, ((int)max_wh - newHeight) / 2, newWidth, newHeight);
                    byte[] data = bmp.ToByteArray(ImageFormat.Png);

                    //await EnsureUserExists(userId, db);
                    UserImage i = new UserImage() { Image = data };
                    db.Users.First(u => u.AppId == userId).ProfileImage = i;
                    await db.SaveChangesAsync();
                }
                return Json(new { result = "success" });
            }
        }

        /// <summary>
        /// Updates the user alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [HttpPost]
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
                ; // Todo - fixup unit test
            }

            string cleanName = alias.CleanUnicode().Trim();

            // This is how long the name is if it was printed out
            string printingName = cleanName.RemoveUnicodeNonPrinting();
            if (printingName.Length < 2)
            {
                return Json(new { success = false, result = "Failure", message = "Username must be at least 2 (printed) characters long." });
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

                // Sets the mailer controller context for views to be rendered.
                mailer.ControllerContext = new ControllerContext(this.Request.RequestContext, mailer);

                string subject = "Your Zapread Username has been updated";
                string emailBody = await mailer.GenerateUpdatedUserAliasEmailBod(
                    id: user.Id, 
                    userName: cleanName,
                    oldUserName: oldName);

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
                    }, "Notify"));

                return Json(new { success = true, result = "Success" });
            }
        }

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

        //
        // POST: /Manage/AddPhoneNumber
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

        //
        // POST: /Manage/EnableTwoFactorAuthentication
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

        //
        // POST: /Manage/DisableTwoFactorAuthentication
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

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
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

        //
        // POST: /Manage/RemovePhoneNumber
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

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
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

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
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

        //
        // GET: /Manage/ManageLogins
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



        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

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

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            UpdateAboutMeSuccess,
            Error
        }

        #endregion
    }
}
