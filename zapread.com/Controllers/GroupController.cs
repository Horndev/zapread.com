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
using zapread.com.Models.Database;
using zapread.com.Models.GroupViews;

namespace zapread.com.Controllers
{
    public class GroupController : Controller
    {
        // GET: Group
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

        [HttpPost, Route("Group/GetGroupsTable")]
        public async Task<ActionResult> GetGroupsTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            using (var db = new ZapContext())
            {
                var sorts = dataTableParameters.Order;
                var search = dataTableParameters.Search;

                //var searchstr = searchstr.Resplace("\"", "");
                //var matched = db.Groups.Where(g => g.GroupName.Contains(searchstr) || g.Tags.Contains(searchstr)).AsNoTracking().Take(30).ToList();

                User user = await GetCurrentUser(db);
                ValidateClaims(user);
                int userid = user != null ? user.Id : 0;

                // Build query
                var groupsQ = db.Groups
                    .Select(g => new
                    {
                        numPosts = g.Posts.Count(),
                        numMembers = g.Members.Count(),
                        IsMember = g.Members.Select(m => m.Id).Contains(userid),
                        IsModerator = g.Moderators.Select(m => m.Id).Contains(userid),
                        IsAdmin = g.Administrators.Select(m => m.Id).Contains(userid),
                        g,
                    }).AsNoTracking();

                if (search.Value != null)
                {
                    groupsQ = groupsQ.Where(g => g.g.GroupName.Contains(search.Value) || g.g.Tags.Contains(search.Value));
                }

                groupsQ = groupsQ.OrderByDescending(g => g.g.TotalEarned + g.g.TotalEarnedToDistribute);

                var groups = await groupsQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length).ToListAsync();


                var values = groups.Select(g => new GroupInfo()
                {
                    Id = g.g.GroupId,
                    CreatedddMMMYYYY = g.g.CreationDate == null ? "2 Aug 2018" : g.g.CreationDate.Value.ToString("dd MMM yyyy"),
                    Name = g.g.GroupName,
                    NumMembers = g.numMembers,
                    NumPosts = g.numPosts,
                    Tags = g.g.Tags != null ? g.g.Tags.Split(',').ToList() : new List<string>(),
                    Icon = g.g.Icon != null ? "fa-" + g.g.Icon : "fa-bolt",
                    Level = g.g.Tier,
                    Progress = GetGroupProgress(g.g),
                    IsMember = g.IsMember,
                    IsLoggedIn = user != null,
                    IsMod = g.IsModerator,
                    IsAdmin = g.IsAdmin,
                }).ToList();

                //// Build our query
                //var pageUsersQS = db.Users
                //    .Include("Funds")
                //    .Include("Posts")
                //    .Include("Comments");
                //IOrderedQueryable<User> pageUsersQ = null;// pageUsersQS.OrderByDescending(q => q.Id);

                //foreach (var s in sorts)
                //{
                //    if (s.Dir == "asc")
                //    {
                //        if (dataTableParameters.Columns[s.Column].Name == "DateJoined")
                //            pageUsersQ = pageUsersQS.OrderBy(q => q.DateJoined);
                //        else if (dataTableParameters.Columns[s.Column].Name == "LastSeen")
                //            pageUsersQ = pageUsersQS.OrderBy(q => q.DateLastActivity);
                //        else if (dataTableParameters.Columns[s.Column].Name == "NumPosts")
                //            pageUsersQ = pageUsersQS.OrderBy(q => q.Posts.Count);
                //        else if (dataTableParameters.Columns[s.Column].Name == "NumComments")
                //            pageUsersQ = pageUsersQS.OrderBy(q => q.Comments.Count);
                //        else if (dataTableParameters.Columns[s.Column].Name == "Balance")
                //            pageUsersQ = pageUsersQS.OrderBy(q => q.Funds == null ? 0 : q.Funds.Balance);
                //    }
                //    else
                //    {
                //        if (dataTableParameters.Columns[s.Column].Name == "DateJoined")
                //            pageUsersQ = pageUsersQS.OrderByDescending(q => q.DateJoined);
                //        else if (dataTableParameters.Columns[s.Column].Name == "LastSeen")
                //            pageUsersQ = pageUsersQS.OrderByDescending(q => q.DateLastActivity);
                //        else if (dataTableParameters.Columns[s.Column].Name == "NumPosts")
                //            pageUsersQ = pageUsersQS.OrderByDescending(q => q.Posts.Count);
                //        else if (dataTableParameters.Columns[s.Column].Name == "NumComments")
                //            pageUsersQ = pageUsersQS.OrderByDescending(q => q.Comments.Count);
                //        else if (dataTableParameters.Columns[s.Column].Name == "Balance")
                //            pageUsersQ = pageUsersQS.OrderByDescending(q => q.Funds == null ? 0 : q.Funds.Balance);
                //    }
                //}

                //if (pageUsersQ == null)
                //{
                //    pageUsersQ = pageUsersQS.OrderByDescending(q => q.Id);
                //}

                //var pageUsers = await pageUsersQ
                //    .Skip(dataTableParameters.Start)
                //    .Take(dataTableParameters.Length)
                //    .ToListAsync();

                //var values = pageUsers.AsParallel()
                //    .Select(u => new GroupInfo()
                //    {
                //        UserName = u.Name,
                //        DateJoined = u.DateJoined != null ? u.DateJoined.Value.ToString("o") : "?",
                //        LastSeen = u.DateLastActivity != null ? u.DateLastActivity.Value.ToString("o") : "?",
                //        NumPosts = u.Posts.Count.ToString(),
                //        NumComments = u.Comments.Count.ToString(),
                //        Balance = ((u.Funds != null ? u.Funds.Balance : 0) / 100000000.0).ToString("F8"),
                //        Id = u.AppId,
                //    }).ToList();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = await db.Groups.CountAsync(),
                    recordsFiltered = await groupsQ.CountAsync(),
                    data = values
                };
                return Json(ret);
            }
        }

        public class GroupDataItem
        {
            public string Icon { get; set; }
            public string Name { get; set; }
            public string Tags { get; set; }
            public string Progress { get; set; }
            public string Tier { get; set; }
            public string Members { get; set; }
            public string Posts { get; set; }
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
                .FirstOrDefaultAsync(u => u.AppId == userId);
            return user;
        }

        // GET: Group/Members/1
        public async Task<ActionResult> Members(int id)
        {
            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.Settings)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.AppId == userId);

                var group = await db.Groups
                    .Include(g => g.Members)
                    .Include(g => g.Moderators)
                    .Include(g => g.Administrators)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupId == id);

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

        [HttpPost]
        public async Task<ActionResult> UpdateUserMakeAdmin(int id, int groupId)
        {
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

                await db.SaveChangesAsync();
            }
            return Json(new { success = true, result = "success" });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateUserRevokeAdmin(int id, int groupId)
        {
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

                await db.SaveChangesAsync();
            }
            return Json(new { success = true, result = "success" });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateUserMakeMod(int id, int groupId)
        {
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

                await db.SaveChangesAsync();
            }
            return Json(new { success = true, result = "success" });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateUserRevokeMod(int id, int groupId)
        {
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

                await db.SaveChangesAsync();
            }
            return Json(new { success = true, result = "success" });
        }

        [HttpPost]
        public async Task<ActionResult> InfiniteScroll(int id, int BlockNumber, string sort)
        {
            int BlockSize = 10;
            var posts = await GetPosts(id, BlockNumber, BlockSize, sort);
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.AppId == uid);

                string PostsHTMLString = "";
                List<int> viewerIgnoredUsers = new List<int>();

                if (user != null && user.IgnoringUsers != null)
                {
                    viewerIgnoredUsers = user.IgnoringUsers.Select(usr => usr.Id).Where(usid => usid != user.Id).ToList();
                }

                foreach (var p in posts)
                {
                    var pvm = new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        NumComments = 0,

                        ViewerIgnoredUsers = viewerIgnoredUsers,
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
                   .FirstOrDefaultAsync(g => g.GroupId == id);

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
                        .Take(count).ToListAsync();
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
                    .Take(count).ToListAsync();
                return posts;
                //}
            }
        }

        [HttpPost]
        public ActionResult Search(string searchstr)
        {
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

        protected static int GetGroupProgress(Group g)
        {
            var e = g.TotalEarned + g.TotalEarnedToDistribute;
            var level = GetGroupLevel(g);

            if (g.Tier == 0)
            {
                return Convert.ToInt32(100.0 * e / 1000.0);
            }
            if (g.Tier == 1)
            {
                return Convert.ToInt32(100.0 * (e - 1000.0) / 10000.0);
            }
            if (g.Tier == 2)
            {
                return Convert.ToInt32(100.0 * (e - 10000.0) / 50000.0);
            }
            if (g.Tier == 3)
            {
                return Convert.ToInt32(100.0 * (e - 50000.0) / 200000.0);
            }
            if (g.Tier == 4)
            {
                return Convert.ToInt32(100.0 * (e - 200000.0) / 500000.0);
            }
            if (g.Tier == 5)
            {
                return Convert.ToInt32(100.0 * (e - 500000.0) / 1000000.0);
            }
            if (g.Tier == 6)
            {
                return Convert.ToInt32(100.0 * (e - 1000000.0) / 5000000.0);
            }
            if (g.Tier == 7)
            {
                return Convert.ToInt32(100.0 * (e - 5000000.0) / 10000000.0);
            }
            if (g.Tier == 8)
            {
                return Convert.ToInt32(100.0 * (e - 10000000.0) / 20000000.0);
            }
            if (g.Tier == 9)
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
        protected static int GetGroupLevel(Group g)
        {
            //259 641.6
            var e = g.TotalEarned + g.TotalEarnedToDistribute;

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

        [HttpGet]
        [MvcSiteMapNodeAttribute(Title = "Details", ParentKey = "Group", DynamicNodeProvider = "zapread.com.DI.GroupsDetailsProvider, zapread.com")]
        public async Task<ActionResult> GroupDetail(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(actionName:"Index", controllerName:"Home");
            }

            var userId = User.Identity.GetUserId();
            GroupViewModel vm = new GroupViewModel()
            {
                HasMorePosts = true,
            };

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(usr => usr.IgnoringUsers)
                    .Include(usr => usr.Groups)
                    .Include(usr => usr.Groups.Select(grp => grp.Moderators))
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(false);

                var group = await db.Groups
                    .FirstOrDefaultAsync(g => g.GroupId == id).ConfigureAwait(false);

                var groupPosts = db.Posts
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
                    .OrderByDescending(p => new { p.IsSticky, p.TimeStamp })
                    .Take(10).ToList();

                if (groupPosts.Count < 10)
                {
                    vm.HasMorePosts = false;
                }

                // Check tier
                var level = GetGroupLevel(group);

                if (group.Tier < level)
                {
                    group.Tier = level;
                    db.SaveChanges();
                }

                List<string> tags = group.Tags != null ? group.Tags.Split(',').ToList() : new List<string>();

                bool isMember = user == null ? false : group.Members.Select(mb => mb.Id).Contains(user.Id);
                var gis = new List<GroupInfo>();

                if (user != null)
                {
                    var userGroups = user.Groups
                    .OrderByDescending(g => g.TotalEarned + g.TotalEarnedToDistribute)
                    .ToList();

                    foreach (var g in userGroups)
                    {
                        gis.Add(new GroupInfo()
                        {
                            Id = g.GroupId,
                            Name = g.GroupName,
                            Icon = g.Icon,
                            Level = 1,
                            Progress = 36,
                            IsMod = g.Moderators.Select(m => m.Id).Contains(user.Id),
                            IsAdmin = g.Administrators.Select(m => m.Id).Contains(user.Id),
                        });
                    }
                }
                var gi = new GroupInfo()
                {
                    Id = group.GroupId,
                    CreatedddMMMYYYY = group.CreationDate == null ? "2 Aug 2018" : group.CreationDate.Value.ToString("dd MMM yyyy"),
                    Name = group.GroupName,
                    NumMembers = group.Members.Count,
                    Tags = tags,
                    Icon = group.Icon != null ? "fa-" + group.Icon : "fa-bolt",
                    Level = group.Tier,
                    //Progress = Convert.ToInt32(100.0 * g.TotalEarned / 0.1),
                    IsMember = isMember,
                    IsLoggedIn = user != null,
                };

                List<PostViewModel> postViews = new List<PostViewModel>();

                List<int> viewerIgnoredUsers = new List<int>();

                if (user != null && user.IgnoringUsers != null)
                {
                    viewerIgnoredUsers = user.IgnoringUsers.Select(usr => usr.Id).Where(uid => uid != user.Id).ToList();
                }

                foreach (var p in groupPosts)
                {
                    postViews.Add(new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerIgnoredUser = user != null ? (user.IgnoringUsers != null ? p.UserId.Id != user.Id && user.IgnoringUsers.Select(usr => usr.Id).Contains(p.UserId.Id) : false) : false,
                        NumComments = 0,

                        ViewerIgnoredUsers = viewerIgnoredUsers,
                    });
                }

                vm.SubscribedGroups = gis;
                vm.GroupInfo = gi;
                vm.Posts = postViews;
                vm.Group = group;
                vm.UserBalance = user == null ? 0 : user.Funds.Balance;
                vm.IsGroupAdmin = user == null ? false : group.Administrators.Select(usr => usr.Id).Contains(user.Id);
                vm.IsGroupMod = user == null ? false : group.Moderators.Select(usr => usr.Id).Contains(user.Id);
                vm.IsIgnored = user == null ? false : user.IgnoredGroups.Select(gr => gr.GroupId).Contains(group.GroupId);
                vm.Tags = group.Tags;
            }
            return View(vm);
        }

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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult ChangeName(int groupId, string newName)
        {
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<JsonResult> ChangeShortDesc(int groupId, string newDesc)
        {
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = await db.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.AppId == uid);

                if (user == null)
                {
                    return Json(new { result = "error", success = false, message = "User not authorized." });
                }

                var g = await db.Groups.SingleOrDefaultAsync(grp => grp.GroupId == groupId);
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult UpdateUserGroupRoles(string group, string user, bool isAdmin, bool isMod)
        {
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

        // Query the DB for users which are a member of the group starting with the prefix
        // This method can only be called by a group admin
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult GetUsers(string group, string prefix)
        {
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public JsonResult GetUserGroupRoles(string group, string user)
        {
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

        [HttpGet]
        [Route("Group/GetGroups/{prefix}")]
        public JsonResult GetGroups(string prefix)
        {
            using (var db = new ZapContext())
            {
                var matched = db.Groups.Where(g => g.GroupName.StartsWith(prefix)).Select(g => new { g.GroupName, g.GroupId }).Take(30).ToList();
                return Json(matched, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GroupExists(string gn)
        {
            using (var db = new ZapContext())
            {
                var matched = db.Groups.Where(g => g.GroupName == gn).FirstOrDefault();
                if (matched != null)
                {
                    return Json(new { exists = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { exists = false }, JsonRequestBehavior.AllowGet);
            }
        }

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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult UpdateGroupIcon(int groupId, string icon)
        {
            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return PartialView();
                }

                g.Icon = icon.CleanUnicode().SanitizeXSS();
                db.SaveChanges();
            }
            return Json(new { result = "success" });
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult UpdateGrouptags(int groupId, string tags)
        {
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
                var cleanName = m.GroupName.CleanUnicode();

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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult ToggleIgnore(int groupId)
        {
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
                return Json(new { result = "success", added });
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> JoinGroup(int gid)
        {
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> LeaveGroup(int gid)
        {
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