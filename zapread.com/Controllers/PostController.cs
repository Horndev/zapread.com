using Hangfire;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
using zapread.com.Services;

namespace zapread.com.Controllers
{
    //[RoutePrefix("{Type:regex(Post|post)}")]
    public class PostController : Controller
    {
        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

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

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        public class EditPostInfo
        {
            public int PostId { get; set; }
        }

        // This is a data structure to return the list of draft posts to view in a client-side table
        public class DataItem
        {
            public string Time { get; set; }
            public string Title { get; set; }
            public string Group { get; set; }
            public string GroupId { get; set; }
            public string PostId { get; set; }
        }

        /// <summary>
        /// This method returns the drafts table on the post editing view.
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "token in header")]
        public ActionResult GetDrafts(DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "No parameters passed to method call." });
            }

            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { success = false, message = "Credentials failure" });
            }

            using (var db = new ZapContext())
            {
                User u = db.Users
                        .Where(us => us.AppId == userId).First();

                var draftPosts = db.Posts
                    .Where(p => p.UserId.Id == u.Id)
                    .Where(p => p.IsDraft == true)
                    .Where(p => p.IsDeleted == false)
                    .Include(p => p.Group)
                    .OrderByDescending(p => p.TimeStamp)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToList();

                var values = draftPosts.Select(t => new DataItem()
                {
                    Time = t.TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Title = t.PostTitle,
                    Group = t.Group.GroupName,
                    GroupId = Convert.ToString(t.Group.GroupId),
                    PostId = Convert.ToString(t.PostId),
                }).ToList();

                int numrec = db.Posts
                    .Where(p => p.UserId.Id == u.Id)
                    .Where(p => p.IsDraft == true)
                    .Where(p => p.IsDeleted == false)
                    .Count();

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

        [HttpGet, Route("Post/Impressions/{id}")]
        public async Task<PartialViewResult> Impressions(int? id)
        {
            using (var db = new ZapContext())
            {
                var post = await db.Posts
                    .FirstOrDefaultAsync(p => p.PostId == id).ConfigureAwait(false);
                if (post != null)
                {
                    post.Impressions += 1;
                    ViewBag.PostImpressions = post.Impressions;
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                return PartialView("_Impressions");
            }
        }

        public async Task<JsonResult> ToggleStickyPost(int id)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var post = db.Posts
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .FirstOrDefault(p => p.PostId == id);

                if (post == null)
                {
                    return Json(new { Result = "Error" }, JsonRequestBehavior.AllowGet);
                }

                if (post.UserId.AppId == userId || UserManager.IsInRole(userId, "Administrator") || post.UserId.GroupModeration.Select(g => g.GroupId).Contains(post.Group.GroupId))
                {
                    post.IsSticky = !post.IsSticky;

                    await db.SaveChangesAsync();
                    return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Result = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public async Task<JsonResult> ToggleNSFW(int id)
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { success = false, message = "Credentials failure" }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new ZapContext())
            {
                var post = await db.Posts
                    .Include(p => p.UserId)
                    .FirstOrDefaultAsync(p => p.PostId == id);

                if (post == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { success = false, message = "Invalid post" }, JsonRequestBehavior.AllowGet);
                }

                var callingUserIsMod = await db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.GroupModeration.Select(g => g.GroupId))
                    .ContainsAsync(post.Group.GroupId);

                if (post.UserId.AppId == userId 
                    || UserManager.IsInRole(userId, "Administrator") 
                    || callingUserIsMod)
                {
                    post.IsNSFW = !post.IsNSFW;

                    // Alert the post owner
                    var postOwner = post.UserId;

                    // Add Alert
                    var alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = (post.IsNSFW ? "Your post has been marked NSFW : " : "Your post is no longer marked NSFW : ") + post.PostTitle,
                        Content = "A moderator has changed the Not Safe For Work status of your post.",
                        IsDeleted = false,
                        IsRead = false,
                        To = postOwner,
                        PostLink = post,
                    };
                    postOwner.Alerts.Add(alert);
                    await db.SaveChangesAsync();
                    return Json(new { success=true, message = "Success", post.IsNSFW }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Json(new { message = "Credentials failure" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> NewPost(int? group)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                await EnsureUserExists(userId, db);
                var user = db.Users.Where(u => u.AppId == userId).First();
                var communityGroup = db.Groups.FirstOrDefault(g => g.GroupId == 1);
                var postGroup = db.Groups.FirstOrDefault(g => g.GroupId == group);
                var post = new Post()
                {
                    Content = "",
                    UserId = user,
                    Group = postGroup,// ?? communityGroup,
                    Language = (postGroup ?? communityGroup).DefaultLanguage ?? "en",
                };

                // List of languages known
                var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                    .GroupBy(ci => ci.TwoLetterISOLanguageName)
                    .Select(g => g.First())
                    .Select(ci => ci.Name + ":" + ci.NativeName).ToList();

                var vm = new NewPostViewModel()
                {
                    Post = post,
                    Languages = languages,
                };

                return View(vm);
            }
        }

        private async Task EnsureUserExists(string userId, ZapContext db)
        {
            if (userId != null)
            {
                if (db.Users.Where(u => u.AppId == userId).Count() == 0)
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
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "Token in JSON header")]
        public async Task<JsonResult> SubmitNewPost(NewPostMsg p)
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { result = "failure", success = false, message = "Error finding user account." });
            }

            if (p == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { result = "failure", success = false, message = "Parameter error." });
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users.Where(u => u.AppId == userId)
                    .FirstOrDefaultAsync().ConfigureAwait(true);  // Note ConfigureAwait must be true since we need to preserve context for the mailer

                // Cleanup post HTML
                HtmlDocument postDocument = new HtmlDocument();
                postDocument.LoadHtml(p.Content);
                var postImages = postDocument.DocumentNode.SelectNodes("//img/@src");
                if (postImages != null)
                {
                    foreach (var item in postImages)
                    {
                        // ensure images have the img-fluid class
                        if (!item.HasClass("img-fluid"))
                        {
                            item.AddClass("img-fluid");
                        }
                    }
                }

                // Check links
                var postLinks = postDocument.DocumentNode.SelectNodes("//a/@href");
                if (postLinks != null)
                {
                    foreach (var link in postLinks.ToList())
                    {
                        string url = link.GetAttributeValue("href", "");
                        // replace links to embedded videos
                        if (url.Contains("youtu.be"))
                        {
                            var uri = new Uri(url);
                            string videoId = uri.Segments.Last();
                            string modElement = $"<div class='embed-responsive embed-responsive-16by9' style='float: none;'><iframe frameborder='0' src='//www.youtube.com/embed/{videoId}?rel=0&amp;loop=0&amp;origin=https://www.zapread.com' allowfullscreen='allowfullscreen' width='auto' height='auto' class='note-video-clip' style='float: none;'></iframe></div>";
                            var newNode = HtmlNode.CreateNode(modElement);
                            link.ParentNode.ReplaceChild(newNode, link);
                        }
                    }
                }
                string contentStr = postDocument.DocumentNode.OuterHtml.SanitizeXSS();
                var postGroup = db.Groups.FirstOrDefault(g => g.GroupId == p.GroupId);

                Post post = null;
                if (p.PostId > 0)
                {
                    // Updated post
                    post = db.Posts
                        .Include(pst => pst.UserId)
                        .Where(pst => pst.PostId == p.PostId).FirstOrDefault();

                    // Ensure user owns this post (or is site admin)
                    if (post.UserId.Id != user.Id && !User.IsInRole("Administrator"))
                    {
                        // Editing another user's post.
                        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Json(new { result = "failure", success = false, message = "User mismatch" });
                    }

                    post.PostTitle = p.Title == null ? "Post" : p.Title.CleanUnicode().SanitizeXSS();
                    post.Group = postGroup;
                    post.Content = contentStr;
                    post.Language = p.Language ?? post.Language;

                    if (post.IsDraft) // Post was or is draft - set timestamp.
                    {
                        post.TimeStamp = DateTime.UtcNow;
                    }
                    else // Post has been published, don't update timestamp, update edit timestamp.
                    {
                        post.TimeStampEdited = DateTime.UtcNow;
                    }

                    if (post.IsDraft && !p.IsDraft) // Post was a draft, now published
                    {
                        post.IsDraft = p.IsDraft;
                        await db.SaveChangesAsync().ConfigureAwait(true);
                        // We don't return yet - so notifications can be fired off.
                    }
                    else
                    {
                        post.IsDraft = p.IsDraft;
                        await db.SaveChangesAsync().ConfigureAwait(true);
                        return Json(new { result = "success", success = true, postId = post.PostId, HTMLContent = contentStr });
                    }
                }
                else
                {
                    // New post
                    post = new Post()
                    {
                        Content = contentStr,
                        UserId = user,
                        TotalEarned = 0,
                        IsDeleted = false,
                        Score = 1,
                        Group = postGroup,
                        TimeStamp = DateTime.UtcNow,
                        VotesUp = new List<User>() { user },
                        PostTitle = p.Title == null ? "" : p.Title.CleanUnicode().SanitizeXSS(),
                        IsDraft = p.IsDraft,
                        Language = p.Language,
                    };

                    db.Posts.Add(post);
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }

                bool quiet = false;  // Used when debugging

                if (p.IsDraft || quiet) // Don't send any alerts
                {
                    return Json(new { result = "success", success = true, postId = post.PostId, HTMLContent = contentStr });
                }

                // Send alerts to users subscribed to group
                await AlertGroupNewPost(db, postGroup, post).ConfigureAwait(true);

                // Send alerts to users subscribed to users
                var mailer = DependencyResolver.Current.GetService<MailerController>();
                await AlertUsersNewPost(db, user, post, mailer).ConfigureAwait(true);

                return Json(new { result = "success", success = true, postId = post.PostId, HTMLContent = contentStr });
            }
        }

        private async Task AlertUsersNewPost(ZapContext db, User user, Post post, MailerController mailer)
        {
            var followUsers = db.Users
                .Include("Alerts")
                .Include("Settings")
                .Where(u => u.Following.Select(usr => usr.Id).Contains(user.Id));

            mailer.ControllerContext = new ControllerContext(this.Request.RequestContext, mailer);
            string subject = "New post by user you are following: " + user.Name;
            string emailBody = await mailer.GenerateNewPostEmailBod(post.PostId, subject).ConfigureAwait(true);

            foreach (var u in followUsers)
            {
                // Add Alert
                var alert = new UserAlert()
                {
                    TimeStamp = DateTime.Now,
                    Title = "New post by user you are following: <a href='" + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = user.Name }) + "'>" + user.Name + "</a>",
                    Content = "",//post.PostTitle,
                    IsDeleted = false,
                    IsRead = false,
                    To = u,
                    PostLink = post,
                };

                u.Alerts.Add(alert);

                if (u.Settings == null)
                {
                    u.Settings = new UserSettings();
                }

                if (u.Settings.NotifyOnNewPostSubscribedUser)
                {
                    string followerEmail = UserManager.FindById(u.AppId).Email;

                    // Enqueue emails for sending out.  Don't need to wait for this to finish before returning client response
                    BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                        new UserEmailModel()
                        {
                            Destination = followerEmail,
                            Body = emailBody,
                            Email = "",
                            Name = "zapread.com",
                            Subject = subject,
                        }, "Notify"));
                }
            }
            await db.SaveChangesAsync();
        }

        private async Task AlertGroupNewPost(ZapContext db, Group postGroup, Post post)
        {
            var subusers = db.Users
                .Include("Alerts")
                .Where(u => u.Groups.Select(g => g.GroupId).Contains(postGroup.GroupId));

            foreach (var u in subusers)
            {
                // Add Alert
                var alert = new UserAlert()
                {
                    TimeStamp = DateTime.Now,
                    Title = "New post in subscribed group <a href='" + Url.Action(actionName: "GroupDetail", controllerName: "Group", routeValues: new { id = postGroup.GroupId }) + "'>" + postGroup.GroupName + "</a>",
                    Content = "",// "<a href='" + Url.Action(actionName:"Detail", controllerName: "Post", routeValues: new { post.PostId }) + "'>" + (post.PostTitle != null ? post.PostTitle : "Post") + "</a>",
                    IsDeleted = false,
                    IsRead = false,
                    To = u,
                    PostLink = post,
                };
                u.Alerts.Add(alert);
            }
            await db.SaveChangesAsync();
        }

        public class DeletePostMsg
        {
            public int PostId { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> DeletePost(DeletePostMsg p)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var post = db.Posts.Find(p.PostId);
                if (!User.IsInRole("Administrator"))
                {
                    if (post.UserId.AppId != userId)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                post.IsDeleted = true;
                await db.SaveChangesAsync().ConfigureAwait(false);

                return Json(new { Success = true });
                //return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Edit(EditPostInfo i)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = db.Users.Where(u => u.AppId == userId).First();
                var post = db.Posts.Include(p => p.UserId).Include(p => p.Group).FirstOrDefault(p => p.PostId == i.PostId);
                if (post == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View(post);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PostId"></param>
        /// <param name="vote">0 = downvote, 1 = upvote</param>
        /// <param name="postTitle">Optonal string which is used in SEO</param>
        /// <returns></returns>
        [MvcSiteMapNodeAttribute(Title = "Details", ParentKey = "Post", DynamicNodeProvider = "zapread.com.DI.PostsDetailsProvider, zapread.com")]
        [Route("Post/Detail/{PostId?}/{postTitle?}")]
        [HttpGet]
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> Detail(int? PostId, string postTitle, int? vote)
        {
            if (PostId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = await db.Users
                    .Include("Settings")
                    .Include(usr => usr.IgnoringUsers)
                    .SingleOrDefaultAsync(u => u.AppId == uid).ConfigureAwait(false);

                if (user != null)
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

                var pst = db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.Comments)
                    .Include(p => p.Comments.Select(cmt => cmt.Parent))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId.ProfileImage))
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .AsNoTracking()
                    .FirstOrDefault(p => p.PostId == PostId);

                if (pst == null)
                {
                    return RedirectToAction("PostNotFound");
                }

                if (vote.HasValue)
                {
                    ViewBag.showVote = true;
                    ViewBag.vote = vote.Value;
                }

                return View(await GeneratePostViewModel(db, user, pst));
            }
        }

        private static async Task<PostViewModel> GeneratePostViewModel(ZapContext db, User user, Post pst)
        {
            List<int> viewerIgnoredUsers = new List<int>();

            if (user != null && user.IgnoringUsers != null)
            {
                viewerIgnoredUsers = user.IgnoringUsers.Select(usr => usr.Id).Where(usrid => usrid != user.Id).ToList();
            }

            PostViewModel vm = new PostViewModel()
            {
                Post = pst,
                ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(pst.Group.GroupId) : false,
                ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(pst.PostId) : false,
                ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(pst.PostId) : false,
                ViewerIgnoredUser = user != null ? (user.IgnoringUsers != null ? pst.UserId.Id != user.Id && user.IgnoringUsers.Select(usr => usr.Id).Contains(pst.UserId.Id) : false) : false,
                NumComments = pst.Comments != null ? pst.Comments.Count() : 0,
                ViewerIgnoredUsers = viewerIgnoredUsers,
            };
            return vm;
        }

        public ActionResult PostNotFound()
        {
            return View();
        }

        public class UpdatePostMessage
        {
            public int PostId { get; set; }
            public int GroupId { get; set; }
            public string UserId { get; set; }
            public string Content { get; set; }
            public string Title { get; set; }
            public bool IsDraft { get; set; }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "JSON Header")]
        public async Task<JsonResult> Update(UpdatePostMessage p)
        {
            var userId = User.Identity.GetUserId();

            if (p == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { result = "failure", success = false, message = "Parameter error." });
            }

            if (userId != p.UserId && !User.IsInRole("Administrator"))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { result = "error", success = false, message = "User authentication error." });
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .SingleOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(true);
                var post = await db.Posts
                    .Include(ps => ps.UserId)
                    .Include(ps => ps.Group)
                    .SingleOrDefaultAsync(ps => ps.PostId == p.PostId).ConfigureAwait(true);
                if (post == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { result = "error", success=false, message = "Post not found." });
                }
                if (post.UserId.Id != user.Id && !User.IsInRole("Administrator"))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(new { result = "error", success=false, message = "User authentication error." });
                }

                string contentStr = p.Content.SanitizeXSS();
                post.Content = contentStr;
                post.PostTitle = p.Title == null ? "Post" : p.Title.CleanUnicode().SanitizeXSS();

                if (post.IsDraft)
                {
                    if (p.IsDraft)
                    {
                        // Post was draft - still draft
                        post.TimeStampEdited = DateTime.UtcNow;
                    }
                    else
                    {
                        // Post was draft - now live
                        post.TimeStamp = DateTime.UtcNow;

                        // Send alerts to users subscribed to group
                        var postGroup = db.Groups.FirstOrDefault(g => g.GroupId == p.GroupId);
                        await AlertGroupNewPost(db, postGroup, post).ConfigureAwait(true);

                        // Send alerts to users subscribed to users
                        var mailer = DependencyResolver.Current.GetService<MailerController>();
                        await AlertUsersNewPost(db, user, post, mailer).ConfigureAwait(true);
                    }
                }
                else
                {
                    // Post was already live - only edit timestamp can be changed.
                    if (!User.IsInRole("Administrator"))
                    {
                        post.TimeStampEdited = DateTime.UtcNow;
                    }
                }
                if (post.Group.GroupId != p.GroupId)
                {
                    // Need to reset score
                    post.Score = 1;
                    post.Group = await db.Groups.FirstAsync(g => g.GroupId == p.GroupId).ConfigureAwait(true);
                }

                post.IsDraft = p.IsDraft;
                await db.SaveChangesAsync().ConfigureAwait(true);
                return Json(new { result = "success", success = true, postId = post.PostId, HTMLContent = contentStr });
            }
        }

        [HttpPost]
        public JsonResult ChangeLanguage(int postId, string newLanguage)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Json(new { result = "error", success = false, message = "Admin role missing." });
            }

            using (var db = new ZapContext())
            {
                var post = db.Posts.SingleOrDefault(ps => ps.PostId == postId);

                if (post == null)
                {
                    return Json(new { result = "error", success = false, message = "Post not found in database." });
                }

                post.Language = newLanguage;

                db.SaveChanges();

                return Json(new { result = "success", success = true });
            }
        }
    }
}