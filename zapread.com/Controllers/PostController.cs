using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity.Owin;
using zapread.com.Services;
using HtmlAgilityPack;

namespace zapread.com.Controllers
{
    [RoutePrefix("{Type:regex(Post|post)}")]
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

        public class DataItem
        {
            public string Time { get; set; }
            public string Title { get; set; }
            public string Group { get; set; }
            public string GroupId { get; set; }
            public string PostId { get; set; }
        }

        [HttpPost]
        public ActionResult GetDrafts(DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                User u;
                u = db.Users
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

        public async Task<JsonResult> ToggleStickyPost(int id)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var post = db.Posts
                    .Include("UserId")
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

        public async Task<JsonResult> ToggleNSFW(int id)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var post = db.Posts
                    .Include("UserId")
                    .FirstOrDefault(p => p.PostId == id);

                if (post == null)
                {
                    return Json(new { Result = "Error" }, JsonRequestBehavior.AllowGet);
                }

                if (post.UserId.AppId == userId || UserManager.IsInRole(userId, "Administrator") || post.UserId.GroupModeration.Select(g => g.GroupId).Contains(post.Group.GroupId))
                {
                    post.IsNSFW = !post.IsNSFW;

                    // Alert the post owner

                    var postOwner = post.UserId;

                    // Add Alert
                    var alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = (post.IsNSFW ? "Your post has been marked NSFW : " : "Your post is no longer marked NSFW : "  ) + post.PostTitle,
                        Content = "A moderator has changed the Not Safe For Work status of your post.",
                        IsDeleted = false,
                        IsRead = false,
                        To = postOwner,
                        PostLink = post,
                    };

                    postOwner.Alerts.Add(alert);

                    await db.SaveChangesAsync();
                    return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Result = "Error" }, JsonRequestBehavior.AllowGet);
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
                    Content = "",//"<h3>[Post Title]</h3> \r\n [add contents here] \r\n <br />",
                    UserId = user,
                    Group = postGroup ?? communityGroup,
                };
                return View(post);
            }
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
                    };
                    db.Users.Add(u);
                    await db.SaveChangesAsync();
                }
            }
        }

        [HttpPost]
        public async Task<JsonResult> SubmitNewPost(NewPostMsg p)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var user = db.Users.Where(u => u.AppId == userId).First();

                var contentStr = p.Content;

                var postGroup = db.Groups.FirstOrDefault(g => g.GroupId == p.GroupId);

                Post post = null;

                if (p.PostId > 0)
                {
                    // Updated post
                    post = db.Posts.Where(pst => pst.PostId == p.PostId).FirstOrDefault();

                    post.PostTitle = p.Title;
                    post.Group = postGroup;
                    post.Content = p.Content;
                    post.IsDraft = p.IsDraft;
                    post.TimeStamp = DateTime.UtcNow;
                    await db.SaveChangesAsync();

                    return Json(new { result = "success", postId = post.PostId });
                }
                else
                {
                    // New post
                    post = new Post()
                    {
                        Content = p.Content,
                        UserId = user,
                        TotalEarned = 0,
                        IsDeleted = false,
                        Score = 1,
                        Group = postGroup,
                        TimeStamp = DateTime.UtcNow,
                        VotesUp = new List<User>() { user },
                        PostTitle = p.Title,
                        IsDraft = p.IsDraft,
                    };

                    db.Posts.Add(post);
                    await db.SaveChangesAsync();
                }
                
                if (p.IsDraft)
                {
                    // Don't send any alerts
                    return Json(new { result = "success", postId = post.PostId });
                }

                // Send alerts to users subscribed to group

                var subusers = db.Users
                    .Include("Alerts")
                    .Where(u => u.Groups.Select(g => g.GroupId).Contains(postGroup.GroupId));

                foreach (var u in subusers)
                {
                    // Add Alert
                    var alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "New post in subscribed group " + postGroup.GroupName,
                        Content = post.PostTitle,
                        IsDeleted = false,
                        IsRead = false,
                        To = u,
                        PostLink = post,
                    };

                    u.Alerts.Add(alert);
                }

                

                // Send alerts to users subscribed to users

                var followUsers = db.Users
                    .Include("Alerts")
                    .Include("Settings")
                    .Where(u => u.Following.Select(usr => usr.Id).Contains(user.Id));

                foreach (var u in followUsers)
                {
                    // Add Alert
                    var alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "New post by user you are following: " + user.Name,
                        Content = post.PostTitle,
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
                        // send email
                        var doc = new HtmlDocument();
                        doc.LoadHtml(post.Content);
                        var baseUri = new Uri("https://www.zapread.com/");
                        var imgs = doc.DocumentNode.SelectNodes("//img/@src");
                        if (imgs != null)
                        {
                            foreach (var item in imgs)
                            {
                                item.SetAttributeValue("src", new Uri(baseUri, item.GetAttributeValue("src", "")).AbsoluteUri);
                            }
                        }
                        string postContent = doc.DocumentNode.OuterHtml;

                        string followerEmail = UserManager.FindById(u.AppId).Email;
                        MailingService.Send(user: "Notify",
                            message: new UserEmailModel()
                            {
                                Subject = "New post by user you are following: " + user.Name,
                                Body = "<a href='http://www.zapread.com/Post/Detail/" + post.PostId.ToString() + "'>"+ post.PostTitle + "</a><br/><br/>" + postContent + "<br/><br/>" + "<a href='http://www.zapread.com'>zapread.com</a><br/><br/>Log in and go to your user settings to unsubscribe from these emails.", //, // "From: " + user.Name + "<br/> " + comment.Text + "<br/><br/>Go to <a href='http://www.zapread.com/Post/Detail/" + post.PostId.ToString() + "'>post</a> at <a href='http://www.zapread.com'>zapread.com</a>",
                                Destination = followerEmail,
                                Email = "",
                                Name = "ZapRead.com Notify"
                            });
                    }
                }

                await db.SaveChangesAsync();

                return Json(new { result = "success", postId = post.PostId });
            }
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
                await db.SaveChangesAsync();

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
                var post = db.Posts.AsNoTracking().Include(p => p.UserId).FirstOrDefault(p => p.PostId == i.PostId);
                if (post == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View(post);
            }
        }

        [Route("Detail/{PostId}")]
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult Detail(int PostId)
        {
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.AppId == uid);

                var pst = db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.Comments)
                    .Include(p => p.Comments.Select(cmt => cmt.Parent))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId))
                    .Include("UserId")
                    .AsNoTracking()
                    .FirstOrDefault(p => p.PostId == PostId);

                PostViewModel vm = new PostViewModel()
                {
                    Post = pst,
                    ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                    ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(pst.PostId) : false,
                    ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(pst.PostId) : false,
                };

                return View(vm);
            }

                
        }

        public class UpdatePostMessage
        {
            public int PostId { get; set; }
            public string UserId { get; set; }
            public string Content { get; set; }
            public string Title { get; set; }
            public bool IsDraft { get; set; }
        }

        [HttpPost]
        public async Task<JsonResult> Update(UpdatePostMessage p)
        {
            var userId = User.Identity.GetUserId();
            if (userId != p.UserId)
            {
                return Json(new { result = "error" });
            }

            using (var db = new ZapContext())
            {
                //var user = db.Users.Where(u => u.AppId == userId).First();
                var post = db.Posts.FirstOrDefault(ps => ps.PostId == p.PostId);
                if (post == null)
                {
                    return Json(new { result = "error" });
                }
                var contentStr = p.Content;
                post.Content = contentStr;
                post.PostTitle = p.Title;
                post.IsDraft = p.IsDraft;
                post.TimeStamp = DateTime.UtcNow;

                await db.SaveChangesAsync();
                return Json(new { result = "success", postId = post.PostId });
            }
        }
    }
}