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

namespace zapread.com.Controllers
{
    public class HomeController : Controller
    {
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult UserImage(int? size, string UserId)
        {
            if (size == null) size = 100;
            if (UserId != null)
            {

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

                var icon = Identicon.FromValue(UserId, size: (int)size);

                icon.SaveAsPng(ms);
                return File(ms.ToArray(), "image/png");
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
            using (var db = new ZapContext())
            {
                DateTime t = DateTime.Now;

                var user = db.Users
                    .SingleOrDefault(u => u.Id == userId);

                if (sort == "Score")
                {
                    IQueryable<Post> validposts;
                    if (userId > 0)
                    {
                        var ig = user.IgnoredGroups.Select(g => g.GroupId);
                        validposts = db.Posts.Where(p => !ig.Contains(p.Group.GroupId));
                    }
                    else
                    {
                        validposts = db.Posts;
                    }

                    var sposts = validposts//db.Posts//.AsNoTracking()
                        .Select(p => new
                        {
                            pst = p,
                            s = (p.Score > 0.0 ? p.Score : -1 * p.Score) < 1 ? 1 : (p.Score > 0.0 ? p.Score : -1 * p.Score),   // Absolute value of s
                            sign = p.Score > 0.0 ? 1.0 : -1.0,              // Sign of s
                            dt = 1.0 * DbFunctions.DiffSeconds(DateTime.UtcNow, p.TimeStamp),
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
                    }
                    else
                    {
                        validposts = db.Posts;
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
                    .Include(usr => usr.UserIgnores)
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
                            IsMod = grp.Moderators.Contains(user),
                            IsAdmin = grp.Administrators.Contains(user),
                        });
                    }
                }

                List<PostViewModel> postViews = new List<PostViewModel>();

                List<int> viewerIgnoredUsers = new List<int>();

                if (user != null && user.UserIgnores != null)
                {
                    viewerIgnoredUsers = user.UserIgnores.IgnoringUsers.Select(usr => usr.Id).Where(usrid => usrid != user.Id).ToList();
                }

                foreach (var p in posts)
                {
                    postViews.Add(new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(grp => grp.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerIgnoredUser = user != null ? (user.UserIgnores != null ? p.UserId.Id != user.Id && user.UserIgnores.IgnoringUsers.Select(usr => usr.Id).Contains(p.UserId.Id) : false) : false,

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
            message.Destination = "steven.horn.mail@gmail.com";
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

            model.Destination = "steven.horn.mail@gmail.com";
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