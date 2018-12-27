using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using System.IO;

namespace zapread.com.Controllers
{
    public class GroupController : Controller
    {
        // GET: Group
        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            GroupsViewModel vm = new GroupsViewModel();

            var gi = new List<GroupInfo>();

            using (var db = new ZapContext())
            {
                var user = db.Users
                    //.Include(u => u.Groups)
                    .FirstOrDefault(u => u.AppId == userId);

                var groups = db.Groups
                    .Include(g => g.Members)
                    .OrderByDescending(g => g.TotalEarned + g.TotalEarnedToDistribute)
                    .Take(100).ToList();

                foreach(var g in groups)
                {
                    int num_members = g.Members != null ? g.Members.Count() : 0;

                    int num_posts = db.Posts.Where(p => p.Group != null).Where(p => p.Group.GroupId == g.GroupId).Count();

                    bool isMember = user == null ? false : g.Members.Contains(user);

                    List<string> tags = g.Tags != null ? g.Tags.Split(',').ToList() : new List<string>();

                    gi.Add(new GroupInfo()
                    {
                        Id = g.GroupId,
                        CreatedddMMMYYYY = g.CreationDate == null ? "2 Aug 2018" : g.CreationDate.Value.ToString("dd MMM yyyy"),
                        Name = g.GroupName,
                        NumMembers = num_members,
                        NumPosts = num_posts,
                        Tags = tags,
                        Icon = g.Icon != null ? "fa-" + g.Icon : "fa-bolt",
                        Level = g.Tier,
                        Progress = Convert.ToInt32(100.0*(g.TotalEarned+g.TotalEarnedToDistribute)/1000.0),
                        IsMember = isMember,
                        IsLoggedIn = user != null,
                    });
                }
                vm.TotalPosts = db.Posts.Count().ToString("N0");
            }

            vm.Groups = gi;
            
            return View(vm);
        }

        // GET: Group/Members/1
        public ActionResult Members(int id)
        {
            var userId = User.Identity.GetUserId();
            GroupViewModel vm = new GroupViewModel();

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.AppId == userId);

                var group = db.Groups
                    .Include(g => g.Members)
                    .AsNoTracking()
                    .FirstOrDefault(g => g.GroupId == id);

                List<string> tags = group.Tags != null ? group.Tags.Split(',').ToList() : new List<string>();

                bool isMember = user == null ? false : group.Members.Contains(user);

                var gi = new GroupInfo()
                {
                    Id = group.GroupId,
                    CreatedddMMMYYYY = group.CreationDate == null ? "2 Aug 2018" : group.CreationDate.Value.ToString("dd MMM yyyy"),
                    Name = group.GroupName,
                    //NumMembers = num_members,
                    //NumPosts = num_posts,
                    Tags = tags,
                    Icon = group.Icon != null ? "fa-" + group.Icon : "fa-bolt",
                    Level = group.Tier,
                    //Progress = Convert.ToInt32(100.0 * g.TotalEarned / 0.1),
                    IsMember = isMember,
                    IsLoggedIn = user != null,
                    Members = group.Members.ToList(),
                    
                };

                vm.GroupInfo = gi;
                vm.Group = group;
                vm.UserBalance = user == null ? 0 : user.Funds.Balance;
                vm.IsGroupAdmin = user == null ? false : group.Administrators.Select(usr => usr.Id).Contains(user.Id);
                vm.IsGroupMod = user == null ? false : group.Moderators.Select(usr => usr.Id).Contains(user.Id);
                vm.Tags = group.Tags;
                vm.Posts = new List<PostViewModel>();
                vm.Upvoted = new List<int>();
                vm.Downvoted = new List<int>();
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult InfiniteScroll(int id, int BlockNumber, string sort)
        {
            int BlockSize = 10;
            var posts = GetPosts(id, BlockNumber, BlockSize, sort);
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.AppId == uid);

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

        protected List<Post> GetPosts(int id, int start, int count, string sort = "New")
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
                var group = db.Groups
                   .AsNoTracking()
                   .FirstOrDefault(g => g.GroupId == id);

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
                        .Where(p => p.Group.GroupId == group.GroupId)
                        .Skip(start)
                        .Take(count).ToList();
                    return sposts;
                }
                //if (sort == "New")
                //{

                var posts = db.Posts//.AsNoTracking()
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
                    .Where(p => p.Group.GroupId == group.GroupId)
                    .Skip(start)
                    .Take(count).ToList();
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
                    Tags = g.Tags.Split(',').ToList(),
                    Id = g.GroupId,
                }).ToList();
                var gi = new GroupsViewModel()
                {
                    Groups = gis,
                };
                return Json(new { groups = gis }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> DoPayouts()
        {
            int maxDistributions = 1000;    // Per group
            int minDistributionSize = 1;    // Go as low as 1 Satoshi


            using (var db = new ZapContext())
            {

                // GROUP PAYOUTS
                var gids = db.Groups.Select(g => g.GroupId).ToList();
                double distributed = 0.0;
                double toDistribute = 0.0;
                int numDistributions = 0;

                Dictionary<int, double> payoutUserAmount = new Dictionary<int, double>();

                foreach (var gid in gids)
                {
                    payoutUserAmount.Clear();
                    
                    var g = db.Groups.FirstOrDefault(grp => grp.GroupId == gid);
                    toDistribute = Math.Floor(g.TotalEarnedToDistribute);
                    numDistributions = Convert.ToInt32(Math.Min(toDistribute / minDistributionSize, maxDistributions));

                    if (numDistributions > 0)
                    {
                        var groupPostsRecent = db.Posts
                            .Include("Group")
                            .Where(p => p.Group.GroupId == gid && p.Score > 0 && DbFunctions.DiffDays(DateTime.UtcNow, p.TimeStamp) <= 30).ToList();
                        var groupPostsOld = db.Posts.Where(p => p.Group.GroupId == gid && p.Score > 0 && DbFunctions.DiffDays(DateTime.UtcNow, p.TimeStamp) > 30).ToList();

                        var numPostsOld = groupPostsOld.Count();
                        var numPostsNew = groupPostsRecent.Count();

                        var newFrac = numPostsOld == 0 ? 1.0 : 0.5;
                        var oldFrac = numPostsOld == 0 ? 0.0 : 0.5;

                        List<Post> postsToDistribute;

                        if (numPostsNew < numDistributions)
                        {
                            // Too few posts, so each post is selected
                            postsToDistribute = groupPostsRecent;
                        }
                        else
                        {
                            // Need to Randomly choose posts to distribute to
                            Random rnd = new Random();
                            postsToDistribute = groupPostsRecent.OrderBy(ps => rnd.Next()).Take(numDistributions).ToList();
                        }

                        double totalScores = 1.0 * postsToDistribute.Select(p => p.Score).Sum();
                        foreach (var p in postsToDistribute)
                        {
                            var score = Math.Max(1.0 * p.Score, 0.0);
                            var frac = score / totalScores;
                            var earnedAmount = newFrac * frac * toDistribute;
                            if (earnedAmount > 0)
                            {
                                var owner = p.UserId;
                                if (owner != null)
                                {
                                    // Record user payment to be paid out later.
                                    if (payoutUserAmount.ContainsKey(owner.Id))
                                    {
                                        payoutUserAmount[owner.Id] += earnedAmount;
                                    }
                                    else
                                    {
                                        payoutUserAmount[owner.Id] = earnedAmount;
                                    }
                                }
                            }
                        }

                        if (numPostsOld < numDistributions)
                        {
                            // Too few posts, so each post is selected
                            postsToDistribute = groupPostsOld;
                        }
                        else
                        {
                            // Need to Randomly choose posts to distribute to
                            Random rnd = new Random();
                            postsToDistribute = groupPostsOld.OrderBy(ps => rnd.Next()).Take(numDistributions).ToList();
                        }

                        // Too few posts, so each post is selected
                        totalScores = 1.0 * postsToDistribute.Select(p => p.Score).Sum();
                        foreach (var p in postsToDistribute)
                        {
                            var score = 1.0 * p.Score;
                            var frac = score / totalScores;
                            var earnedAmount = oldFrac * frac * toDistribute;

                            var owner = p.UserId;
                            if (owner != null)
                            {
                                // Record and increment user payment to be saved to DB later.
                                if (payoutUserAmount.ContainsKey(owner.Id))
                                {
                                    payoutUserAmount[owner.Id] += earnedAmount;
                                }
                                else
                                {
                                    payoutUserAmount[owner.Id] = earnedAmount;
                                }
                            }
                        }
                        distributed = 0.0;

                        foreach (var uid in payoutUserAmount.Keys)
                        {
                            // This is where payouts should be made for each user link to group
                            var owner = db.Users.FirstOrDefault(u => u.Id == uid);
                            double earnedAmount = payoutUserAmount[uid];
                            var ea = new EarningEvent()
                            {
                                Amount = earnedAmount,
                                OriginType = 0, // 0 = post
                                TimeStamp = DateTime.UtcNow,
                                Type = 1, // 1 = group
                                Id = gid, // Indicates the group which generated the payout
                            };
                            owner.EarningEvents.Add(ea);
                            owner.TotalEarned += earnedAmount;
                            owner.Funds.Balance += earnedAmount;
                            distributed += earnedAmount;
                        }

                        // record distributions to group
                        g.TotalEarnedToDistribute -= distributed;
                        g.TotalEarned += distributed;

                        await db.SaveChangesAsync();
                    }
                }

                // Do community payouts
                payoutUserAmount.Clear();
                
                var website = db.ZapreadGlobals.FirstOrDefault(i => i.Id == 1);
                toDistribute = Math.Floor(website.CommunityEarnedToDistribute);

                if (toDistribute < 0)
                {
                    toDistribute = 0;

                    // Send error
                    Services.MailingService.Send(new UserEmailModel()
                    {
                        Body = "Error during community distribution.  Total to distribute is negative.",
                        Destination = "steven.horn.mail@gmail.com",
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "Community payout error",
                    });
                }

                numDistributions = Convert.ToInt32(Math.Min(toDistribute / minDistributionSize, maxDistributions));
                if (numDistributions > 0)
                {
                    var sitePostsRecent = db.Posts
                        .Where(p => p.Score > 0 && DbFunctions.DiffDays(DateTime.UtcNow, p.TimeStamp) <= 30).ToList();
                    var sitePostsOld = db.Posts.Where(p => p.Score > 0 && DbFunctions.DiffDays(DateTime.UtcNow, p.TimeStamp) > 30).ToList();

                    var numPostsOld = sitePostsOld.Count();
                    var numPostsNew = sitePostsRecent.Count();

                    var newFrac = numPostsOld == 0 ? 1.0 : 0.5;
                    var oldFrac = numPostsOld == 0 ? 0.0 : 0.5;

                    List<Post> postsToDistribute;

                    if (numPostsNew < numDistributions)
                    {
                        // Too few posts, so each post is selected
                        postsToDistribute = sitePostsRecent;
                    }
                    else
                    {
                        // Need to Randomly choose posts to distribute to
                        Random rnd = new Random();
                        postsToDistribute = sitePostsRecent.OrderBy(ps => rnd.Next()).Take(numDistributions).ToList();
                    }

                    double totalScores = 1.0 * postsToDistribute.Select(p => p.Score).Sum();
                    foreach (var p in postsToDistribute)
                    {
                        var score = Math.Max(1.0 * p.Score, 0.0);
                        var frac = score / totalScores;
                        var earnedAmount = newFrac * frac * toDistribute;
                        if (earnedAmount > 0)
                        {
                            var owner = p.UserId;
                            if (owner != null)
                            {
                                // Record and increment user payment to be saved to DB later.
                                if (payoutUserAmount.ContainsKey(owner.Id))
                                {
                                    payoutUserAmount[owner.Id] += earnedAmount;
                                }
                                else
                                {
                                    payoutUserAmount[owner.Id] = earnedAmount;
                                }
                            }
                        }
                    }

                    if (numPostsOld < numDistributions)
                    {
                        // Too few posts, so each post is selected
                        postsToDistribute = sitePostsOld;
                    }
                    else
                    {
                        // Need to Randomly choose posts to distribute to
                        Random rnd = new Random();
                        postsToDistribute = sitePostsOld.OrderBy(ps => rnd.Next()).Take(numDistributions).ToList();
                    }

                    totalScores = 1.0 * postsToDistribute.Select(p => p.Score).Sum();
                    foreach (var p in postsToDistribute)
                    {
                        var score = 1.0 * p.Score;
                        var frac = score / totalScores;
                        var earnedAmount = oldFrac * frac * toDistribute;
                        var owner = p.UserId;
                        if (owner != null)
                        {
                            // Record and increment user payment to be saved to DB later.
                            if (payoutUserAmount.ContainsKey(owner.Id))
                            {
                                payoutUserAmount[owner.Id] += earnedAmount;
                            }
                            else
                            {
                                payoutUserAmount[owner.Id] = earnedAmount;
                            }
                        }
                    }

                    // apply distribution to DB
                    distributed = 0.0;
                    foreach (var uid in payoutUserAmount.Keys)
                    {
                        // This is where payouts should be made for each user link to group
                        var owner = db.Users.FirstOrDefault(u => u.Id == uid);
                        double earnedAmount = payoutUserAmount[uid];
                        var ea = new EarningEvent()
                        {
                            Amount = earnedAmount,
                            OriginType = 0, // 0 = post
                            TimeStamp = DateTime.UtcNow,
                            Type = 2,   // 2 = community
                            Id = 0,     // Indicates the group which generated the payout
                        };
                        owner.EarningEvents.Add(ea);
                        owner.TotalEarned += earnedAmount;
                        owner.Funds.Balance += earnedAmount;
                        distributed += earnedAmount;
                    }

                    //record distribution
                    website.CommunityEarnedToDistribute -= distributed;// toDistribute;
                    website.TotalEarnedCommunity += distributed;// toDistribute;

                    await db.SaveChangesAsync();
                }
            }

            return View();
        }

        public ActionResult GroupDetail(int id)
        {
            var userId = User.Identity.GetUserId();
            GroupViewModel vm = new GroupViewModel() {
                HasMorePosts = true,
            };

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.AppId == userId);

                var group = db.Groups
                    .AsNoTracking()
                    .FirstOrDefault(g => g.GroupId == id);

                var groupPosts = db.Posts
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
                    .Where(p => p.Group.GroupId == group.GroupId)
                    .OrderByDescending(p => new { p.IsSticky, p.TimeStamp })
                    .Take(10).ToList();

                if (groupPosts.Count() < 10)
                {
                    vm.HasMorePosts = false;
                }

                List<string> tags = group.Tags != null ? group.Tags.Split(',').ToList() : new List<string>();

                bool isMember = user == null ? false : group.Members.Contains(user);
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
                            //NumPosts = g.Posts.Count(),
                            //UserPosts = g.Posts.Where(p => p.UserId.Id == u.Id).Count(),
                            IsMod = g.Moderators.Contains(user),
                            IsAdmin = g.Administrators.Contains(user),
                        });
                    }
                }
                var gi = new GroupInfo()
                {
                    Id = group.GroupId,
                    CreatedddMMMYYYY = group.CreationDate == null ? "2 Aug 2018" : group.CreationDate.Value.ToString("dd MMM yyyy"),
                    Name = group.GroupName,
                    //NumMembers = num_members,
                    //NumPosts = num_posts,
                    Tags = tags,
                    Icon = group.Icon != null ? "fa-" + group.Icon : "fa-bolt",
                    Level = group.Tier,
                    //Progress = Convert.ToInt32(100.0 * g.TotalEarned / 0.1),
                    IsMember = isMember,
                    IsLoggedIn = user != null,
                };

                List<PostViewModel> postViews = new List<PostViewModel>();

                foreach (var p in groupPosts)
                {
                    postViews.Add(new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                    });
                }

                vm.SubscribedGroups = gis;
                vm.GroupInfo = gi;
                vm.Posts = postViews;
                //vm.Upvoted = user == null ? new List<int>() : user.PostVotesUp.Select(p => p.PostId).ToList();
                //vm.Downvoted = user == null ? new List<int>() : user.PostVotesDown.Select(p => p.PostId).ToList();
                vm.Group = group;
                vm.UserBalance = user == null ? 0 : user.Funds.Balance;
                vm.IsGroupAdmin = user == null ? false : group.Administrators.Select(usr => usr.Id).Contains(user.Id);
                vm.IsGroupMod = user == null ? false : group.Moderators.Select(usr => usr.Id).Contains(user.Id);
                vm.Tags = group.Tags;
            }
            return View(vm);
        }

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

        [HttpPost]
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
                    return Json(new { exists =  true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new { exists = false }, JsonRequestBehavior.AllowGet);
            }
        }

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

        public ActionResult UpdateGroupIcon(int groupId, string icon)
        {
            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return PartialView();
                }

                g.Icon = icon;
                db.SaveChanges();
            }
            return Json(new { result = "success" });
        }

        public ActionResult UpdateGrouptags(int groupId, string tags)
        {
            using (var db = new ZapContext())
            {
                var g = db.Groups.Where(grp => grp.GroupId == groupId).FirstOrDefault();
                if (g == null)
                {
                    return PartialView();
                }

                g.Tags = tags;
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
                //SeedIcons(m);
                using (var db = new ZapContext())
                {
                    m.Icons = db.Icons.Select(i => i.Icon).ToList();
                    return View(m);
                }
            }

            using (var db = new ZapContext())
            {
                var userId = User.Identity.GetUserId();
                //EnsureUserExists(userId, db);
                Group g = new Group()
                {
                    GroupName = m.GroupName,
                    TotalEarned = 0.0,
                    TotalEarnedToDistribute = 0.0,
                    Moderators = new List<User>(),
                    Members = new List<User>(),
                    Tags = m.Tags,
                    Icon = m.Icon,
                    CreationDate = DateTime.UtcNow,
                };
                
                var u = db.Users.Where(us => us.AppId == userId).First();

                g.Members.Add(u);
                g.Moderators.Add(u);

                db.Groups.Add(g);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        //private void EnsureUserExists(string userId, ZapContext db)
        //{
        //    if (db.Users.Where(u => u.AppId == userId).Count() == 0)
        //    {
        //        // no user entry
        //        User u = new Models.User()
        //        {
        //            AboutMe = "Nothing to tell.",
        //            AppId = userId,
        //            Name = UserManager.FindById(userId).UserName,
        //            ProfileImage = new UserImage(),
        //            ThumbImage = new UserImage(),
        //        };
        //        db.Users.Add(u);
        //        db.SaveChanges();
        //    }
        //}

        public class jgModel
        {
            public int gid { get; set; }
        }

        [HttpPost]
        public ActionResult JoinGroup(jgModel m)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            var userId = User.Identity.GetUserId();

            // if userId is null, then it is anonymous

            using (var db = new ZapContext())
            {
                var user = db.Users
                    //.Include(u => u.Groups)
                    .FirstOrDefault(u => u.AppId == userId);

                var group = db.Groups
                    .Include(g => g.Members)
                    .FirstOrDefault(g => g.GroupId == m.gid);

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
                    db.SaveChanges();
                }
            }

            return Json(new { result = "success" });
        }

        [HttpPost]
        public ActionResult LeaveGroup(jgModel m)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { result = "error", message = "Invalid" });
            }

            var userId = User.Identity.GetUserId();

            // if userId is null, then it is anonymous

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Include(u => u.Groups)
                    .FirstOrDefault(u => u.AppId == userId);

                var group = db.Groups
                    .Include(g => g.Members)
                    .FirstOrDefault(g => g.GroupId == m.gid);

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
                db.SaveChanges();
            }

            return Json(new { result = "success" });
        }

        [HttpPost]
        public ActionResult CreateNewGroup(NewGroupViewModel m)
        {

            return RedirectToAction("Index","Home");
        }
    }
}