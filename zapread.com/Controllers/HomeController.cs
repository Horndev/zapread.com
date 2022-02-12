using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
            stringBuilder.AppendLine("disallow: /Account/UserBalance/");
            stringBuilder.AppendLine("disallow: /Account/Balance/");
            stringBuilder.AppendLine("disallow: /Account/GetBalance/");
            stringBuilder.AppendLine("disallow: /Account/Login/");
            stringBuilder.AppendLine("disallow: /Messages/SendMessage/");
            stringBuilder.AppendLine("disallow: /Comment/DeleteComment/");
            stringBuilder.AppendLine("disallow: /Comment/GetInputBox/");
            stringBuilder.AppendLine("disallow: /Manage/TipUser/");
            stringBuilder.AppendLine("disallow: /Post/ToggleStickyPost/");
            stringBuilder.AppendLine("disallow: /Img/Content/");

            stringBuilder.Append("sitemap: ");
            stringBuilder.AppendLine("https://www.zapread.com/sitemap.xml");

            return this.Content(stringBuilder.ToString(), "text/plain", Encoding.UTF8);
        }

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
                    var r = RoboHash.Net.RoboHash.Create(Guid.NewGuid().ToString());

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
                            UserImage img = new UserImage() {
                                ContentType = "image/png",
                                Image = DBdata,
                                XSize = 1024,
                                YSize = 1024,
                                Version = user.ProfileImage.Version + 1};
                            user.ProfileImage = img;
                            await db.SaveChangesAsync().ConfigureAwait(false);
                            return Json(new { success = true, version = img.Version });
                        }
                        return Json(new { success = false, message = "User not found" });
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
        [OutputCache(Duration = 3600, VaryByParam = "v;r", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [HttpGet]
        public async Task<ActionResult> UserImage(int? size, string UserId, string v, string r)
        {
            if (!size.HasValue)
            {
                size = 100;
            }

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

            int ver = -1;

            // Check for image in DB
            using (var db = new ZapContext())
            {
                try
                {
                    if (v != null)
                    {
                        ver = Convert.ToInt32(v, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var dbver = await db.Images.Where(im => im.UserAppId == UserId)
                            .Select(im => (int?)im.Version)
                            .MaxAsync().ConfigureAwait(true);
                        ver = dbver ?? -1;
                    }
                }
                catch (FormatException fe)
                {
                    //
                }

                var imgq = db.Images.Where(im => im.UserAppId == UserId && im.XSize == size);
                if (ver > -1)
                {
                    imgq = imgq.Where(im => im.Version == ver);
                }

                // Fetch image from image cache
                var i = await imgq
                    .Select(im => new { im.Image, im.ContentType, im.Version })
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(false);

                if (r == null && i != null && i.Image != null)
                {
                    // https://code.msdn.microsoft.com/How-to-save-Image-to-978a7b0b
                    var ms = new MemoryStream(i.Image);
                    return new FileStreamResult(ms, i.ContentType ?? "image/png");
                }
                else
                {
                    // Check if non size-cached version exists
                    var uimq = db.Users.Where(u => u.AppId == UserId);

                    //if (ver > -1)
                    //{ 
                    //    uimq = uimq.Where(u => u.ProfileImage.Version == ver);
                    //}

                    // This is the most recent (current) user image
                    i = await uimq
                    .Select(u => new { u.ProfileImage.Image, u.ProfileImage.ContentType, u.ProfileImage.Version})
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(false);

                    if (i != null && i.Image != null)
                    {
                        // Load and convert image size to size requested
                        using (var ms = new MemoryStream(i.Image))
                        {
                            Image png = Image.FromStream(ms);
                            using (Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size))
                            {
                                byte[] data = thumb.ToByteArray(ImageFormat.Png);
                                if (ver < 0)
                                {
                                    ver = 0;
                                }
                                db.Images.Add(new Models.UserImage()
                                {
                                    ContentType = "image/png",
                                    Image = data,
                                    UserAppId = UserId,
                                    Version = i.Version,//ver, // Not using requested version (this could lead to a bug)
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
                var rh = RoboHash.Net.RoboHash.Create(UserId);
                using (var image = rh.Render(
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

        /// <summary>
        /// Method to get the posts view model
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="sort"></param>
        /// <param name="userAppId"></param>
        /// <returns></returns>
        protected async Task<List<PostViewModel>> GetPostsVm(int start, int count, string sort = "Score", string userAppId = "")
        {
            List<string> userLanguages = GetUserLanguages();

            using (var db = new ZapContext())
            {
                //DateTime t = DateTime.Now;

                var userInfo = string.IsNullOrEmpty(userAppId) ? null : await db.Users
                    //.Include(usr => usr.Settings)
                    //.AsNoTracking()
                    .Select(u => new QueryHelpers.PostQueryUserInfo()
                    {
                        Id = u.Id,
                        AppId = u.AppId,
                        ViewAllLanguages = u.Settings.ViewAllLanguages,
                        IgnoredGroups = u.IgnoredGroups.Select(g => g.GroupId).ToList(),
                    })
                    .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);

                IQueryable<Post> validposts = QueryHelpers.QueryValidPosts(userLanguages, db, userInfo);

                IQueryable<QueryHelpers.PostQueryInfo> postquery = null;

                //var numposts = validposts.Count();// DEBUG

                switch (sort)
                {
                    case "Score":
                        postquery = QueryHelpers.OrderPostsByScore(validposts);
                        //var numvalidposts = postquery.Count();
                        break;
                    case "Active":
                        postquery = QueryHelpers.OrderPostsByActive(validposts);
                        break;
                    default:
                        postquery = QueryHelpers.OrderPostsByNew(validposts);
                        break;
                }

                return await QueryHelpers.QueryPostsVm(
                    start: start, 
                    count: count, 
                    postquery: postquery, 
                    userInfo: userInfo).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sort"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected async Task<IQueryable<QueryHelpers.PostQueryInfo>> GetPostsQuery(ZapContext db, string sort = "Score", string userAppId = null)
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

            //DateTime t = DateTime.Now;

            //var user = await db.Users
            //    .Include(usr => usr.Settings)
            //    .SingleOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

            var userInfo = string.IsNullOrEmpty(userAppId) ? null : await db.Users
                .Select(u => new QueryHelpers.PostQueryUserInfo()
                {
                    Id = u.Id,
                    AppId = u.AppId,
                    ViewAllLanguages = u.Settings.ViewAllLanguages,
                    IgnoredGroups = u.IgnoredGroups.Select(g => g.GroupId).ToList(),
                })
                .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);

            IQueryable<Post> validposts = QueryHelpers.QueryValidPosts(
                userLanguages: userLanguages, 
                db: db, 
                userInfo: userInfo);

            switch (sort)
            {
                case "Score":
                    return QueryHelpers.OrderPostsByScore(validposts);
                case "Active":
                    return QueryHelpers.OrderPostsByActive(validposts);
                default:
                    return QueryHelpers.OrderPostsByNew(validposts);
            }
        }

        private List<string> GetUserLanguages()
        {
            List<string> userLanguages;

            try
            {
                userLanguages = Request.UserLanguages.ToList().Select(l => l.Split(';')[0].Split('-')[0]).Distinct().ToList();

                if (userLanguages.Count == 0)
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
        [HttpGet]
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
                    .SingleOrDefaultAsync(i => i.Id == 1).ConfigureAwait(true);

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
                        EarningEvents = new List<EarningEvent>(),
                        SpendingEvents = new List<SpendingEvent>(),
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

        /// <summary>
        /// User does not want to continue tour - make note in cookie
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("This endpoint is not used and will be retired in a future version.")]
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
                //HttpContext.Response.SetCookie(cookie);
                cookieResultValue = cookie.Value;
            }

            return cookieResultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TopFollowing()
        {

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetPayoutInfo()
        {
            using (var db = new ZapContext())
            {
                var website = await db.ZapreadGlobals.Where(gl => gl.Id == 1).FirstOrDefaultAsync().ConfigureAwait(false);

                if (website == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Json(new { success = false });
                }

                return Json(new 
                { 
                    success = true, 
                    community=Convert.ToInt32(website.CommunityEarnedToDistribute),
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> TopPosts(string sort)
        {
            using (var db = new ZapContext())
            {
                //User user = await GetCurrentUser(db).ConfigureAwait(true); // it would be nice to remove this line
                
                var userAppId = User.Identity.GetUserId();
                //var userId = userAppId == null ? 0 : (await db.Users.FirstOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(true))?.Id;

                PostsViewModel vm = new PostsViewModel()
                {
                    Posts = await GetPostsVm(0,10,sort, userAppId).ConfigureAwait(true)
                };

                // Not sure these are needed.
                if (vm.Downvoted == null)
                {
                    vm.Downvoted = new List<int>();
                }

                if (vm.Upvoted == null)
                {
                    vm.Upvoted = new List<int>();
                }

                var PostHTMLString = RenderPartialViewToString("_PartialPosts", vm);

                string contentStr = PostHTMLString;

                try
                {
                    var cookie = HttpContext.Request.Cookies.Get("tarteaucitron");

                    if (cookie != null)
                    {
                        var youtubeCookie = cookie.Value.Split('!').Select(i => i.Split('=')).Where(i => i.Length > 1).Where(i => i[0] == "zyoutube").FirstOrDefault();
                        if (youtubeCookie != null && youtubeCookie[1] == "false")
                        {
                            HtmlDocument postDocument = new HtmlDocument();
                            postDocument.LoadHtml(PostHTMLString);

                            // Check links
                            var postLinks = postDocument.DocumentNode.SelectNodes("//iframe/@src");
                            if (postLinks != null)
                            {
                                foreach (var link in postLinks.ToList())
                                {
                                    string url = link.GetAttributeValue("src", "");
                                    // replace links to embedded videos
                                    if (url.Contains("youtube"))
                                    {
                                        var uri = new Uri(url);
                                        string videoId = uri.Segments.Last();
                                        //string modElement = $"<div class='embed-responsive embed-responsive-16by9' style='float: none;'><iframe frameborder='0' src='//www.youtube.com/embed/{videoId}?rel=0&amp;loop=0&amp;origin=https://www.zapread.com' allowfullscreen='allowfullscreen' width='auto' height='auto' class='note-video-clip' style='float: none;'></iframe></div>";
                                        string modElement = $"<div class='youtube_player' videoID='{videoId}' showinfo='0'></div>";
                                        var newNode = HtmlNode.CreateNode(modElement);
                                        link.ParentNode.ReplaceChild(newNode, link);
                                    }
                                }
                            }
                            contentStr = postDocument.DocumentNode.OuterHtml;
                        }
                        else
                        {
                            // filter youtube nocookie anyway
                            contentStr = PostHTMLString.Replace("//www.youtube.com/", "//www.youtube-nocookie.com/");
                        }
                    }
                } catch (Exception)
                {

                }
                return Json(new { success = true, HTMLString = contentStr }, JsonRequestBehavior.AllowGet);
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
            //LNNodeMonitor a = new LNNodeMonitor();
            //a.UpdateHourly();

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

            //SetTourCookie();

            try
            {
                using (var db = new ZapContext())
                {
                    var userAppId = User.Identity.GetUserId();
                    int userId = 0;
                    List<GroupInfo> subscribedGroups;

                    if (userAppId == null)
                    {
                        // Not logged in
                        subscribedGroups = new List<GroupInfo>();
                    }
                    else
                    {
                        User user = await GetCurrentUser(db).ConfigureAwait(true); // it would be nice to remove this line (for now, only used when logged in)

                        if (user == null)
                        {
                            return RedirectToAction(actionName: "Login", controllerName: "Account");
                        }

                        subscribedGroups = await GetUserGroups(user == null ? 0 : user.Id, db).ConfigureAwait(true);
                        userId = user.Id;
                        await ClaimsHelpers.ValidateClaims(userId, User).ConfigureAwait(true);
                    }

                    //var userId = userAppId == null ? 0 : (await db.Users.FirstOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(true))?.Id;

                    var vm = new HomeIndexViewModel()
                    {
                        Sort = sort ?? "Score",
                        SubscribedGroups = subscribedGroups,
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

        private static Task<List<GroupInfo>> GetUserGroups(int userId, ZapContext db)
        {
            if (userId == 0)
            {
                return db.Groups
                    .OrderByDescending(g => g.TotalEarned)
                    .Take(20)
                    .Select(g => new GroupInfo()
                    {
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

        //private async Task ValidateClaims(int userId)
        //{
        //    try
        //    {
        //        if (userId > 0)
        //        {
        //            using (var db = new ZapContext())
        //            {
        //                var us = await db.Users
        //                    .Where(u => u.Id == userId)
        //                    .Select(u => new
        //                    {
        //                        u.Settings.ColorTheme,
        //                        u.ProfileImage.Version,
        //                        u.AppId,
        //                    })
        //                    .FirstOrDefaultAsync().ConfigureAwait(true);

        //                User.AddUpdateClaim("ColorTheme", us.ColorTheme ?? "light");
        //                User.AddUpdateClaim("ProfileImageVersion", us.Version.ToString(CultureInfo.InvariantCulture));
        //                User.AddUpdateClaim("UserAppId", us.AppId);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        //TODO: handle (or fix test for HttpContext.Current.GetOwinContext().Authentication mocking)
        //    }
        //}

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
                .FirstOrDefaultAsync(u => u.AppId == userid).ConfigureAwait(true);
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

        /// <summary>
        /// Returns the next set of posts, rendered to HTML
        /// </summary>
        /// <param name="BlockNumber">The starting block</param>
        /// <param name="sort">How the posts are sorted</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "Validated in JSON header")]
        public async Task<ActionResult> InfiniteScroll(int BlockNumber, string sort)
        {
            int BlockSize = 10;

            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();

                var user = await db.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.AppId
                    })
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(true);

                var postquery = await GetPostsQuery(
                    db: db, 
                    sort: sort, 
                    userAppId: userAppId).ConfigureAwait(true);

                var postsVm = await QueryHelpers.QueryPostsVm(
                    start: BlockNumber, 
                    count: BlockSize, 
                    postquery: postquery, 
                    userInfo: new QueryHelpers.PostQueryUserInfo()
                    {
                        Id = user.Id,
                        AppId = userAppId,
                    }).ConfigureAwait(true);

                string PostsHTMLString = "";

                foreach (var p in postsVm)
                {
                    var PostHTMLString = RenderPartialViewToString("_PartialPostRenderVm", p);
                    PostsHTMLString += PostHTMLString;
                }

                // filter youtube nocookie anyway
                var contentStr = PostsHTMLString.Replace("//www.youtube.com/", "//www.youtube-nocookie.com/");

                return Json(new
                {
                    NoMoreData = postsVm.Count < BlockSize,
                    HTMLString = contentStr,//PostsHTMLString,
                });
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
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "JSON Only")]
        public ActionResult SendFeedback(string msg, string loc)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult FeedbackSuccess()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult About()
        {
            ViewBag.Message = "About Zapread.com.";

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult FAQ()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Feedback()
        {
            return View();
        }
    }
}
