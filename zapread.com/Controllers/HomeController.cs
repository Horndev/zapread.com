using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Home;
using zapread.com.Services;

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
            stringBuilder.Append("sitemap: ");
            stringBuilder.AppendLine("https://www.zapread.com/sitemap.xml");

            return this.Content(stringBuilder.ToString(), "text/plain", Encoding.UTF8);
        }

        //[Route("sitemap.xml", Name = "GetSitemapXml"), OutputCache(Duration = 86400)]
        //public ContentResult SitemapXml()
        //{
        //}

        [HttpPost, ValidateJsonAntiForgeryToken]
        public async Task<ActionResult> SetUserImage(int set)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, result = "Failure", message = "User authentication error." });
            }

            var UserAppId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.AppId == UserAppId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);

                if (user == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { success = false, result = "Failure", message = "User not found." });
                }

                var imagesPath = Server.MapPath("~/bin");
                RoboHash.Net.RoboHash.ImageFileProvider = new RoboHash.Net.Internals.DefaultImageFileProvider(
                    basePath: imagesPath);

                try
                {

                    // The image hash is based on the user UID
                    var r = RoboHash.Net.RoboHash.Create(UserAppId);

                    var Rand = new Random();

                    var hashSet = "set1";

                    var robotSets = new List<string>() { "set1", "set3" };

                    if (set == 1)
                    {
                        hashSet = robotSets[Rand.Next(0, 1)];
                    }
                    else if (set == 2)
                    {
                        hashSet = "set4";
                    }
                    else if (set == 3)
                    {
                        hashSet = "set5";
                    }
                    else // Monster
                    {
                        hashSet = "set2";
                    }

                    int size = 1024;

                    using (var image = r.Render(
                        set: hashSet,
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
                            UserImage img = new UserImage() { Image = DBdata, Version = user.ProfileImage.Version + 1};
                            user.ProfileImage = img;
                            await db.SaveChangesAsync().ConfigureAwait(false);
                        }
                        return Json(new { success = true, result = "Success" });
                    }
                }
                catch (Exception e)
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Json(new { success = false, result = "Failure", message = "Endpoint error." });
                }
            }
        }

        /// <summary>
        /// Gets the user's image
        /// </summary>
        /// <param name="size"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [OutputCache(Duration = 3600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [HttpGet]
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
                // Fetch image from image cache
                var i = await db.Images
                    .Where(im => im.UserAppId == UserId && im.XSize == size)
                    .Select(im => new { im.Image })
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(false);

                if (i != null && i.Image != null)
                {
                    // https://code.msdn.microsoft.com/How-to-save-Image-to-978a7b0b
                    var ms = new MemoryStream(i.Image);
                    return new FileStreamResult(ms, "image/png");
                }
                else
                {
                    // Check if non size-cached version exists
                    i = await db.Users
                    .Where(u => u.AppId == UserId)
                    .Select(u => new { u.ProfileImage.Image })
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(false);

                    if (i != null && i.Image != null)
                    {
                        // Load and convert image size
                        using (var ms = new MemoryStream(i.Image))
                        {
                            Image png = Image.FromStream(ms);
                            using (Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size))
                            {
                                byte[] data = thumb.ToByteArray(ImageFormat.Png);

                                db.Images.Add(new Models.UserImage()
                                {
                                    ContentType = "image/png",
                                    Image = data,
                                    UserAppId = UserId,
                                    Version = 0,
                                    XSize = size.Value,
                                    YSize = size.Value,
                                });

                                await db.SaveChangesAsync().ConfigureAwait(false);

                                return File(data, "image/png");
                            }
                        }
                    }
                }

                // RoboHash
                var user = await db.Users
                    .Where(u => u.AppId == UserId || u.Name == UserId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);

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
                        UserImage img = new UserImage() { Image = DBdata, Version = 0 };
                        user.ProfileImage = img;
                        await db.SaveChangesAsync().ConfigureAwait(false);
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
                    cScore = p.Comments.Count() > 0 ? p.Comments.Where(c => !c.IsDeleted).Sum(c => Math.Abs((double)c.Score) < 1.0 ? 1.0 : Math.Abs((double)c.Score)) : 1.0
                })
                .Select(p => new
                {
                    p.p,
                    p.cScore,
                    s = (Math.Abs((double)p.p.Score) < 1.0 ? 1.0 : Math.Abs((double)p.p.Score)),    // Max (|x|,1)                                                           
                })
                .Select(p => new
                {
                    p.p,
                    order1 = SqlFunctions.Log10(p.s),
                    order2 = SqlFunctions.Log10(p.cScore < 1.0 ? 1.0 : p.cScore),     // Comment scores
                    sign = p.p.Score > 0.0 ? 1.0 : -1.0,                              // Sign of s
                    dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.p.TimeStamp),    // time since start
                })
                .Select(p => new
                {
                    p.p,
                    p.order1,
                    p.order2,
                    p.sign,
                    p.dt,
                    hot = p.sign * (p.order1 + p.order2) + p.dt / 90000
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
            var isenabled = System.Configuration.ConfigurationManager.AppSettings["EnableInstall"];
            if (!Convert.ToBoolean(isenabled))
            {
                return RedirectToAction("Index");
            }

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

                var communityGroup = db.Groups.FirstOrDefault(g => g.GroupId == 1);
                if (communityGroup == null)
                {
                    //Create community group
                    db.Groups.Add(new Group()
                    {
                        GroupId = 1,
                        GroupName = "Community",
                        CreationDate = DateTime.UtcNow,
                        DefaultLanguage = "en",
                        Icon = "star",
                        Tier = 0,
                        Tags = "random",
                        ShortDescription = "A catch-all group for ZapRead denziens",
                        TotalEarnedToDistribute = 0,
                        TotalEarned = 0,
                    });
                    db.SaveChanges();
                }
                
                return View();
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

        [HttpGet]
        public async Task<ActionResult> TopFollowing()
        {

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> TopPosts(string sort)
        {
            using (var db = new ZapContext())
            {
                User user = await GetCurrentUser(db).ConfigureAwait(true); // it would be nice to remove this line
                
                var userAppId = User.Identity.GetUserId();
                var userId = userAppId == null ? 0 : (await db.Users.FirstOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(true))?.Id;

                var posts = await GetPosts(
                    start: 0,
                    count: 10,
                    sort: sort ?? "Score",
                    userId: userId.Value).ConfigureAwait(true);

                PostsViewModel vm = new PostsViewModel()
                {
                    Posts = await GeneratePostViewModels(user, posts, db, userId.Value).ConfigureAwait(true),
                };

                var PostHTMLString = RenderPartialViewToString("_Posts", vm);
                return Json(new { success = true, HTMLString = PostHTMLString }, JsonRequestBehavior.AllowGet);
            }
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
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public async Task<ActionResult> Index(string sort, string l, int? g, int? f)
        {
            //PaymentPoller.Subscribe();
            //LNTransactionMonitor a = new LNTransactionMonitor();
            //a.CheckLNTransactions();
            //AchievementsService a = new AchievementsService();
            //a.CheckAchievements();

            try
            {
                if (Request.IsAuthenticated && (String.IsNullOrEmpty(l) || l == "0"))
                {
                    return RedirectToAction("Index", new { sort, l = "1", g, f });
                }
                else if (!Request.IsAuthenticated && (String.IsNullOrEmpty(l) || l == "1"))
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
                    User user = await GetCurrentUser(db).ConfigureAwait(true); // it would be nice to remove this line

                    var userAppId = User.Identity.GetUserId();

                    var userId = userAppId == null ? 0 : (await db.Users.FirstOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(true))?.Id;

                    if (!userId.HasValue)
                    {
                        return RedirectToAction(actionName: "Login", controllerName: "Account");
                    }

                    await ValidateClaims(userId.Value).ConfigureAwait(true); // Checks user security claims

                    //var posts = await GetPosts(
                    //    start: 0,
                    //    count: 10,
                    //    sort: sort ?? "Score",
                    //    userId: userId.Value).ConfigureAwait(true);

                    var vm = new HomeIndexViewModel()
                    {
                        Sort = sort ?? "Score",
                        SubscribedGroups = await GetUserGroups(user == null ? 0 : user.Id, db).ConfigureAwait(true),
                    };

                    //PostsViewModel vm = new PostsViewModel()
                    //{
                    //    Posts = await GeneratePostViewModels(user, posts, db, userId.Value).ConfigureAwait(true),
                    //    UserBalance = user == null ? 0 : Math.Floor(user.Funds.Balance),    // TODO: Should this be here?
                    //    Sort = sort ?? "Score",
                    //    SubscribedGroups = await GetUserGroups(user == null ? 0 : user.Id, db).ConfigureAwait(true),
                    //};
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

        private async Task<List<PostViewModel>> GeneratePostViewModels(User user, List<Post> posts, ZapContext db, int userId)
        {
            List<int> viewerIgnoredUsers = await GetUserIgnoredUsers(userId);
            List<PostViewModel> postViews = posts
                .Select(p => new PostViewModel()
                {
                    Post = p,
                    ViewerIsMod = user != null ? user.GroupModeration.Select(grp => grp.GroupId).Contains(p.Group.GroupId) : false,
                    ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    ViewerIgnoredUser = user != null ? (user.IgnoringUsers != null ? p.UserId.Id != user.Id && user.IgnoringUsers.Select(usr => usr.Id).Contains(p.UserId.Id) : false) : false,
                    NumComments = 0,
                    ViewerIgnoredUsers = viewerIgnoredUsers, // Very inefficient
                }).ToList();
            return postViews;
        }

        private static async Task<List<int>> GetUserIgnoredUsers(int userId)
        {
            if (userId > 0)
            {
                using (var db = new ZapContext())
                {
                    return await db.Users
                        .Where(u => u.Id == userId)
                        .SelectMany(u => u.IgnoringUsers.Select(i => i.Id))
                        .Where(i => i != userId)
                        .ToListAsync();
                }
            }
            else
            {
                return new List<int>();
            }
        }

        private static Task<List<GroupInfo>> GetUserGroups(int userId, ZapContext db)
        {
            return db.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.Groups)
                .OrderByDescending(g => g.TotalEarned)
                .Select(g => new GroupInfo() {
                    Id = g.GroupId,
                    IsAdmin = g.Administrators.Select(m => m.Id).Contains(userId),
                    IsMod = g.Moderators.Select(m => m.Id).Contains(userId),
                    Name = g.GroupName,
                    Icon = g.Icon,
                    Level = g.Tier,
                    Progress = 36,
                })
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task ValidateClaims(int userId)
        {
            try
            {
                if (userId > 0)
                {
                    using (var db = new ZapContext())
                    {
                        var us = await db.Users
                            .Where(u => u.Id == userId)
                            .Select(u => u.Settings)
                            .FirstOrDefaultAsync();
                        User.AddUpdateClaim("ColorTheme", us.ColorTheme ?? "light");
                    }
                }
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

                foreach (var p in posts)
                {
                    var pvm = new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        NumComments = 0,
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
