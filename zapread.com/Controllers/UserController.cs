using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using zapread.com.Models.GroupView;
using zapread.com.Models.UserViews;

namespace zapread.com.Controllers
{
    [RoutePrefix("user")]
    public class UserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        /// <summary>
        /// 
        /// </summary>
        public UserController()
        {
            // Empty constructor
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        public UserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        protected async Task<List<PostViewModel>> GetActivityPosts(int start, int count, int userId = 0)
        {
            using (var db = new ZapContext())
            {
                List<int> followingIds = await db.Users
                        .Where(us => us.Id == userId)
                        .SelectMany(us => us.Following)
                        .Select(f => f.Id)
                        .ToListAsync()
                        .ConfigureAwait(true);

                var userposts = db.Posts
                    .Where(p => p.UserId.Id == userId)
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft)
                    .OrderByDescending(p => p.TimeStamp)
                    .Take(20)
                    .Select(p => new PostViewModel()
                    {
                        PostTitle = p.PostTitle,
                        Content = p.Content,
                        PostId = p.PostId,
                        GroupId = p.Group.GroupId,
                        GroupName = p.Group.GroupName,
                        IsSticky = p.IsSticky,
                        UserName = p.UserId.Name,
                        UserId = p.UserId.Id,
                        UserAppId = p.UserId.AppId,
                        UserProfileImageVersion = p.UserId.ProfileImage.Version,
                        Score = p.Score,
                        TimeStamp = p.TimeStamp,
                        TimeStampEdited = p.TimeStampEdited,
                        IsNSFW = p.IsNSFW,
                        ViewerIsMod = p.Group.Moderators.Select(m => m.Id).Contains(userId),
                        ViewerUpvoted = p.VotesUp.Select(v => v.Id).Contains(userId),
                        ViewerDownvoted = p.VotesDown.Select(v => v.Id).Contains(userId),
                        ViewerIgnoredUser = p.UserId.Id == userId ? false : p.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                        CommentVms = p.Comments.Select(c => new PostCommentsViewModel()
                        {
                            CommentId = c.CommentId,
                            Text = c.Text,
                            Score = c.Score,
                            IsReply = c.IsReply,
                            IsDeleted = c.IsDeleted,
                            TimeStamp = c.TimeStamp,
                            TimeStampEdited = c.TimeStampEdited,
                            UserId = c.UserId.Id,
                            UserName = c.UserId.Name,
                            UserAppId = c.UserId.AppId,
                            ProfileImageVersion = c.UserId.ProfileImage.Version,
                            ViewerUpvoted = c.VotesUp.Select(v => v.Id).Contains(userId),
                            ViewerDownvoted = c.VotesDown.Select(v => v.Id).Contains(userId),
                            ViewerIgnoredUser = c.UserId.Id == userId ? false : c.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                            ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                            ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                            ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                        }),
                    });

                    //.Include(p => p.Group)
                    //.Include(p => p.Comments)
                    //.Include(p => p.Comments.Select(cmt => cmt.Parent))
                    //.Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                    //.Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                    //.Include(p => p.Comments.Select(cmt => cmt.UserId))
                    //.Include(p => p.Comments.Select(cmt => cmt.UserId.ProfileImage))
                    //.Include(p => p.UserId)
                    //.Include(p => p.UserId.ProfileImage)
                    //.AsNoTracking();

                // These are the user ids which we are following
                //var followingIds = user.Following.Select(usr => usr.Id).ToList();// db.Users.Where(u => u.Name == username).Select(u => u.Id).ToList();

                // Posts by users who are following this user
                var followposts = db.Posts
                    .Where(p => followingIds.Contains(p.UserId.Id))
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft)
                    .OrderByDescending(p => p.TimeStamp)
                    .Take(20)
                    .Select(p => new PostViewModel()
                    {
                        PostTitle = p.PostTitle,
                        Content = p.Content,
                        PostId = p.PostId,
                        GroupId = p.Group.GroupId,
                        GroupName = p.Group.GroupName,
                        IsSticky = p.IsSticky,
                        UserName = p.UserId.Name,
                        UserId = p.UserId.Id,
                        UserAppId = p.UserId.AppId,
                        UserProfileImageVersion = p.UserId.ProfileImage.Version,
                        Score = p.Score,
                        TimeStamp = p.TimeStamp,
                        TimeStampEdited = p.TimeStampEdited,
                        IsNSFW = p.IsNSFW,
                        ViewerIsMod = p.Group.Moderators.Select(m => m.Id).Contains(userId),
                        ViewerUpvoted = p.VotesUp.Select(v => v.Id).Contains(userId),
                        ViewerDownvoted = p.VotesDown.Select(v => v.Id).Contains(userId),
                        ViewerIgnoredUser = p.UserId.Id == userId ? false : p.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                        CommentVms = p.Comments.Select(c => new PostCommentsViewModel()
                        {
                            CommentId = c.CommentId,
                            Text = c.Text,
                            Score = c.Score,
                            IsReply = c.IsReply,
                            IsDeleted = c.IsDeleted,
                            TimeStamp = c.TimeStamp,
                            TimeStampEdited = c.TimeStampEdited,
                            UserId = c.UserId.Id,
                            UserName = c.UserId.Name,
                            UserAppId = c.UserId.AppId,
                            ProfileImageVersion = c.UserId.ProfileImage.Version,
                            ViewerUpvoted = c.VotesUp.Select(v => v.Id).Contains(userId),
                            ViewerDownvoted = c.VotesDown.Select(v => v.Id).Contains(userId),
                            ViewerIgnoredUser = c.UserId.Id == userId ? false : c.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                            ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                            ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                            ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                        }),
                    });

                //.Include(p => p.Group)
                //.Include(p => p.Comments)
                //.Include(p => p.Comments.Select(cmt => cmt.Parent))
                //.Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                //.Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                //.Include(p => p.Comments.Select(cmt => cmt.UserId))
                //.Include(p => p.Comments.Select(cmt => cmt.UserId.ProfileImage))
                //.Include(p => p.UserId)
                //.Include(p => p.UserId.ProfileImage)
                //.AsNoTracking();

                //var activityposts = await userposts.Union(followposts)
                //    .OrderByDescending(p => p.TimeStamp)
                //    .Skip(start)
                //    .Take(count)
                //    .ToListAsync().ConfigureAwait(true);

                var activityposts = userposts.ToList().Union(followposts.ToList())
                    .OrderByDescending(p => p.TimeStamp)
                    .Skip(start)
                    .Take(count)
                    .ToList();
                    //.ToListAsync().ConfigureAwait(true);

                return activityposts;
            }
        }

        /// <summary>
        /// Gets the list of language options for the user
        /// </summary>
        /// <returns></returns>
        [Route("languages")]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public async Task<ActionResult> Languages(string prefix, int max)
        {
            // First we check what the browser is claiming to support
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

            var knownLanguages = LanguageHelpers.GetLanguages();

            // The first entries should be the user languages

            var userLangs = knownLanguages.Select(kl => kl.Split(':'))
                .Where(kl => userLanguages.Contains(kl[0]));

            var otherLangs = knownLanguages.Select(kl => kl.Split(':'))
                .Where(kl => !userLanguages.Contains(kl[0]));

            var query = userLangs.Concat(otherLangs)
                .DistinctBy(kl => kl[0] + kl[1]);

            if (String.IsNullOrEmpty(prefix))
            {
                
            }
            else
            {
                query = query.Where(kl => kl[1].ToLowerInvariant().StartsWith(prefix.ToLowerInvariant()));
            }
                
            var languages = query.Select(kl => new
                {
                    Name = kl[1],
                    iso = kl[0]
                })
                .Take(max);

            return Json(new { languages });
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId">The id of the user to hover</param>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Hover/")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<JsonResult> Hover(int userId, string username)
        {
            using (var db = new ZapContext())
            {
                int hoverUserId = userId; // take from parameter passed
                var userAppId = User.Identity.GetUserId();

                // If a username was provided - use it in search, otherwise don't use.  This is built as a separate query
                // here to reduce the sql query payload.
                IQueryable<User> uiq;
                if (!String.IsNullOrEmpty(username))
                {
                    uiq = db.Users
                        .Where(u => u.Id == hoverUserId || u.Name == username);
                }
                else
                {
                    uiq = db.Users
                        .Where(u => u.Id == hoverUserId);
                }

                var userInfo = await uiq
                    .Select(u => new
                    {
                        u.Id,
                        u.AppId,
                        u.Name,
                        u.Reputation,
                        u.ProfileImage.Version,
                        u.IsOnline,
                        IsFollowed = userAppId == null ? false : u.Followers.Select(f => f.AppId).Contains(userAppId),
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (userInfo == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { success = false, message = "User not found." });
                }

                UserHoverViewModel vm = new UserHoverViewModel()
                {
                    UserId = userInfo.Id,
                    AppId = userInfo.AppId,
                    Name = userInfo.Name,
                    Reputation = userInfo.Reputation,
                    ProfileImageVersion = userInfo.Version,
                    IsFollowing = userInfo.IsFollowed,
                    IsIgnored = false, // TODO?
                    IsOnline = userInfo.IsOnline,
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
                    Username = username,
                };

                var userAchievements = new List<UserAchievementViewModel>();

                foreach (var ach in user.Achievements)
                {
                    userAchievements.Add(new UserAchievementViewModel()
                    {
                        Id = ach.Id,
                        ImageId = ach.Achievement.Id,
                        Name = ach.Achievement.Name,
                        Description = ach.Achievement.Description,
                        DateAchieved = ach.DateAchieved.Value,
                    });
                }

                vm.Achievements = userAchievements;

                return View(vm);
            }
        }

        // GET: User
        [Route("{username?}")]
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [HttpGet]
        public async Task<ActionResult> Index(string username)
        {
            if (username == null)
            {
                return RedirectToAction(actionName: "Index", controllerName: "Manage");
            }

            using (var db = new ZapContext())
            {
                double userFunds = 0;
                bool isFollowing = false;
                bool isIgnoring = false;

                //var user = await db.Users.Where(u => u.Name == username)
                //    .Include(u => u.Following)
                //    .Include(usr => usr.Groups)
                //    .Include(usr => usr.ProfileImage)
                //    .Include(usr => usr.Achievements)
                //    .Include(usr => usr.Achievements.Select(ach => ach.Achievement))
                //    .AsNoTracking()
                //    .FirstOrDefaultAsync().ConfigureAwait(true);

                //ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                //ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                //ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,

                var userInfo = await db.Users
                    .Where(u => u.Name == username)
                    .Select(u => new
                    {
                        u.Name,
                        u.Id,
                        u.AboutMe,
                        u.AppId,
                        UserProfileImageVersion = u.ProfileImage.Version,
                        u.DateJoined,
                        u.Reputation,
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (userInfo == null)
                {
                    // User doesn't exist.
                    // TODO: send to user not found error page
                    return RedirectToAction("Index", "Home");
                }

                int userId = userInfo.Id;

                if (User.Identity.IsAuthenticated)
                {
                    var userAppId = User.Identity.GetUserId();

                    var loggedInUserInfo = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => new
                        {
                            u.Name,
                            u.Funds.Balance,
                            isFollowing = u.Following.Select(us => us.Id).Contains(userId),
                            isIgnoring = u.IgnoringUsers.Select(us => us.Id).Contains(userId),
                        })
                        .AsNoTracking()
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    if (loggedInUserInfo != null && loggedInUserInfo.Name == username)
                    {
                        return RedirectToAction(actionName: "Index", controllerName: "Manage");
                    }

                    userFunds = loggedInUserInfo == null ? 0 : loggedInUserInfo.Balance;
                    isFollowing = loggedInUserInfo.isFollowing;
                    isIgnoring = loggedInUserInfo.isIgnoring;
                }

                var activityposts = await QueryHelpers.QueryActivityPostsVm(0, 10, userId).ConfigureAwait(true); //await GetActivityPosts(0, 10, userId).ConfigureAwait(false);

                int numUserPosts = await db.Posts.Where(p => p.UserId.Id == userId)
                    .CountAsync().ConfigureAwait(true);

                int numFollowers = await db.Users
                    .Where(p => p.Following.Select(f => f.Id).Contains(userId))
                    .CountAsync().ConfigureAwait(true);

                int numFollowing = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Following)
                    .CountAsync().ConfigureAwait(true);

                
                var topFollowing = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Following)
                    .OrderByDescending(us => us.TotalEarned)
                    .Include(us => us.ProfileImage)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(true);

                var topFollowers = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Followers)
                    .OrderByDescending(us => us.TotalEarned)
                    .Include(us => us.ProfileImage)
                    .Take(20)
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(true);

                //List<PostViewModel> postViews = new List<PostViewModel>();

                //foreach (var p in activityposts)
                //{
                //    postViews.Add(new PostViewModel()
                //    {
                //        //Post = p,
                //        ViewerIsMod = false, //user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                //        ViewerUpvoted = false, //user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                //        ViewerDownvoted = false, //user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                //        NumComments = 0,
                //    });
                //}

                List<GroupInfo> gi = await db.Users.Where(u => u.Name == username)
                    .SelectMany(usr => usr.Groups)
                    .Select(g => new GroupInfo()
                    {
                        Id = g.GroupId,
                        Name = g.GroupName,
                        Icon = "fa-bolt",
                        Level = 1,
                        Progress = 36,
                        NumPosts = g.Posts.Count,
                        UserPosts = g.Posts.Where(p => p.UserId.Id == userId).Count(),
                        IsMod = g.Moderators.Select(usr => usr.Id).Contains(userId),
                        IsAdmin = g.Administrators.Select(usr => usr.Id).Contains(userId),
                    })
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(true);

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
                        .ToListAsync().ConfigureAwait(true)
                };

                var vm = new UserViewModel()
                {
                    AboutMe = new AboutMeViewModel()
                    {
                        AboutMe = userInfo.AboutMe
                    },
                    UserGroups = new ManageUserGroupsViewModel() { Groups = gi },
                    NumPosts = numUserPosts,
                    NumFollowers = numFollowers,
                    NumFollowing = numFollowing,
                    IsFollowing = isFollowing,
                    IsIgnoring = isIgnoring,
                    //User = user,
                    ActivityPosts = activityposts,
                    TopFollowers = topFollowers,
                    TopFollowing = topFollowing,
                    UserBalance = userFunds,
                    AchievementsViewModel = uavm,
                    UserId = userId,
                    UserAppId = userInfo.AppId,
                    UserProfileImageVersion = userInfo.UserProfileImageVersion,
                    UserName = userInfo.Name,
                    DateJoined = userInfo.DateJoined,
                    Reputation = userInfo.Reputation,
                };

                ViewBag.Username = username;
                ViewBag.UserId = userId;

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
                User user = await db
                    .Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.AppId == uid).ConfigureAwait(true);

                List<PostViewModel> posts = await QueryHelpers.QueryActivityPostsVm(BlockNumber, BlockSize, userId != null ? userId.Value : 0).ConfigureAwait(true);

                //List <GroupStats> groups = await db.Groups.AsNoTracking()
                //        .Select(gr => new GroupStats { GroupId = gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                //        .ToListAsync().ConfigureAwait(true);

                string PostsHTMLString = "";
                foreach (var p in posts)
                {
                    var PostHTMLString = RenderPartialViewToString("_PartialPostRenderVm", p);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id of user to follow</param>
        /// <param name="s">setting 1 = follow; 0 = unfollow</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetFollowing")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult SetFollowing(int id, int s)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in to perform this action." });
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
                    return Json(new { success = false, message = "Error finding logged in user." });
                }

                User user = db.Users.Where(u => u.Id == id)
                    .Include(u => u.Followers)
                    .FirstOrDefault();

                if (user == null)
                {
                    // User doesn't exist.
                    return Json(new { success = false, message = "Error finding user." });
                }

                if (loggedInUser.Id == user.Id)
                {
                    return Json(new { success = false, message = "Can't follow yourself!" });
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
                return Json(new { success = true });
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

                if (user == null)
                {
                    user = new User()
                    {
                        IgnoringUsers = new List<User>()
                    };
                }

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