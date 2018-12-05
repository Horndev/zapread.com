﻿using HtmlAgilityPack;
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

                var icon = Identicon.FromValue(UserId, size: (int)size);

                icon.SaveAsPng(ms);
                return File(ms.ToArray(), "image/png");
            }
        }

        protected List<Post> GetPosts(int start, int count, string sort = "Score")
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
                if (sort == "Score")
                {
                    var sposts = db.Posts//.AsNoTracking()
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
                        //.OrderByDescending(p => 5.0-5*p.dt+5.0*p.dt*p.dt/2.0-5*p.dt*p.dt*p.dt/6.0+5*p.dt*p.dt*p.dt*p.dt/24.0)
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
                //{
                    var posts = db.Posts//.AsNoTracking()
                        .OrderByDescending(p => p.TimeStamp)
                        //.OrderByDescending(p => 5.0-5*p.dt+5.0*p.dt*p.dt/2.0-5*p.dt*p.dt*p.dt/6.0+5*p.dt*p.dt*p.dt*p.dt/24.0)
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
                //}
            }
        }

        [OutputCache(Duration = 600, VaryByParam = "*", Location=System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> Index(string sort)
        {
            using (var db = new ZapContext())
            {
                var posts = GetPosts(0, 10, sort ?? "Score");
                var uid = User.Identity.GetUserId();

                await EnsureUserExists(uid, db);

                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.AppId == uid);

                var gi = new List<GroupInfo>();

                if (user != null)
                {
                    var userGroups = user.Groups
                        .OrderByDescending(g => g.TotalEarned + g.TotalEarnedToDistribute)
                        .ToList();

                    foreach (var g in userGroups)
                    {
                        gi.Add(new GroupInfo()
                        {
                            Id = g.GroupId,
                            Name = g.GroupName,
                            Icon = g.Icon,
                            Level = 1,
                            Progress = 36,
                            //NumPosts = g.Posts.Count(),
                            //UserPosts = g.Posts.Where(p => p.UserId.Id == u.Id).Count(),
                            IsMod = g.Moderators.Contains(user),
                            IsAdmin = g.Administrators.Contains(user),
                        });
                    }
                }

                PostsViewModel vm = new PostsViewModel()
                {
                    Posts = posts,
                    Upvoted = user == null ? new List<int>() : user.PostVotesUp.Select(p => p.PostId).ToList(),
                    Downvoted = user == null ? new List<int>() : user.PostVotesDown.Select(p => p.PostId).ToList(),
                    UserBalance = user == null ? 0 : Math.Floor(user.Funds.Balance),
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
            var posts = GetPosts(BlockNumber, BlockSize, sort);

            string PostsHTMLString = "";

            foreach (var p in posts)
            {
                var PostHTMLString = RenderPartialViewToString("_PartialPostRender", p);
                PostsHTMLString += PostHTMLString;
            }
            return Json(new
            {
                NoMoreData = posts.Count < BlockSize,
                HTMLString = PostsHTMLString,
            });
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
            UserEmailModel message = new UserEmailModel();
            message.Email = "";
            message.Name = "ZapRead Feedback";
            message.Subject = "ZapRead Feedback";
            message.Body = msg + Environment.NewLine + " Location: " + loc;
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
                    };// no user entry
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