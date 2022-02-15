using Microsoft.AspNet.Identity;
using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Admin;
using zapread.com.Models.API.DataTables;
using zapread.com.Models.Database;
using zapread.com.Models.GroupViews;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Controller for the /Group path
    /// </summary>
    public class GroupController : Controller
    {
        // GET: Group
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [HttpGet]
        public async Task<ActionResult> Index(int? p = 1)
        {
            using (var db = new ZapContext())
            {
                GroupsViewModel vm = new GroupsViewModel()
                {
                    TotalPosts = (await db.Posts.CountAsync().ConfigureAwait(true)).ToString("N0", CultureInfo.InvariantCulture),
                };
                return View(vm);
            }
        }

        /// <summary>
        /// Returns the data for the groups index table, which lists all the groups
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost, Route("Group/GetGroupsTable")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> GetGroupsTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Error = "No query provided." });
            }

            using (var db = new ZapContext())
            {
                var sorts = dataTableParameters.Order;
                var search = dataTableParameters.Search;

                User user = await GetCurrentUser(db).ConfigureAwait(true);
                ValidateClaims(user);
                int userid = user != null ? user.Id : 0;

                // Build query
                var groupsQ = db.Groups
                    .Select(g => new
                    {
                        numPosts = g.Posts.Count,
                        numMembers = g.Members.Count,
                        IsMember = g.Members.Select(m => m.Id).Contains(userid),
                        IsModerator = g.Moderators.Select(m => m.Id).Contains(userid),
                        IsAdmin = g.Administrators.Select(m => m.Id).Contains(userid),
                        g.GroupId,
                        g.GroupName,
                        g.Tags,
                        g.TotalEarned,
                        g.TotalEarnedToDistribute,
                        g.CreationDate,
                        g.Icon,
                        g.Tier,
                    }).AsNoTracking();

                if (search.Value != null)
                {
                    groupsQ = groupsQ.Where(g => g.GroupName.Contains(search.Value) || g.Tags.Contains(search.Value));
                }

                groupsQ = groupsQ.OrderByDescending(g => g.TotalEarned + g.TotalEarnedToDistribute);

                var groups = await groupsQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToListAsync().ConfigureAwait(false);

                var values = groups.Select(g => new GroupInfo()
                {
                    Id = g.GroupId,
                    CreatedddMMMYYYY = g.CreationDate == null ? "2 Aug 2018" : g.CreationDate.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                    Name = g.GroupName,
                    NumMembers = g.numMembers,
                    NumPosts = g.numPosts,
                    Tags = g.Tags != null ? g.Tags.Split(',').ToList() : new List<string>(),
                    Icon = g.Icon != null ? "fa-" + g.Icon : null, // "fa-bolt",  // NOTE: this is legacy, and will eventually be replaced.  All new groups will have image icons.
                    Level = g.Tier,
                    Progress = GetGroupProgress(g.TotalEarned, g.TotalEarnedToDistribute, g.Tier),
                    IsMember = g.IsMember,
                    IsLoggedIn = user != null,
                    IsMod = g.IsModerator,
                    IsAdmin = g.IsAdmin,
                }).ToList();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = await db.Groups.CountAsync().ConfigureAwait(false),
                    recordsFiltered = await groupsQ.CountAsync().ConfigureAwait(false),
                    data = values
                };
                return Json(ret);
            }
        }

        private void ValidateClaims(User user)
        {
            if (user != null)
            {
                try
                {
                    User.AddUpdateClaim("ColorTheme", user.Settings.ColorTheme ?? "light");
                }
                catch (Exception)
                {
                    ; //TODO: handle (or fix test for HttpContext.Current.GetOwinContext().Authentication mocking)
                }
            }
        }

        private async Task<User> GetCurrentUser(ZapContext db)
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users
                .Include(u => u.Settings)
                .FirstOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(true);
            return user;
        }

        /// <summary>
        /// GET: Group/Members/1
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Members(int id)
        {
            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.Settings)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(true);

                var group = await db.Groups
                    .Include(g => g.Members)
                    .Include(g => g.Moderators)
                    .Include(g => g.Administrators)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupId == id).ConfigureAwait(true);

                List<GroupMemberViewModel> groupMembers = new List<GroupMemberViewModel>();

                // Verify calling user is a group admin
                var isGroupAdmin = user == null ? false: group.Administrators.Select(m => m.Id).Contains(user.Id);

                foreach (var m in group.Members)
                {
                    groupMembers.Add(new GroupMemberViewModel()
                    {
                        ViewerIsGroupAdministrator = isGroupAdmin,
                        UserName = m.Name,
                        UserId = m.Id,
                        GroupId = group.GroupId, // Should move to a separate view maybe
                        AboutMe = m.AboutMe,
                        AppId = m.AppId,
                        IsModerator = group.Moderators.Select(u => u.Id).Contains(m.Id),
                        IsGroupAdministrator = group.Administrators.Select(u => u.Id).Contains(m.Id),
                        IsSiteAdministrator = m.Name == "Zelgada", // Hardcoded for now.  Need to make DB flag in future.
                        IsOnline = m.IsOnline,
                        LastSeen = m.DateLastActivity,
                    });
                }

                GroupMembersViewModel vm = new GroupMembersViewModel()
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    Icon = group.Icon != null ? "fa-" + group.Icon : "fa-bolt",
                    TotalEarned = group.TotalEarned + group.TotalEarnedToDistribute,
                    Members = groupMembers,
                };

                return View(vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> UpdateUserMakeAdmin(int id, int groupId)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }

            using (var db = new ZapContext())
            {
                var u = db.Users.Where(usr => usr.Id == id).FirstOrDefault();
                if (u == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return Json(new { success = false, message = "Group not found" });
                }

                // Verify calling user is a group admin or site admin
                var userId = User.Identity.GetUserId();

                var callingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(usr => usr.AppId == userId);

                // TODO also check if user is a site admin
                if (!g.Administrators.Select(m => m.Id).Contains(callingUser.Id))
                {
                    return Json(new { success = false, message = "You do not have administration privilages for this group." });
                }

                g.Administrators.Add(u);
                u.GroupAdministration.Add(g);

                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            return Json(new { success = true, result = "success" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> UpdateUserRevokeAdmin(int id, int groupId)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var u = db.Users.Where(usr => usr.Id == id).FirstOrDefault();
                if (u == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return Json(new { success = false, message = "Group not found" });
                }

                // Verify calling user is a group admin or site admin
                var userId = User.Identity.GetUserId();

                var callingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(usr => usr.AppId == userId);

                // TODO also check if user is a site admin
                if (!g.Administrators.Select(m => m.Id).Contains(callingUser.Id))
                {
                    return Json(new { success = false, message = "You do not have administration privilages for this group." });
                }

                g.Administrators.Remove(u);
                u.GroupAdministration.Remove(g);

                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            return Json(new { success = true, result = "success" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> UpdateUserMakeMod(int id, int groupId)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var u = db.Users.Where(usr => usr.Id == id).FirstOrDefault();
                if (u == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return Json(new { success = false, message = "Group not found" });
                }

                // Verify calling user is a group admin or site admin
                var userId = User.Identity.GetUserId();

                var callingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(usr => usr.AppId == userId);

                // TODO also check if user is a site admin
                if (!g.Administrators.Select(m => m.Id).Contains(callingUser.Id))
                {
                    return Json(new { success = false, message = "You do not have administration privilages for this group." });
                }

                g.Moderators.Add(u);
                u.GroupModeration.Add(g);

                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            return Json(new { success = true, result = "success" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> UpdateUserRevokeMod(int id, int groupId)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var u = db.Users.Where(usr => usr.Id == id).FirstOrDefault();
                if (u == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return Json(new { success = false, message = "Group not found" });
                }

                // Verify calling user is a group admin or site admin
                var userId = User.Identity.GetUserId();

                var callingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(usr => usr.AppId == userId);

                // TODO also check if user is a site admin
                if (!g.Administrators.Select(m => m.Id).Contains(callingUser.Id))
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Json(new { success = false, message = "You do not have administration privilages for this group." });
                }

                g.Moderators.Remove(u);
                u.GroupModeration.Remove(g);

                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            return Json(new { success = true, result = "success" });
        }

        /// <summary>
        /// Load more posts for the group detail view
        /// </summary>
        /// <param name="BlockNumber"></param>
        /// <param name="groupId"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> InfiniteScroll(int BlockNumber, int groupId, string sort)
        {
            int BlockSize = 10;

            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();
                //var user = await db.Users
                //    .AsNoTracking()
                //    .FirstOrDefaultAsync(u => u.AppId == uid).ConfigureAwait(true);

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
                    .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(true);

                //List<int> viewerIgnoredUsers = new List<int>();

                //if (user != null && user.IgnoringUsers != null)
                //{
                //    viewerIgnoredUsers = user.IgnoringUsers.Select(usr => usr.Id).Where(usid => usid != user.Id).ToList();
                //}

                //int userId = user == null ? 0 : user.Id;

                IQueryable<Post> validposts = QueryHelpers.QueryValidPosts(null, db, userInfo);

                var postquery = QueryHelpers.OrderPostsByNew(validposts, groupId, true);

                var postsVm = await QueryHelpers.QueryPostsVm(
                    start: BlockNumber, 
                    count: BlockSize, 
                    postquery: postquery, 
                    userInfo: userInfo).ConfigureAwait(true);

                // Render each post HTML
                string PostsHTMLString = "";
                foreach (var pvm in postsVm)
                {
                    var PostHTMLString = RenderPartialViewToString("_PartialPostRenderVm", pvm);
                    PostsHTMLString += PostHTMLString;
                }

                return Json(new
                {
                    Success = true,
                    NoMoreData = postsVm.Count < BlockSize,
                    HTMLString = PostsHTMLString,
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Group/Hover")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "Not sensitive")]
        public async Task<JsonResult> Hover(int groupId)
        {
            using (var db = new ZapContext())
            {
                var userId = User.Identity.GetUserId();

                if (userId == null)
                {
                    userId = "";
                }

                var group = await db.Groups
                    .Select(g => new
                    {
                        g.GroupId,
                        g.Tier,
                        MemberCount = g.Members.Count,
                        PostCount = g.Posts.Count,
                        IsMember = g.Members.Select(m=>m.AppId).Contains(userId),
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupId == groupId).ConfigureAwait(false);

                if (group == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "Group not found." });
                }

                GroupHoverViewModel vm = new GroupHoverViewModel()
                {
                    GroupId = group.GroupId,
                    GroupLevel = group.Tier,
                    GroupMemberCount = group.MemberCount,
                    GroupPostCount = group.PostCount,
                    IsMember = group.IsMember,
                };

                string HTMLString = RenderPartialViewToString("_PartialGroupHover", model: vm);
                return Json(new { success = true, HTMLString });
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
                ViewContext viewContext = new ViewContext
                (ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        protected async Task<List<Post>> GetPosts(int id, int start, int count, string sort = "New")
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

            using (var db = new ZapContext())
            {
                DateTime t = DateTime.Now;
                var group = await db.Groups
                   .AsNoTracking()
                   .FirstOrDefaultAsync(g => g.GroupId == id).ConfigureAwait(true);

                if (sort == "Score")
                {
                    DateTime scoreStart = new DateTime(2018, 07, 01);
                    var sposts = await db.Posts//.AsNoTracking()
                        .Select(p => new
                        {
                            p,
                            s = Math.Abs((double)p.Score) < 1.0 ? 1.0 : Math.Abs((double)p.Score),    // Max (|x|,1)                                                           
                        })
                        .Select(p => new
                        {
                            p.p,
                            order = SqlFunctions.Log10(p.s),
                            sign = p.p.Score > 0.0 ? 1.0 : -1.0,                              // Sign of s
                            dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.p.TimeStamp),    // time since start
                        })
                        .Select(p => new
                        {
                            p.p,
                            hot = (p.sign * p.order) + (p.dt / 90000),
                        })
                        .OrderByDescending(p => p.hot)
                        .Select(p => p.p)
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
                        .Where(p => !p.IsDeleted)
                        .Where(p => !p.IsDraft)
                        .Where(p => p.Group.GroupId == group.GroupId)
                        .Skip(start)
                        .Take(count).ToListAsync().ConfigureAwait(true);
                    return sposts;
                }
                //if (sort == "New")
                //{

                var posts = await db.Posts//.AsNoTracking()
                    .OrderByDescending(p => p.TimeStamp)
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
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft)
                    .Where(p => p.Group.GroupId == group.GroupId)
                    .Skip(start)
                    .Take(count).ToListAsync().ConfigureAwait(true);
                return posts;
                //}
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchstr"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult Search(string searchstr)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }

            using (var db = new ZapContext())
            {
                searchstr = searchstr.Replace("\"", "");
                //.Select(g => g.GroupName)
                var matched = db.Groups.Where(g => g.GroupName.Contains(searchstr) || g.Tags.Contains(searchstr)).AsNoTracking().Take(30).ToList();
                var gis = matched.Select(g => new GroupInfo()
                {
                    Name = g.GroupName,
                    Icon = g.Icon,
                    Tags = g.Tags != null ? g.Tags.Split(',').ToList() : new List<string>(),
                    Id = g.GroupId,
                }).ToList();
                var gi = new GroupsViewModel()
                {
                    Groups = gis,
                };
                return Json(new { groups = gis }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TotalEarned"></param>
        /// <param name="TotalEarnedToDistribute"></param>
        /// <param name="Tier"></param>
        /// <returns></returns>
        protected static int GetGroupProgress(double TotalEarned, double TotalEarnedToDistribute, double Tier)
        {
            var e = TotalEarned + TotalEarnedToDistribute;
            //var level = GetGroupLevel(g);

            if (Tier == 0)
            {
                return Convert.ToInt32(100.0 * e / 1000.0);
            }
            if (Tier == 1)
            {
                return Convert.ToInt32(100.0 * (e - 1000.0) / 10000.0);
            }
            if (Tier == 2)
            {
                return Convert.ToInt32(100.0 * (e - 10000.0) / 50000.0);
            }
            if (Tier == 3)
            {
                return Convert.ToInt32(100.0 * (e - 50000.0) / 200000.0);
            }
            if (Tier == 4)
            {
                return Convert.ToInt32(100.0 * (e - 200000.0) / 500000.0);
            }
            if (Tier == 5)
            {
                return Convert.ToInt32(100.0 * (e - 500000.0) / 1000000.0);
            }
            if (Tier == 6)
            {
                return Convert.ToInt32(100.0 * (e - 1000000.0) / 5000000.0);
            }
            if (Tier == 7)
            {
                return Convert.ToInt32(100.0 * (e - 5000000.0) / 10000000.0);
            }
            if (Tier == 8)
            {
                return Convert.ToInt32(100.0 * (e - 10000000.0) / 20000000.0);
            }
            if (Tier == 9)
            {
                return Convert.ToInt32(100.0 * (e - 20000000.0) / 50000000.0);
            }
            return 100;
        }

        /// <summary>
        /// Returns the tier of the group
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        protected static int GetGroupLevel(double e)
        {
            //259 641.6
            //var e = g.TotalEarned + g.TotalEarnedToDistribute;

            if (e < 1000)
            {
                return 0;
            }
            if (e < 10000)
            {
                return 1;
            }
            if (e < 50000)
            {
                return 2;
            }
            if (e < 200000)
            {
                return 3;
            }
            if (e < 500000)
            {
                return 4;
            }
            if (e < 1000000)
            {
                return 5;
            }
            if (e < 5000000)
            {
                return 6;
            }
            if (e < 10000000)
            {
                return 7;
            }
            if (e < 20000000)
            {
                return 8;
            }
            if (e < 50000000)
            {
                return 9;
            }
            return 10;
        }

        /// <summary>
        /// View posts in a group.
        /// </summary>
        /// <param name="id">Group id</param>
        /// <returns></returns>
        [HttpGet]
        [MvcSiteMapNodeAttribute(Title = "Details", ParentKey = "Group", DynamicNodeProvider = "zapread.com.DI.GroupsDetailsProvider, zapread.com")]
        public async Task<ActionResult> GroupDetail(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(actionName:"Index", controllerName:"Home");
            }

            var userAppId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        IsIgnored = u.IgnoredGroups.Select(gr => gr.GroupId).Contains(id.Value),
                        u.Id,
                        u.AppId,
                        FundsBalance = u.Funds.Balance,
                        SubscribedGroups = u.Groups
                            .OrderByDescending(g => g.TotalEarned + g.TotalEarnedToDistribute)
                            .Select(g => new GroupInfo()
                            {
                                Id = g.GroupId,
                                Name = g.GroupName,
                                Icon = g.Icon,
                                Level = 1,
                                Progress = 36,
                                IsMod = g.Moderators.Select(m => m.Id).Contains(u.Id),
                                IsAdmin = g.Administrators.Select(m => m.Id).Contains(u.Id),
                            }),
                    })
                    .AsNoTracking()
                    .SingleOrDefaultAsync().ConfigureAwait(true);

                int userId = user == null ? 0 : user.Id;

                var group = await db.Groups
                    .Where(g => g.GroupId == id)
                    .Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        g.ShortDescription,
                        g.Icon,
                        g.Tags,
                        g.TotalEarned,
                        g.TotalEarnedToDistribute,
                        g.Tier,
                        NumMembers = g.Members.Count,
                        isMember = userId == 0 ? false : g.Members.Select(mb => mb.Id).Contains(userId),
                        g.CreationDate,
                        IsGroupAdmin = userId == 0 ? false : g.Administrators.Select(usr => usr.Id).Contains(userId),
                        IsGroupMod = userId == 0 ? false : g.Moderators.Select(usr => usr.Id).Contains(userId),
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                IQueryable<Post> validposts = QueryHelpers.QueryValidPosts(
                    userLanguages: null, 
                    db: db, 
                    userInfo: null);

                var groupPosts = QueryHelpers.OrderPostsByNew(validposts, group.GroupId, true);

                // Check tier
                // TODO - move to periodic check
                //var level = GetGroupLevel(group.TotalEarned + group.TotalEarnedToDistribute);
                //if (group.Tier < level)
                //{
                //    //group.Tier = level;
                //    //db.SaveChanges();
                //}

                List<string> tags = group.Tags != null ? group.Tags.Split(',').ToList() : new List<string>();

                bool isMember = group.isMember;

                var gi = new GroupInfo()
                {
                    Id = group.GroupId,
                    CreatedddMMMYYYY = group.CreationDate == null ? "2 Aug 2018" : group.CreationDate.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                    Name = group.GroupName,
                    NumMembers = group.NumMembers,
                    Tags = tags,
                    Icon = group.Icon != null ? "fa-" + group.Icon : "fa-bolt",
                    Level = group.Tier,
                    IsMember = isMember,
                    IsLoggedIn = user != null,
                };

                var vm = new GroupViewModel()
                {
                    HasMorePosts = groupPosts.Count() < 10 ? false : true,
                    SubscribedGroups = user == null ? new List<GroupInfo>() : user.SubscribedGroups.ToList(),
                    Posts = await QueryHelpers.QueryPostsVm(
                        start: 0, 
                        count: 10, 
                        postquery: groupPosts, 
                        userInfo: new QueryHelpers.PostQueryUserInfo()
                        {
                            Id = userId,
                            AppId = user?.AppId,
                        }).ConfigureAwait(true),
                    GroupId = group.GroupId,
                    IsMember = group.isMember,
                    NumMembers = group.NumMembers,
                    GroupName = group.GroupName,
                    ShortDescription = group.ShortDescription,
                    Icon = group.Icon,
                    Tier = group.Tier,
                    TotalEarned = group.TotalEarned,
                    TotalEarnedToDistribute = group.TotalEarnedToDistribute,
                    UserBalance = user == null ? 0 : user.FundsBalance,
                    IsGroupAdmin = group.IsGroupAdmin,
                    IsGroupMod = group.IsGroupMod,
                    IsIgnored = user == null ? false : user.IsIgnored,
                    Tags = group.Tags,
                };

                return View(vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult GroupAdminBar(string groupId)
        {
            ViewBag.groupId = groupId;
            using (var db = new ZapContext())
            {
                int gid = Convert.ToInt32(groupId);
                var group = db.Groups.AsNoTracking()
                    .SingleOrDefault(g => g.GroupId == gid);

                var vm = new GroupAdminBarViewModel()
                {
                    GroupId = Convert.ToInt32(groupId),
                    Tier = group.Tier,
                };

                return PartialView("_PartialGroupAdminBar", model: vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult AdminAddUserToGroupRoleForm(int groupId)
        {
            using (var db = new ZapContext())
            {
                var g = db.Groups.FirstOrDefault(grp => grp.GroupId == groupId);
                var groupName = "";

                if (g == null)
                {
                    // TODO: handle group not found
                }

                groupName = g.GroupName;
                var vm = new AddUserToGroupRoleViewModel();
                vm.GroupName = groupName;
                return PartialView("_PartialAddUserToGroupRoleForm", vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult ChangeName(int groupId, string newName)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.AppId == uid);

                if (user == null)
                {
                    return Json(new { result = "error", success = false, message = "User not authorized." });
                }

                var g = db.Groups.FirstOrDefault(grp => grp.GroupId == groupId);
                if (g == null)
                {
                    return Json(new { result = "error", success = false, message = "Group not found in database." });
                }

                if (!g.Administrators.Select(a => a.Id).Contains(user.Id))
                {
                    return Json(new { result = "error", success = false, message = "User not authorized." });
                }

                var cleanName = newName.CleanUnicode().SanitizeXSS();

                if (db.Groups.Select(grp => grp.GroupName).Contains(cleanName))
                {
                    return Json(new { result = "error", success = false, message = "Group name already used." });
                }

                g.GroupName = cleanName;

                db.SaveChanges();

                return Json(new { result = "success", success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="newDesc"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<JsonResult> ChangeShortDesc(int groupId, string newDesc)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = await db.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.AppId == uid).ConfigureAwait(true);

                if (user == null)
                {
                    return Json(new { result = "error", success = false, message = "User not authorized." });
                }

                var g = await db.Groups.SingleOrDefaultAsync(grp => grp.GroupId == groupId).ConfigureAwait(true);
                if (g == null)
                {
                    return Json(new { result = "error", success = false, message = "Group not found in database." });
                }

                if (!g.Administrators.Select(a => a.Id).Contains(user.Id))
                {
                    return Json(new { result = "error", success = false, message = "User not authorized." });
                }

                var cleanName = newDesc.CleanUnicode().SanitizeXSS();

                if (cleanName.Length > 60)
                {
                    return Json(new { result = "error", success = false, message = "Group short description must be 60 characters or less." });
                }

                g.ShortDescription = cleanName;

                db.SaveChanges();

                return Json(new { result = "success", success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="isAdmin"></param>
        /// <param name="isMod"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult UpdateUserGroupRoles(string group, string user, bool isAdmin, bool isMod)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var u = db.Users.Where(usr => usr.Name == user).FirstOrDefault();
                if (u == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var g = db.Groups.Where(grp => grp.GroupName == group).FirstOrDefault();
                if (g == null)
                {
                    return Json(new { success = false });
                }

                // Verify calling user is a group admin or site admin
                var userId = User.Identity.GetUserId();

                var callingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(usr => usr.AppId == userId);

                if (!g.Administrators.Select(m => m.Id).Contains(callingUser.Id))
                {
                    return Json(new { success = false, message = "You do not have administration privilages for this group." });
                }

                if (isAdmin)
                {
                    g.Administrators.Add(u);
                    u.GroupAdministration.Add(g);
                }
                else
                {
                    if (g.Administrators.Select(a => a.Id).Contains(u.Id))
                    {
                        g.Administrators.Remove(u);
                        u.GroupAdministration.Remove(g);
                    }
                }
                if (isMod)
                {
                    g.Moderators.Add(u);
                    u.GroupModeration.Add(g);
                }
                else
                {
                    if (g.Moderators.Select(m => m.Id).Contains(u.Id))
                    {
                        g.Moderators.Remove(u);
                        u.GroupModeration.Remove(g);
                    }
                }

                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        /// <summary>
        /// Query the DB for users which are a member of the group starting with the prefix
        /// This method can only be called by a group admin
        /// </summary>
        /// <param name="group"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult GetUsers(string group, string prefix)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupName == group).FirstOrDefault();
                if (g == null)
                {
                    return Json(new { success = false });
                }

                // Verify calling user is a group admin or site admin of the group
                var userId = User.Identity.GetUserId();

                var callingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(usr => usr.AppId == userId);

                if (!g.Moderators.Select(m => m.Id).Contains(callingUser.Id))
                {
                    return Json(new { success = false });
                }

                var matched = g.Members.Where(u => u.Name.StartsWith(prefix)).Select(u => u.Name).Take(30).ToList();
                return Json(matched, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult GetUserGroupRoles(string group, string user)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            var roles = new List<string>();

            using (var db = new ZapContext())
            {
                var u = db.Users.Where(usr => usr.Name == user).FirstOrDefault();
                if (u == null)
                {
                    return Json(roles);
                }
                if (u.GroupAdministration.Select(g => g.GroupName).Contains(group))
                {
                    roles.Add("Administrator");
                }
                if (u.GroupModeration.Select(g => g.GroupName).Contains(group))
                {
                    roles.Add("Moderator");
                }
                if (u.Groups.Select(g => g.GroupName).Contains(group))
                {
                    roles.Add("Member");
                }
            }
            return Json(roles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult GetGroupIcons(int groupId)
        {
            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupId == groupId).AsNoTracking().FirstOrDefault();
                if (g == null)
                {
                    return PartialView();
                }

                var icons = db.Icons.Select(i => i.Icon).ToList();
                var vm = new GroupAdminIconsViewModel()
                {
                    Icons = icons,
                    GroupId = groupId,
                    Icon = g.Icon,
                };
                return PartialView("_PartialGroupEditIcon", vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Group/GetGroups")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<JsonResult> GetGroups(string prefix, int max)
        {
            using (var db = new ZapContext())
            {
                var query = db.Groups
                    .Select(g => new
                    {
                        g.GroupName,
                        g.GroupId,
                        g.Tags,
                        g.TotalEarned,
                        g.TotalEarnedToDistribute,
                        Icon = g.Icon != null ? "fa-" + g.Icon : null,
                        //ImageId = g.GroupImage == null ? 3 : g.GroupImage.ImageId,
                        numMembers = g.Members.Count,
                    });

                if (String.IsNullOrEmpty(prefix))
                {
                    query.OrderByDescending(g => g.numMembers);
                }
                else
                {
#pragma warning disable CA1304 // Specify CultureInfo
                    query = query.Where(g => g.GroupName.ToLower().Contains(prefix.ToLower()) || g.Tags.ToLower().Contains(prefix.ToLower()))
#pragma warning restore CA1304 // Specify CultureInfo
                        .OrderByDescending(g => g.TotalEarned + g.TotalEarnedToDistribute); 
                }

                var matched = await query.Take(max)
                    .ToListAsync().ConfigureAwait(false);

                return Json(matched);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult GetGroupTags(int groupId)
        {
            var vm = new GroupAdminTagsViewModel();
            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupId == groupId).AsNoTracking().FirstOrDefault();
                if (g == null)
                {
                    return PartialView();
                }

                vm.Tags = g.Tags;
                vm.GroupId = g.GroupId;
            }
            return PartialView("_PartialGroupEditTags", vm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        [Route("Group/Icon/Update")]
        public ActionResult UpdateGroupIcon(int groupId, string icon)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            // calling user id
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, message = "User not authorized."});
            }

            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupId == groupId)
                    .Include(gr => gr.Administrators)
                    .FirstOrDefault();
                if (g == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "Group not found." });
                }

                // Ensure user is admin
                if (!g.Administrators.Select(a => a.AppId).Contains(userAppId))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(new { success = false, message = "User not authorized." });
                }

                g.Icon = icon.CleanUnicode().SanitizeXSS();
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult UpdateGrouptags(int groupId, string tags)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return PartialView();
                }

                g.Tags = tags.CleanUnicode().SanitizeXSS();
                db.SaveChanges();
            }
            return Json(new { result = "success" });
        }

        // GET: Group/New
        /// <summary>
        /// This is the controller for creating new groups
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
            }
            using (var db = new ZapContext())
            {
                NewGroupViewModel vm = new NewGroupViewModel();
                vm.Icons = db.Icons.Select(i => i.Icon).ToList();

                // List of languages known
                var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                    .GroupBy(ci => ci.TwoLetterISOLanguageName)
                    .Select(g => g.First())
                    .Select(ci => ci.Name + ":" + ci.NativeName).ToList();

                vm.Language = "en";
                vm.Languages = languages;

                return View(vm);
            }
        }

        // GET: Group/Edit
        /// <summary>
        /// Edit settings for an existing group
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
            }
            using (var db = new ZapContext())
            {
                NewGroupViewModel vm = new NewGroupViewModel();
                vm.Icons = db.Icons.Select(i => i.Icon).ToList();

                // List of languages known
                var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                    .GroupBy(ci => ci.TwoLetterISOLanguageName)
                    .Select(g => g.First())
                    .Select(ci => ci.Name + ":" + ci.NativeName).ToList();

                vm.Language = "en";
                vm.Languages = languages;

                return View(vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(NewGroupViewModel m)
        {
            if (!ModelState.IsValid)
            {
                // Validation error - send back to user
                using (var db = new ZapContext())
                {
                    m.Icons = db.Icons.Select(i => i.Icon).ToList();

                    // List of languages known
                    var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                        .GroupBy(ci => ci.TwoLetterISOLanguageName)
                        .Select(g => g.First())
                        .Select(ci => ci.Name + ":" + ci.NativeName).ToList();

                    m.Languages = languages;

                    return View(m);
                }
            }

            using (var db = new ZapContext())
            {
                var userId = User.Identity.GetUserId();

                // Ensure not a duplicate group!
                var cleanName = m.GroupName.CleanUnicode().Trim();

                if (db.Groups.Select(grp => grp.GroupName).Contains(cleanName))
                {
                    ModelState.AddModelError("GroupName", "Group already exists!");
                    m.Icons = db.Icons.Select(i => i.Icon).ToList();
                    return View(m);
                }

                Group g = new Group()
                {
                    GroupName = cleanName,
                    TotalEarned = 0.0,
                    TotalEarnedToDistribute = 0.0,
                    Moderators = new List<User>(),
                    Members = new List<User>(),
                    Administrators = new List<User>(),
                    Tags = m.Tags,
                    Icon = m.Icon,
                    CreationDate = DateTime.UtcNow,
                    DefaultLanguage = m.Language,
                };

                var u = db.Users.Where(us => us.AppId == userId).First();

                g.Members.Add(u);
                g.Moderators.Add(u);
                g.Administrators.Add(u);

                db.Groups.Add(g);
                db.SaveChanges();

                return RedirectToAction(actionName: "GroupDetail", routeValues: new { id = g.GroupId });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult ToggleIgnore(int groupId)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new HttpUnauthorizedResult("User not authorized");
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = db.Users
                    //.Include(u => u.Groups)
                    .FirstOrDefault(u => u.AppId == userId);

                var group = db.Groups
                    .Include(g => g.Members)
                    .FirstOrDefault(g => g.GroupId == groupId);

                bool added = false;

                if (group != null)
                {
                    if (!user.IgnoredGroups.Contains(group))
                    {
                        user.IgnoredGroups.Add(group);
                        added = true;
                    }
                    else
                    {
                        user.IgnoredGroups.Remove(group);
                    }

                    if (!group.Ignoring.Contains(user))
                    {
                        group.Ignoring.Add(user);
                        added = true;
                    }
                    else
                    {
                        group.Ignoring.Remove(user);
                    }
                    db.SaveChanges();
                }
                return Json(new { success=true, result = "success", added });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> JoinGroup(int gid)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            if (!ModelState.IsValid)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            var userId = User.Identity.GetUserId();

            // if userId is null, then it is anonymous

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.Groups)
                    .FirstOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(false);

                var group = await db.Groups
                    .Include(g => g.Members)
                    .FirstOrDefaultAsync(g => g.GroupId == gid).ConfigureAwait(false);

                if (group != null)
                {
                    if (!user.Groups.Contains(group))
                    {
                        user.Groups.Add(group);
                    }
                    if (!group.Members.Contains(user))
                    {
                        group.Members.Add(user);
                    }
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            return Json(new { success = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> LeaveGroup(int gid)
        {
            if (!Request.ContentType.Contains("json"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Bad request type." });
            }
            if (!ModelState.IsValid)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.Groups)
                    .FirstOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(false);

                var group = await db.Groups
                    .Include(g => g.Members)
                    .FirstOrDefaultAsync(g => g.GroupId == gid).ConfigureAwait(false);

                if (group != null)
                {
                    if (user.Groups.Contains(group))
                    {
                        user.Groups.Remove(group);
                    }
                    if (group.Members.Contains(user))
                    {
                        group.Members.Remove(user);
                    }
                }
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            return Json(new { success = true });
        }
    }
}