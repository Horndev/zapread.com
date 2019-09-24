using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.GroupView;
using zapread.com.Models.UserViews;

namespace zapread.com.Controllers
{
    [RoutePrefix("user")]
    public class UserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public UserController()
        {
            // Empty constructor
        }

        public UserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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


        protected async Task<List<Post>> GetPosts(int start, int count, int userId = 0)
        {
            using (var db = new ZapContext())
            {
                var user = await db.Users
                        .AsNoTracking()
                        .Where(us => us.Id == userId).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new List<Post>();
                }

                // These are the user ids which we are following
                var followingIds = user.Following.Select(usr => usr.Id).ToList();// db.Users.Where(u => u.Name == username).Select(u => u.Id).ToList();

                var userposts = db.Posts
                    .Where(p => p.UserId.Id == user.Id)
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

                var followposts = db.Posts
                    .Where(p => followingIds.Contains(p.UserId.Id))
                    .Where(p => !p.IsDeleted)
                    .Include(p => p.Group)
                    .Include(p => p.Comments)
                    .Include(p => p.Comments.Select(cmt => cmt.Parent))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId))
                    .Include("UserId")
                    .AsNoTracking().Take(20);

                var activityposts = await userposts.Union(followposts).OrderByDescending(p => p.TimeStamp)
                    .Skip(start)
                    .Take(count)
                    .ToListAsync();

                return activityposts;
            }
        }

        [HttpPost]
        [Route("Achievement/Hover/")]
        public async Task<JsonResult> AchievementHover(int id)
        {
            using (var db = new ZapContext())
            {
                var a = await db.UserAchievements
                    .Include(i => i.Achievement)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (a == null)
                {
                    return Json(new { success = false, message = "Achievement not found." });
                }

                var vm = new UserAchievementViewModel()
                {
                    Id = a.Id,
                    ImageId = a.Achievement.Id,
                    Name = a.Achievement.Name,
                    DateAchieved = a.DateAchieved.Value,
                    Description = a.Achievement.Description,
                };

                string HTMLString = RenderPartialViewToString("_PartialUserAchievement", model: vm);
                return Json(new { success = true, HTMLString });
               
            }
        }

        [HttpPost]
        [Route("Hover/")]
        public async Task<JsonResult> Hover(int userId, string username)
        {
            using (var db = new ZapContext())
            {
                User user;
                User loggedInUser = null;
                if (userId == -1)
                {
                    string usernameClean = CleanUsername(username);
                    user = db.Users.FirstOrDefault(u => u.Name == usernameClean);
                }
                else
                {
                    user = db.Users.FirstOrDefault(u => u.Id == userId);
                }

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                if (User.Identity.IsAuthenticated)
                {
                    var uid = User.Identity.GetUserId();
                    loggedInUser = await db.Users
                        .Include(usr => usr.Following)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.AppId == uid);
                }

                bool isFollowing = loggedInUser != null ? loggedInUser.Following.Select(f => f.Id).Contains(user.Id) : false;

                UserHoverViewModel vm = new UserHoverViewModel()
                {
                    User = user,
                    IsFollowing = isFollowing,
                    IsIgnored = false,
                };
                string HTMLString = RenderPartialViewToString("_PartialUserHover", model: vm);
                return Json(new { success = true, HTMLString });
            }
        }

        private static string CleanUsername(string username)
        {
            string usernameCleaned = username;

            var doc = new HtmlDocument();
            doc.LoadHtml(username);

            // Need to remove any html tags

            usernameCleaned = doc.DocumentNode.InnerText.Replace("@", "");

            return usernameCleaned.Trim();
        }

        [Route("{username?}/Achievements")]
        public async Task<ActionResult> Achievements(string username)
        {
            if (username == null)
            {
                return RedirectToAction(actionName: "Index", controllerName: "Home");
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.Achievements)
                    .Include(u => u.Achievements.Select(a => a.Achievement))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Name == username);

                if (user == null)
                {
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                }

                var vm = new UserAchievementsViewModel()
                {
                    Achievements = new List<UserAchievementViewModel>(),
                    Username = username,
                };

                foreach (var ach in user.Achievements)
                {
                    vm.Achievements.Add(new UserAchievementViewModel()
                    {
                        Id = ach.Id,
                        ImageId = ach.Achievement.Id,
                        Name = ach.Achievement.Name,
                        Description = ach.Achievement.Description,
                        DateAchieved = ach.DateAchieved.Value,
                    });
                }

                return View(vm);
            }
        }

        // GET: User
        [Route("{username?}")]
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> Index(string username)
        {
            if (username == null)
            {
                return RedirectToAction(actionName: "Index", controllerName: "Manage");
            }

            using (var db = new ZapContext())
            {
                double userFunds = 0;
                User loggedInUser = null;
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId();
                    loggedInUser = await db.Users
                        .Include(usr => usr.IgnoringUsers)
                        .Include(usr => usr.Funds)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.AppId == userId);

                    if (loggedInUser != null && loggedInUser.Name == username)
                    {
                        return RedirectToAction(actionName: "Index", controllerName: "Manage");
                    }
                    userFunds = loggedInUser.Funds.Balance;
                }

                var user = await db.Users.Where(u => u.Name == username)
                    .Include(u => u.Following)
                    .Include(usr => usr.Groups)
                    .Include(usr => usr.Achievements)
                    .Include(usr => usr.Achievements.Select(ach => ach.Achievement))
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    // User doesn't exist.
                    // TODO: send to user not found error page
                    return RedirectToAction("Index", "Home");
                }

                var activityposts = await GetPosts(0, 10, user.Id);

                int numUserPosts = await db.Posts.Where(p => p.UserId.Id == user.Id)
                    .CountAsync();

                int numFollowers = await db.Users
                    .Where(p => p.Following.Select(f => f.Id).Contains(user.Id))
                    .CountAsync();

                int numFollowing = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Following)
                    .CountAsync();

                bool isFollowing = loggedInUser != null ? loggedInUser.Following.Select(f => f.Id).Contains(user.Id) : false;

                bool isIgnoring = loggedInUser != null ? (loggedInUser.IgnoringUsers != null ? loggedInUser.IgnoringUsers.Select(usr => usr.Id).Contains(user.Id) : false) : false;

                var topFollowing = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Following)
                    .OrderByDescending(us => us.TotalEarned)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync();

                var topFollowers = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Followers)
                    .OrderByDescending(us => us.TotalEarned)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync();

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
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        NumComments = 0,
                        GroupMemberCounts = groups.ToDictionary(i => i.GroupId, i => i.mc),
                        GroupPostCounts = groups.ToDictionary(i => i.GroupId, i => i.pc),
                        GroupLevels = groups.ToDictionary(i => i.GroupId, i => i.l),
                    });
                }

                List<GroupInfo> gi = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Groups)
                    .Select(g => new GroupInfo()
                    {
                        Id = g.GroupId,
                        Name = g.GroupName,
                        Icon = "fa-bolt",
                        Level = 1,
                        Progress = 36,
                        NumPosts = g.Posts.Count(),
                        UserPosts = g.Posts.Where(p => p.UserId.Id == user.Id).Count(),
                        IsMod = g.Moderators.Select(usr => usr.Id).Contains(user.Id),
                        IsAdmin = g.Administrators.Select(usr => usr.Id).Contains(user.Id),
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var uavm = new UserAchievementsViewModel
                {
                    Achievements = await db.Users.Where(u => u.Name == username)
                        .SelectMany(usr => usr.Achievements)
                        .Select(ac => new UserAchievementViewModel()
                        {
                            Id = ac.Id,
                            ImageId = ac.Achievement.Id,
                            Name = ac.Achievement.Name,
                        })
                        .AsNoTracking()
                        .ToListAsync()
                };

                var vm = new UserViewModel()
                {
                    AboutMe = new AboutMeViewModel()
                    {
                        AboutMe = user.AboutMe
                    },
                    UserGroups = new ManageUserGroupsViewModel() { Groups = gi },
                    NumPosts = numUserPosts,
                    NumFollowers = numFollowers,
                    NumFollowing = numFollowing,
                    IsFollowing = isFollowing,
                    IsIgnoring = isIgnoring,
                    User = user,
                    ActivityPosts = postViews,
                    TopFollowers = topFollowers,
                    TopFollowing = topFollowing,
                    UserBalance = userFunds,
                    AchievementsViewModel = uavm,
                };

                ViewBag.Username = username;
                ViewBag.UserId = user.Id;

                return View(vm);
            }
        }

        [HttpPost]
        [Route("InfiniteScroll/")]
        public async Task<ActionResult> InfiniteScroll(int BlockNumber, int? userId)
        {
            int BlockSize = 10;

            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                User user = await db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.AppId == uid);

                List<Post> posts = await GetPosts(BlockNumber, BlockSize, userId != null ? userId.Value : 0);

                List<GroupStats> groups = await db.Groups.AsNoTracking()
                        .Select(gr => new GroupStats { GroupId = gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .ToListAsync();

                string PostsHTMLString = "";
                foreach (var p in posts)
                {
                    PostViewModel pvm = HTMLRenderHelpers.CreatePostViewModel(p, user, groups);
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
        [Route("SetFollowing")]
        public JsonResult SetFollowing(int id, int s)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { Result = "Failure", Message = "You must be logged in to perform this action." });
            }

            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                User loggedInUser;

                loggedInUser = db.Users
                    .Where(us => us.AppId == userId)
                    .Include(u => u.Followers)
                    .Include(u => u.Following)
                    .FirstOrDefault();

                if (loggedInUser == null)
                {
                    return Json(new { Result = "Failure", Message = "Error finding logged in user." });
                }

                User user = db.Users.Where(u => u.Id == id)
                    .Include(u => u.Followers)
                    .FirstOrDefault();

                if (user == null)
                {
                    // User doesn't exist.
                    return Json(new { Result = "Failure", Message = "Error finding user." });
                }

                if (s == 0)
                {
                    // Unsubscribe
                    if (loggedInUser.Following.Select(f => f.Id).Contains(user.Id))
                    {
                        loggedInUser.Following.Remove(user);
                    }

                    if (user.Followers.Select(f => f.Id).Contains(loggedInUser.Id))
                    {
                        user.Followers.Remove(loggedInUser);
                    }
                }
                else
                {
                    // Subscribe
                    if (!loggedInUser.Following.Select(f => f.Id).Contains(user.Id))
                    {
                        loggedInUser.Following.Add(user);
                    }
                    if (!user.Followers.Select(f => f.Id).Contains(loggedInUser.Id))
                    {
                        user.Followers.Add(loggedInUser);
                    }
                }

                db.SaveChanges();
                return Json(new { Result = "Success" });
            }
        }

        /// <summary>
        /// Start following another user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Route("Follow/{username?}")]
        public ActionResult Follow(string username)
        {
            if (username == null)
            {
                return RedirectToAction(actionName: "Index", controllerName: "Home");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
            }

            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                User loggedInUser;

                loggedInUser = db.Users
                    .Where(us => us.AppId == userId)
                    .Include(u => u.Followers)
                    .FirstOrDefault();

                if (loggedInUser == null)
                {
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                }

                User user = db.Users.Where(u => u.Name == username)
                    .Include(u => u.Followers)
                    .FirstOrDefault();

                if (user == null)
                {
                    // User doesn't exist.
                    // TODO: send to user not found error page
                    return RedirectToAction("Index", "Home");
                }

                if (!loggedInUser.Following.Select(f => f.Id).Contains(user.Id))
                {
                    loggedInUser.Following.Add(user);
                }

                if (!user.Followers.Select(f => f.Id).Contains(loggedInUser.Id))
                {
                    user.Followers.Add(loggedInUser);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index", "User", new { username = username });
        }

        [Route("Unfollow/{username?}")]
        public ActionResult UnFollow(string username)
        {
            if (username == null)
            {
                return RedirectToAction(actionName: "Index", controllerName: "Home");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
            }

            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                User loggedInUser;

                loggedInUser = db.Users
                    .Where(us => us.AppId == userId)
                    .Include(u => u.Followers)
                    .FirstOrDefault();

                if (loggedInUser == null)
                {
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                }

                User user = db.Users.Where(u => u.Name == username)
                    .Include(u => u.Followers)
                    .FirstOrDefault();

                if (user == null)
                {
                    // User doesn't exist.
                    // TODO: send to user not found error page
                    return RedirectToAction("Index", "Home");
                }

                if (loggedInUser.Following.Select(f => f.Id).Contains(user.Id))
                {
                    loggedInUser.Following.Remove(user);
                }

                if (user.Followers.Select(f => f.Id).Contains(loggedInUser.Id))
                {
                    user.Followers.Remove(loggedInUser);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index", "User", new { username = username });
        }

        [HttpPost]
        [Route("ToggleIgnore")]
        public ActionResult ToggleIgnore(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new HttpUnauthorizedResult("User not authorized");
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Include(usr => usr.IgnoringUsers)
                    .FirstOrDefault(u => u.AppId == userId);

                var ignoredUser = db.Users
                    .FirstOrDefault(u => u.Id == id);

                if (ignoredUser == null)
                {
                    return Json(new { result = "error", message = "user not found." });
                }

                bool added = false;

                if (user.IgnoringUsers == null)
                {
                    user.IgnoringUsers = new List<User>();
                }

                if (user.IgnoringUsers.Select(u => u.Id).Contains(id))
                {
                    user.IgnoringUsers.Remove(ignoredUser);
                }
                else
                {
                    user.IgnoringUsers.Add(ignoredUser);
                    added = true;
                }

                db.SaveChanges();

                return Json(new { result = "success", added });
            }
        }

        /// <summary>
        /// Returns a partial view of a users recent activity
        /// </summary>
        /// <returns></returns>
        public ActionResult RecentActivity(string username)
        {
            /* Recent activity includes:
             * [ ] Posts
             * [ ] Comments
             * [ ] Upvotes (default yes)
             * [ ] Downvotes (default no)
             * [ ] Following Users
             */

            return PartialView();
        }

        // https://www.codemag.com/article/1312081/Rendering-ASP.NET-MVC-Razor-Views-to-String
        protected string RenderViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult =
                    ViewEngines.Engines.FindView(ControllerContext, "~/Views/User/" + viewName + ".cshtml", null);
                ViewContext viewContext = new ViewContext
                (ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}