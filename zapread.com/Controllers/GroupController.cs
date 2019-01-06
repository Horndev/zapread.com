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
using zapread.com.Helpers;

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
                    .Include(u => u.Settings)
                    .FirstOrDefault(u => u.AppId == userId);

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
                        Progress = GetGroupProgress(g),
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
            using (var db = new ZapContext())
            {
                var group = db.Groups
                    .Include(g => g.Members)
                    .Include(g => g.Moderators)
                    .Include(g => g.Administrators)
                    .AsNoTracking()
                    .FirstOrDefault(g => g.GroupId == id);

                List<GroupMemberViewModel> groupMembers = new List<GroupMemberViewModel>();

                foreach (var m in group.Members)
                {
                    groupMembers.Add(new GroupMemberViewModel()
                    {
                        UserName = m.Name,
                        AboutMe = m.AboutMe,
                        AppId = m.AppId,
                        IsModerator = group.Moderators.Select(u => u.Id).Contains(m.Id),
                        IsGroupAdministrator = group.Administrators.Select(u => u.Id).Contains(m.Id),
                        IsSiteAdministrator = m.Name == "Zelgada",
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

        protected int GetGroupProgress(Group g)
        {
            var e = g.TotalEarned + g.TotalEarnedToDistribute;
            var level = GetGroupLevel(g);

            if (g.Tier == 0)
            {
                return Convert.ToInt32(100.0 * e / 1000.0);
            }
            if (g.Tier == 1)
            {
                return Convert.ToInt32(100.0 * (e-1000.0) / 10000.0);
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
            return 100;// Convert.ToInt32(100.0 * (g.TotalEarned + g.TotalEarnedToDistribute) / 1000.0);
        }

        /// <summary>
        /// Returns the tier of the group
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        protected int GetGroupLevel(Group g)
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

        public ActionResult GroupDetail(int id)
        {
            var userId = User.Identity.GetUserId();
            GroupViewModel vm = new GroupViewModel() {
                HasMorePosts = true,
            };

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Include(usr => usr.IgnoringUsers)
                    .Include(usr => usr.Groups)
                    .Include(usr => usr.Groups.Select(grp => grp.Moderators))
                    .AsNoTracking()
                    .SingleOrDefault(u => u.AppId == userId);

                var group = db.Groups
                    //.AsNoTracking()
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
                    NumMembers = group.Members.Count(),
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
            var vm = new GroupAdminBarViewModel()
            {
                GroupId = Convert.ToInt32(groupId),
            };

            return PartialView("_PartialGroupAdminBar", model: vm);
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
                var vm = new AddUserToGroupRoleModel();
                vm.GroupName = groupName;
                return PartialView("_PartialAddUserToGroupRoleForm", vm);
            }
        }

        [HttpPost]
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

                if (db.Groups.Select(grp => grp.GroupName).Contains(newName))
                {
                    return Json(new { result = "error", success = false, message = "Group name already used." });
                }

                g.GroupName = newName;

                db.SaveChanges();

                return Json(new { result = "success", success = true });
            }
        }

        [HttpPost]
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
                using (var db = new ZapContext())
                {
                    m.Icons = db.Icons.Select(i => i.Icon).ToList();
                    return View(m);
                }
            }

            using (var db = new ZapContext())
            {
                var userId = User.Identity.GetUserId();

                Group g = new Group()
                {
                    GroupName = m.GroupName,
                    TotalEarned = 0.0,
                    TotalEarnedToDistribute = 0.0,
                    Moderators = new List<User>(),
                    Members = new List<User>(),
                    Administrators = new List<User>(),
                    Tags = m.Tags,
                    Icon = m.Icon,
                    CreationDate = DateTime.UtcNow,
                };
                
                var u = db.Users.Where(us => us.AppId == userId).First();

                g.Members.Add(u);
                g.Moderators.Add(u);
                g.Administrators.Add(u);

                db.Groups.Add(g);
                db.SaveChanges();

                return RedirectToAction( actionName:"GroupDetail", routeValues: new { id = g.GroupId });
            }
        }

        public class jgModel
        {
            public int gid { get; set; }
        }

        [HttpPost]
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