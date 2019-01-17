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

namespace zapread.com.Controllers
{
    public class HomeController : Controller
    {
        private static DateTime lastLNCheck = DateTime.Now;

        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult UserImage(int? size, string UserId)
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
                MemoryStream ms = new MemoryStream();

                if (db.Users.Where(u => u.AppId == UserId).Count() > 0)
                {
                    var img = db.Users.Where(u => u.AppId == UserId).First().ProfileImage;
                    if (img.Image != null)
                    {
                        Image png = Image.FromStream(new MemoryStream(img.Image));
                        Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size);
                        byte[] data = thumb.ToByteArray(ImageFormat.Png);

                        return File(data, "image/png");
                    }
                }
                // Alternative if userId was username
                else if (db.Users.Where(u => u.Name == UserId).Count() > 0)
                {
                    var userAppId = db.Users.Where(u => u.Name == UserId).FirstOrDefault().AppId;
                    var img = db.Users.Where(u => u.AppId == userAppId).First().ProfileImage;
                    if (img.Image != null)
                    {
                        Image png = Image.FromStream(new MemoryStream(img.Image));
                        Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size);
                        byte[] data = thumb.ToByteArray(ImageFormat.Png);

                        return File(data, "image/png");
                    }
                }

                // Use generated icon

                // Identicon
                //var icon = Identicon.FromValue(UserId, size: (int)size);
                //icon.SaveAsPng(ms);
                //return File(ms.ToArray(), "image/png");

                // RoboHash
                var imagesPath = Server.MapPath("~/bin");
                RoboHash.Net.RoboHash.ImageFileProvider = new RoboHash.Net.Internals.DefaultImageFileProvider(
                    basePath: imagesPath);
                var r = RoboHash.Net.RoboHash.Create(UserId);
                using (var image = r.Render(
                    set: null,
                    backgroundSet: RoboHash.Net.RoboConsts.Any,
                    color: null,
                    width: (int)size,
                    height: (int)size))
                {
                    Bitmap thumb = ImageExtensions.ResizeImage(image, (int)size, (int)size);
                    byte[] data = thumb.ToByteArray(ImageFormat.Png);
                    return File(data, "image/png");
                }
            }
        }

        protected List<Post> GetPosts(int start, int count, string sort = "Score", int userId = 0)
        {
            //Reddit algorithm
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
            double ln10 = 2.302585092994046;

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
            

            using (var db = new ZapContext())
            {
                DateTime t = DateTime.Now;

                var user = db.Users
                    .Include(usr => usr.Settings)
                    .SingleOrDefault(u => u.Id == userId);

                if (sort == "Score")
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

                    DateTime scoreStart = new DateTime(2018, 07, 01);

                    var sposts = validposts//db.Posts//.AsNoTracking()
                        .Select(p => new
                        {
                            pst = p,
                            s = (p.Score > 0.0 ? p.Score : -1 * p.Score) < 1 ? 1 : (p.Score > 0.0 ? p.Score : -1 * p.Score),   // Absolute value of s
                            sign = p.Score > 0.0 ? 1.0 : -1.0,              // Sign of s
                            dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.TimeStamp),
                        })
                        .Select(p => new
                        {
                            pst = p.pst,
                            order = ((p.s - 1) + (-1.0 / p.s / p.s) * (p.s - 1) * (p.s - 1) / 2.0 + (-2.0 / p.s / p.s / p.s) * (p.s - 1) * (p.s - 1) * (p.s - 1) / 6.0) / ln10,
                            sign = p.sign,
                            dt = p.dt,
                        })
                        .Select(p => new
                        {
                            pst = p.pst,
                            hot = p.sign * p.order + p.dt / 45000
                        })
                        .OrderByDescending(p => p.hot)
                        .Select(p => p.pst)
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
                        .Take(count).ToList();


                    return sposts;
                }
                //if (sort == "New")
                else {
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

                    var posts = validposts//db.Posts//.AsNoTracking()
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
                        .Take(count).ToList();
                    return posts;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sort">Score, New</param>
        /// <param name="l"></param>
        /// <param name="g">include subscribed groups null = yes</param>
        /// <param name="f">include subscribed followers null = yes</param>
        /// <returns></returns>
        [OutputCache(Duration = 600, VaryByParam = "*", Location=System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> Index(string sort, string l, int? g, int? f)
        {
            // Check for settled invoices which were not applied every 5 minutes
            if (DateTime.Now - lastLNCheck > TimeSpan.FromMinutes(5))
            {
                lastLNCheck = DateTime.Now;
                var lndClient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings["LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings["LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings["LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings["LnMainnetMacaroonInvoice"]);

                using (var db = new ZapContext())
                {
                    // These are the unpaid invoices in database
                    var unpaidInvoices = db.LightningTransactions
                        .Where(t => t.IsSettled == false)
                        .Where(t => t.IsDeposit == true)
                        .Where(t => t.IsIgnored == false)
                        .Include(t => t.User)
                        .Include(t => t.User.Funds);

                    var website = db.ZapreadGlobals
                    .SingleOrDefault(ix => ix.Id == 1);

                    var invoiceDebug = unpaidInvoices.ToList();

                    foreach (var i in unpaidInvoices)
                    {
                        if (i.HashStr != null)
                        {
                            var inv = lndClient.GetInvoice(rhash: i.HashStr);
                            if (inv.settled != null && inv.settled == true)
                            {
                                // Paid but not applied in DB
                                var use = i.UsedFor;
                                if (use == TransactionUse.VotePost)
                                {
                                    if (false) // Disable for now
                                    {
                                        var vc = new VoteController();
                                        var v = new VoteController.Vote()
                                        {
                                            a = Convert.ToInt32(i.Amount),
                                            d = i.UsedForAction == TransactionUseAction.VoteDown ? 0 : 1,
                                            Id = i.UsedForId,
                                            tx = i.Id
                                        };
                                        await vc.Post(v);

                                        i.IsSpent = true;
                                        i.IsSettled = true;
                                        i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                    }
                                }
                                else if (use == TransactionUse.VoteComment)
                                {
                                    int z = 1;
                                }
                                else if (use == TransactionUse.UserDeposit)
                                {
                                    if (i.User == null)
                                    {
                                        // Not sure how to deal with this other than add funds to Community
                                        website.CommunityEarnedToDistribute += i.Amount;
                                        i.IsSpent = true;
                                        i.IsSettled = true;
                                        i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                    }
                                    else
                                    {
                                        // Deposit funds in user account
                                        int z = 1;
                                        var user = i.User;
                                        user.Funds.Balance += i.Amount;
                                        i.IsSettled = true;
                                        i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;

                                    }
                                }
                                else if (use == TransactionUse.Undefined)
                                {
                                    if (i.User == null)
                                    {
                                        // Not sure how to deal with this other than add funds to Community
                                        website.CommunityEarnedToDistribute += i.Amount;
                                        i.IsSpent = true;
                                        i.IsSettled = true;
                                        i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                    }
                                    else
                                    {
                                        // Not sure what the user was doing - deposit into their account.
                                        int z = 1;
                                    }
                                }
                            }
                            else
                            {
                                // Not settled - check expiry
                                var t1 = Convert.ToInt64(inv.creation_date);
                                var tNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                var tExpire = t1 + Convert.ToInt64(inv.expiry) + 10000; //Add a buffer time
                                if (tNow > tExpire)
                                {
                                    // Expired - let's stop checking this invoice
                                    i.IsIgnored = true;
                                }
                                else
                                {
                                    int w = 1; // keep waiting
                                }
                            }
                        }
                        else
                        {
                            // No hash string to look it up.  Must be an error somewhere.
                            i.IsIgnored = true;
                        }
                    }

                    db.SaveChanges();
                }
            }

            using (var db = new ZapContext())
            {
                string uid = null;

                if (User != null) // This is the case when testing unauthorized call
                {
                    uid = User.Identity.GetUserId();
                }

                await EnsureUserExists(uid, db);

                var user = db.Users
                    .Include("Settings")
                    .Include(usr => usr.IgnoringUsers)
                    .Include(usr => usr.Groups)
                    .AsNoTracking()
                    .SingleOrDefault(u => u.AppId == uid);

                var posts = GetPosts(0, 10, sort ?? "Score", user != null ? user.Id : 0);

                try
                {
                    User.AddUpdateClaim("ColorTheme", user.Settings.ColorTheme ?? "light");
                }
                catch (Exception)
                {
                    //TODO: handle (or fix test for HttpContext.Current.GetOwinContext().Authentication mocking)
                }

                var gi = new List<GroupInfo>();

                if (user != null)
                {
                    // Get list of user subscribed groups (with highest activity on top)
                    var userGroups = user.Groups
                        .OrderByDescending(grp => grp.TotalEarned + grp.TotalEarnedToDistribute)
                        .ToList();

                    foreach (var grp in userGroups)
                    {
                        gi.Add(new GroupInfo()
                        {
                            Id = grp.GroupId,
                            Name = grp.GroupName,
                            Icon = grp.Icon,
                            Level = 1,
                            Progress = 36,
                            IsMod = grp.Moderators.Select(m => m.Id).Contains(user.Id),
                            IsAdmin = grp.Administrators.Select(m => m.Id).Contains(user.Id),
                        });
                    }
                }

                List<PostViewModel> postViews = new List<PostViewModel>();

                List<int> viewerIgnoredUsers = new List<int>();

                if (user != null && user.IgnoringUsers != null)
                {
                    viewerIgnoredUsers = user.IgnoringUsers.Select(usr => usr.Id).Where(usrid => usrid != user.Id).ToList();
                }

                foreach (var p in posts)
                {
                    postViews.Add(new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(grp => grp.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerIgnoredUser = user != null ? (user.IgnoringUsers != null ? p.UserId.Id != user.Id && user.IgnoringUsers.Select(usr => usr.Id).Contains(p.UserId.Id) : false) : false,
                        NumComments = 0,

                        ViewerIgnoredUsers = viewerIgnoredUsers,
                    });
                }

                PostsViewModel vm = new PostsViewModel()
                {
                    Posts = postViews,
                    //Upvoted = user == null ? new List<int>() : user.PostVotesUp.Select(p => p.PostId).ToList(),
                    //Downvoted = user == null ? new List<int>() : user.PostVotesDown.Select(p => p.PostId).ToList(),
                    UserBalance = user == null ? 0 : Math.Floor(user.Funds.Balance),    // TODO: Should this be here?
                    Sort = sort == null ? "Score" : sort == "New" ? "New" : "UNK",
                    SubscribedGroups = gi,
                };

                return View(vm);
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
        public ActionResult InfiniteScroll(int BlockNumber, string sort)
        {
            int BlockSize = 10;
            
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.AppId == uid);

                var posts = GetPosts(BlockNumber, BlockSize, sort, user != null ? user.Id : 0);

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
                if (db.Users.Where(u => u.AppId == userId).Count() == 0)
                {
                    // no user entry
                    User u = new Models.User()
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
                    var user = db.Users
                        .Include(usr => usr.ProfileImage)
                        .Include(usr => usr.ThumbImage)
                        .Include(usr => usr.Funds)
                        .Include(usr => usr.Settings)
                        .Where(u => u.AppId == userId).First();

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
                }
            }
        }
    }
}