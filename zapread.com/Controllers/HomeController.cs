using HtmlAgilityPack;
using Jdenticon;
using Jdenticon.Rendering;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using System.Data.Entity;
using zapread.com.Services;
using System.Globalization;
using LightningLib.lndrpc;
using zapread.com.Models.Database;
using System.Text;
using System.Data.Entity.SqlServer;

namespace zapread.com.Controllers
{
    public class HomeController : Controller
    {
        [Route("robots.txt", Name = "GetRobotsText"), OutputCache(Duration = 86400)]
        public ContentResult RobotsText()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("user-agent: *");
            stringBuilder.AppendLine("disallow: ");
            //stringBuilder.Append("sitemap: ");
            //stringBuilder.AppendLine(this.Url.RouteUrl("GetSitemapXml", null, this.Request.Url.Scheme).TrimEnd('/'));

            return this.Content(stringBuilder.ToString(), "text/plain", Encoding.UTF8);
        }

        //[Route("sitemap.xml", Name = "GetSitemapXml"), OutputCache(Duration = 86400)]
        //public ContentResult SitemapXml()
        //{
        //}

        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> UserImage(int? size, string UserId)
        {
            if (size == null) size = 100;
            if (UserId != null)
            {
                // use the value passed
            }
            else
            {
                if (User.Identity.IsAuthenticated)
                {
                    UserId = User.Identity.GetUserId();
                }
            }

            // Check for image in DB
            using (var db = new ZapContext())
            {
                var i = await db.Users
                    .Where(u => u.AppId == UserId || u.Name == UserId)
                    .Select(u => u.ProfileImage)
                    .FirstAsync();

                if (i.Image != null)
                {
                    Image png = Image.FromStream(new MemoryStream(i.Image));
                    Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size);
                    byte[] data = thumb.ToByteArray(ImageFormat.Png);
                    return File(data, "image/png");
                }

                // RoboHash
                var user = await db.Users
                    .Where(u => u.AppId == UserId || u.Name == UserId)
                    .FirstOrDefaultAsync();

                // Should generate robohash off the appId if Name was supplied
                if (user != null)
                {
                    UserId = user.AppId;
                }
                var imagesPath = Server.MapPath("~/bin");
                RoboHash.Net.RoboHash.ImageFileProvider = new RoboHash.Net.Internals.DefaultImageFileProvider(
                    basePath: imagesPath);
                var r = RoboHash.Net.RoboHash.Create(UserId);
                using (var image = r.Render(
                    set: null,
                    backgroundSet: RoboHash.Net.RoboConsts.Any,
                    color: null,
                    width: 1024,
                    height: 1024))
                {
                    Bitmap thumb = ImageExtensions.ResizeImage(image, (int)size, (int)size);
                    byte[] data = thumb.ToByteArray(ImageFormat.Png);

                    // Cache to DB at full resolution
                    if (user != null)
                    {
                        Bitmap DBthumb = ImageExtensions.ResizeImage(image, 1024, 1024);
                        byte[] DBdata = DBthumb.ToByteArray(ImageFormat.Png);
                        UserImage img = new UserImage() { Image = DBdata };
                        user.ProfileImage = img;
                        await db.SaveChangesAsync();
                    }
                    return File(data, "image/png");
                }
            }
        }

        protected async Task<List<Post>> GetPosts(int start, int count, string sort = "Score", int userId = 0)
        {
            //Modified reddit-like algorithm
            /*epoch = datetime(1970, 1, 1)

            def epoch_seconds(date):
                td = date - epoch
                return td.days * 86400 + td.seconds + (float(td.microseconds) / 1000000)

            def score(ups, downs):
                return ups - downs

            def hot(ups, downs, date):
                s = score(ups, downs)
                order = log(max(abs(s), 1), 10)
                sign = 1 if s > 0 else -1 if s < 0 else 0
                seconds = epoch_seconds(date) - 1134028003
                return round(sign * order + seconds / 45000, 7)*/

            List<string> userLanguages = GetUserLanguages();

            using (var db = new ZapContext())
            {
                DateTime t = DateTime.Now;

                var user = await db.Users
                    .Include(usr => usr.Settings)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                IQueryable<Post> validposts = QueryValidPosts(userId, userLanguages, db, user);

                switch (sort)
                {
                    case "Score":
                        return await QueryPostsByScore(start, count, validposts);
                    case "Active":
                        return await QueryPostsByActive(start, count, validposts);
                    default:
                        return await QueryPostsByNew(start, count, validposts);
                }
            }
        }

        private static IQueryable<Post> QueryValidPosts(int userId, List<string> userLanguages, ZapContext db, User user)
        {
            IQueryable<Post> validposts;
            if (userId > 0)
            {
                var ig = user.IgnoredGroups.Select(g => g.GroupId);
                validposts = db.Posts.Where(p => !ig.Contains(p.Group.GroupId));

                var allLang = user.Settings.ViewAllLanguages;

                if (!allLang)
                {
                    var languages = user.Languages == null ? new List<string>() { "en" } : user.Languages.Split(',').ToList();
                    validposts = validposts
                        .Where(p => p.Language == null || languages.Contains(p.Language));
                }
            }
            else
            {
                validposts = db.Posts
                    .Where(p => p.Language == null || userLanguages.Contains(p.Language));
            }

            return validposts;
        }

        private static async Task<List<Post>> QueryPostsByNew(int start, int count, IQueryable<Post> validposts)
        {
            return await validposts
                                    .OrderByDescending(p => p.TimeStamp)
                                    .Include(p => p.Group)
                                    .Include(p => p.Comments)
                                    .Include(p => p.Comments.Select(cmt => cmt.Parent))
                                    .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                                    .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                                    .Include(p => p.Comments.Select(cmt => cmt.UserId))
                                    .Include("UserId")
                                    .AsNoTracking()
                                    .Where(p => !p.IsDeleted)
                                    .Where(p => !p.IsDraft)
                                    .Skip(start)
                                    .Take(count)
                                    .ToListAsync();
        }

        private static async Task<List<Post>> QueryPostsByActive(int start, int count, IQueryable<Post> validposts)
        {
            DateTime scoreStart = new DateTime(2018, 07, 01);

            // Use number of comments as score
            var sposts = await validposts
                .Where(p => !p.IsDeleted)
                .Where(p => !p.IsDraft)
                .Select(p => new
                {
                    p,
                    s = (Math.Abs((double)p.Comments.Count) < 1.0 ? 1.0 : 100000.0 * Math.Abs((double)p.Comments.Count)),    // Max (|x|,1)                                                           
                })
                .Select(p => new
                {
                    p.p,
                    order = SqlFunctions.Log10(p.s),
                    sign = p.p.Comments.Count >= 0.0 ? 1.0 : -1.0,                              // Sign of s
                    dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.p.TimeStamp),    // time since start
                })
                .Select(p => new
                {
                    p.p,
                    active = p.sign * p.order + p.dt / 2000000 // Reduced time effect
                })
                .OrderByDescending(p => p.active)
                .Select(p => p.p)
                .Include(p => p.Group)
                .Include(p => p.Comments)
                .Include(p => p.Comments.Select(cmt => cmt.Parent))
                .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                .Include(p => p.Comments.Select(cmt => cmt.UserId))
                .Include("UserId")
                .AsNoTracking()
                .Skip(start)
                .Take(count)
                .ToListAsync();
            return sposts;
        }

        private static async Task<List<Post>> QueryPostsByScore(int start, int count, IQueryable<Post> validposts)
        {
            DateTime scoreStart = new DateTime(2018, 07, 01);

            var sposts = await validposts
                .Where(p => !p.IsDeleted)
                .Where(p => !p.IsDraft)
                .Select(p => new
                {
                    p,
                    // Includes the sum of absolute value of comment scores
                    c = p.Comments.Sum(c => Math.Abs((double)c.Score) < 1.0 ? 1.0 : Math.Abs((double)c.Score))
                })
                .Select(p => new
                {
                    p.p,
                    s = 0.5*p.c + (Math.Abs((double)p.p.Score) < 1.0 ? 1.0 : Math.Abs((double)p.p.Score)),    // Max (|x|,1)                                                           
                })
                .Select(p => new
                {
                    p.p,
                    order = SqlFunctions.Log10(p.s),
                    sign = p.p.Score > 0.0 ? 1.0 : -1.0,                              // Sign of s
                    dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.p.TimeStamp),    // time since start
                })
                .Select(p => new
                {
                    p.p,
                    hot = p.sign * p.order + p.dt / 90000
                })
                .OrderByDescending(p => p.hot)
                .Select(p => p.p)
                .Include(p => p.Group)
                .Include(p => p.Comments)
                .Include(p => p.Comments.Select(cmt => cmt.Parent))
                .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                .Include(p => p.Comments.Select(cmt => cmt.UserId))
                .Include("UserId")
                .AsNoTracking()
                .Skip(start)
                .Take(count)
                .ToListAsync();
            return sposts;
        }

        private List<string> GetUserLanguages()
        {
            List<string> userLanguages;

            try
            {
                userLanguages = Request.UserLanguages.ToList().Select(l => l.Split(';')[0].Split('-')[0]).Distinct().ToList();

                if (userLanguages.Count() == 0)
                {
                    userLanguages.Add("en");
                }
            }
            catch
            {
                userLanguages = new List<string>() { "en" };
            }

            return userLanguages;
        }

        /// <summary>
        /// This method is run once when first launched to ensure database globals are in place.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Install()
        {
            using (var db = new ZapContext())
            {
                var zapreadGlobals = await db.ZapreadGlobals
                    .SingleOrDefaultAsync(i => i.Id == 1);

                // This is run only the first time the app is launched in the database.
                // The global entry should only be created once in the database.
                if (zapreadGlobals == null)
                {
                    // Initialize everything with zeros.
                    db.ZapreadGlobals.Add(new ZapReadGlobals()
                    {
                        Id = 1,
                        CommunityEarnedToDistribute = 0.0,
                        TotalDepositedCommunity = 0.0,
                        TotalEarnedCommunity = 0.0,
                        TotalWithdrawnCommunity = 0.0,
                        ZapReadEarnedBalance = 0.0,
                        ZapReadTotalEarned = 0.0,
                        ZapReadTotalWithdrawn = 0.0,
                        LNWithdraws = new List<LNTransaction>(),
                    });
                    db.SaveChanges();
                }
                return Json(new { result = "success" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DismissTour(int id)
        {
            if (SetOrUpdateUserTourCookie("hide") == "hide")
            {
                return Json(new { success = true, result = "success" });
            }
            return Json(new { success = false, result = "failure setting cookie" });
        }

        /// <summary>
        /// If the user is away for longer than 30 days, it presents the tour again.
        /// value is "hide"     : do not present the tour to the user
        ///          "show"     : present to user
        /// </summary>
        /// <returns></returns>
        private string SetOrUpdateUserTourCookie(string setValue = "", string defaultValue = "")
        {
            string cookieResultValue;

            //Check if user is returning
            if (HttpContext.Request.Cookies["ZapRead.com.Tour"] != null)
            {
                var cookie = HttpContext.Request.Cookies.Get("ZapRead.com.Tour");
                HttpContext.Response.Cookies.Remove("ZapRead.com.Tour");
                cookie.Expires = DateTime.Now.AddDays(365);   //update
                if (setValue != "")
                    cookie.Value = setValue;
                HttpContext.Response.SetCookie(cookie);
                cookieResultValue = cookie.Value;
            }
            else
            {
                HttpCookie cookie = new HttpCookie("ZapRead.com.Tour");
                cookie.Value = defaultValue;
                if (setValue != "")
                    cookie.Value = setValue;
                cookie.Expires = DateTime.Now.AddDays(365);
                HttpContext.Response.Cookies.Remove("ZapRead.com.Tour");
                HttpContext.Response.SetCookie(cookie);
                cookieResultValue = cookie.Value;
            }

            return cookieResultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sort">Score, New</param>
        /// <param name="l"></param>
        /// <param name="g">include subscribed groups null = yes</param>
        /// <param name="f">include subscribed followers null = yes</param>
        /// <param name="p">page</param>
        /// <returns></returns>
        [OutputCache(Duration = 600, VaryByParam = "*", Location=System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> Index(string sort, string l, int? g, int? f)
        {
            //PaymentPoller.Subscribe();
            //LNTransactionMonitor a = new LNTransactionMonitor();
            //a.CheckLNTransactions();

            try
            {
                if (Request.IsAuthenticated && (l == null || l == "" || l == "0"))
                {
                    return RedirectToAction("Index", new { sort, l = "1", g, f });
                }
                else if (!Request.IsAuthenticated && (l == null || l == "" || l == "1"))
                {
                    return RedirectToAction("Index", new { sort, l = "0", g, f });
                }
            }
            catch
            {
                ; // Todo - fixup unit test
            }

            SetTourCookie();

            try
            {
                using (var db = new ZapContext())
                {
                    User user = await GetCurrentUser(db);
                    var posts = await GetPosts(
                        start: 0, 
                        count: 10, 
                        sort: sort ?? "Score", 
                        userId: user != null ? user.Id : 0);

                    if (user != null)
                    {
                        ValidateClaims(user); // Checks user security claims
                    }

                    PostsViewModel vm = new PostsViewModel()
                    {
                        Posts = await GeneratePostViewModels(user, posts, db),
                        UserBalance = user == null ? 0 : Math.Floor(user.Funds.Balance),    // TODO: Should this be here?
                        Sort = sort ?? "Score",
                        SubscribedGroups = GetUserGroups(user),
                    };
                    return View(vm);
                }
            }
            catch (Exception e)
            {
                MailingService.Send(new UserEmailModel()
                {
                    Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                    Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n user: " + User.Identity.GetUserId() ?? "anonymous",
                    Email = "",
                    Name = "zapread.com Exception",
                    Subject = "Exception on index",
                });
                throw e;
            }
        }

        private async Task<List<PostViewModel>> GeneratePostViewModels(User user, List<Post> posts, ZapContext db)
        {
            List<int> viewerIgnoredUsers = GetUserIgnoredUsers(user);
            var groups = await db.Groups
                        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .AsNoTracking()
                        .ToListAsync();
            var groupMemberCounts = groups.ToDictionary(i => i.GroupId, i => i.mc);
            var groupPostCounts = groups.ToDictionary(i => i.GroupId, i => i.pc);
            var groupLevels = groups.ToDictionary(i => i.GroupId, i => i.l);

            List<PostViewModel> postViews = posts
                .Select(p => new PostViewModel()
                {
                    Post = p,
                    ViewerIsMod = user != null ? user.GroupModeration.Select(grp => grp.GroupId).Contains(p.Group.GroupId) : false,
                    ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    ViewerIgnoredUser = user != null ? (user.IgnoringUsers != null ? p.UserId.Id != user.Id && user.IgnoringUsers.Select(usr => usr.Id).Contains(p.UserId.Id) : false) : false,
                    NumComments = 0,
                    ViewerIgnoredUsers = viewerIgnoredUsers,
                    GroupMemberCounts = groupMemberCounts,
                    GroupPostCounts = groupPostCounts,
                    GroupLevels = groupLevels,
                }).ToList();
            return postViews;
        }

        private static List<int> GetUserIgnoredUsers(User user)
        {
            List<int> viewerIgnoredUsers = new List<int>();

            if (user != null && user.IgnoringUsers != null)
            {
                viewerIgnoredUsers = user.IgnoringUsers.Select(usr => usr.Id).Where(usrid => usrid != user.Id).ToList();
            }

            return viewerIgnoredUsers;
        }

        private static List<GroupInfo> GetUserGroups(User user)
        {
            var gi = new List<GroupInfo>();
            if (user != null)
            {
                // Get list of user subscribed groups (with highest activity on top)
                int userid = user != null ? user.Id : 0;
                var userGroups = user.Groups
                    .Select(grp => new
                    {
                        IsModerator = grp.Moderators.Select(m => m.Id).Contains(userid),
                        IsAdmin = grp.Administrators.Select(m => m.Id).Contains(userid),
                        TotalIncome = grp.TotalEarned + grp.TotalEarnedToDistribute,
                        grp,
                    })
                    .OrderByDescending(grp => grp.TotalIncome)
                    .ToList();
                gi = userGroups.Select(grp => new GroupInfo()
                {
                    Id = grp.grp.GroupId,
                    Name = grp.grp.GroupName,
                    Icon = grp.grp.Icon,
                    Level = 1,
                    Progress = 36,
                    IsMod = grp.IsModerator,
                    IsAdmin = grp.IsAdmin,
                }).ToList();
            }
            return gi;
        }

        private void ValidateClaims(User user)
        {
            try
            {
                User.AddUpdateClaim("ColorTheme", user.Settings.ColorTheme ?? "light");
            }
            catch (Exception)
            {
                //TODO: handle (or fix test for HttpContext.Current.GetOwinContext().Authentication mocking)
            }
        }

        private async Task<User> GetCurrentUser(ZapContext db)
        {
            string userid = null;

            if (User != null) // This is the case when testing unauthorized call
            {
                userid = User.Identity.GetUserId();
            }

            var user = await db.Users
                .Include(usr => usr.Settings)
                .Include(usr => usr.IgnoringUsers)
                .Include(usr => usr.Groups)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AppId == userid);
            return user;
        }

        private void SetTourCookie()
        {
            ViewBag.ShowTourModal = false;
            try
            {
                if (SetOrUpdateUserTourCookie(defaultValue: "show") != "hide")
                {
                    // User has not dismissed tour request
                    ViewBag.ShowTourModal = false;// Temporary disable. true;
                }
            }
            catch
            {
                ; //TODO proper exception handling
            }
        }

        private string SetOrUpdateUserCookie()
        {
            string userId;
            //Check if user is returning
            if (HttpContext.Request.Cookies["ZapRead.com"] != null)
            {
                var cookie = HttpContext.Request.Cookies.Get("ZapRead.com");
                cookie.Expires = DateTime.Now.AddDays(30);   //update
                HttpContext.Response.Cookies.Remove("ZapRead.com");
                HttpContext.Response.SetCookie(cookie);
                userId = cookie.Value;
            }
            else
            {
                HttpCookie cookie = new HttpCookie("ZapRead.com");
                cookie.Value = Guid.NewGuid().ToString();
                cookie.Expires = DateTime.Now.AddDays(30);
                HttpContext.Response.Cookies.Remove("ZapRead.com");
                HttpContext.Response.SetCookie(cookie);
                userId = cookie.Value;
            }

            return userId;
        }

        [HttpPost]
        public async Task<ActionResult> InfiniteScroll(int BlockNumber, string sort)
        {
            int BlockSize = 10;
            
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = await db.Users.AsNoTracking()
                    .SingleOrDefaultAsync(u => u.AppId == uid);

                var posts = await GetPosts(BlockNumber, BlockSize, sort, user != null ? user.Id : 0);

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

        [HttpPost]
        public ActionResult SendFeedback(string msg, string loc)
        {
            String uid = "";

            uid = User.Identity.GetUserId();

            UserEmailModel message = new UserEmailModel();
            message.Email = "";
            message.Name = "ZapRead Feedback";
            message.Subject = "ZapRead Feedback";
            message.Body = msg + Environment.NewLine + " Location: " + loc + Environment.NewLine + Environment.NewLine + " User: " + uid;
            message.Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];
            MailingService.Send(message);

            return Json(new { result = "success" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMail(UserEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                //TODO: Have a proper error screen
                return RedirectToAction("Index");
            }

            model.Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];
            MailingService.Send(model);

            return RedirectToAction("FeedbackSuccess");
        }

        public ActionResult FeedbackSuccess()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About Zapread.com.";

            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Feedback()
        {
            return View();
        }

        private async Task EnsureUserExists(string userId, ZapContext db)
        {
            if (userId != null)
            {
                var user = await db.Users
                    .Include(usr => usr.ProfileImage)
                    .Include(usr => usr.ThumbImage)
                    .Include(usr => usr.Funds)
                    .Include(usr => usr.Settings)
                    .Where(u => u.AppId == userId)
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    // no user entry
                    User u = new User()
                    {
                        AboutMe = "Nothing to tell.",
                        AppId = userId,
                        Name = User.Identity.Name,
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
                    // ensure all properties are not null
                    if (user.Funds == null)
                    {
                        // DANGER!
                        user.Funds = new UserFunds();
                    }
                    if (user.Settings == null)
                    {
                        user.Settings = new UserSettings();
                    }
                    if (user.ThumbImage == null)
                    {
                        user.ThumbImage = new UserImage();
                    }
                    if (user.ProfileImage == null)
                    {
                        user.ProfileImage = new UserImage();
                    }
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}