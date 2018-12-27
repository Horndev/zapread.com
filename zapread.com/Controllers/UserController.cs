using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;

namespace zapread.com.Controllers
{
    [RoutePrefix("user")]
    public class UserController : Controller
    {
        // GET: User
        [Route("{username?}")]
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult Index(string username)
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
                    loggedInUser = db.Users.Where(u => u.AppId == userId)
                        .Include(usr => usr.Funds)
                        .AsNoTracking().FirstOrDefault();

                    if (loggedInUser != null && loggedInUser.Name == username)
                    {
                        return RedirectToAction(actionName: "Index", controllerName: "Manage");
                    }
                    userFunds = loggedInUser.Funds.Balance;
                }

                var user = db.Users.Where(u => u.Name == username)
                    .Include(u => u.Following)
                    .AsNoTracking().FirstOrDefault();

                if (user == null)
                {
                    // User doesn't exist.
                    // TODO: send to user not found error page
                    return RedirectToAction("Index", "Home");
                }

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

                var activityposts = userposts.Union(followposts).OrderByDescending(p => p.TimeStamp).Take(10).ToList();

                int numUserPosts = db.Posts.Where(p => p.UserId.Id == user.Id).Count();

                int numFollowers = db.Users.Where(p => p.Following.Select(f => f.Id).Contains(user.Id)).Count();

                int numFollowing = user.Following.Count();

                bool isFollowing = loggedInUser != null ? loggedInUser.Following.Select(f => f.Id).Contains(user.Id) : false;

                var topFollowing = user.Following.OrderByDescending(us => us.TotalEarned).Take(20).ToList();

                var topFollowers = user.Followers.OrderByDescending(us => us.TotalEarned).Take(20).ToList();

                List<PostViewModel> postViews = new List<PostViewModel>();

                foreach (var p in activityposts)
                {
                    postViews.Add(new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    });
                }

                var vm = new UserViewModel()
                {
                    AboutMe = new AboutMeViewModel()
                    {
                        AboutMe = user.AboutMe
                    },
                    NumPosts = numUserPosts,
                    NumFollowers = numFollowers,
                    NumFollowing = numFollowing,
                    IsFollowing = isFollowing, 
                    User = user,
                    ActivityPosts = postViews,
                    TopFollowers = topFollowers,
                    TopFollowing = topFollowing,
                    UserBalance = userFunds, 
                };

                ViewBag.Username = username;
                ViewBag.UserId = user.Id;

                return View(vm);
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
            
            return RedirectToAction("Index", "User", new { username=username});
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
    }
}