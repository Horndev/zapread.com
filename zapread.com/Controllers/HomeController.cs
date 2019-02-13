using HtmlAgilityPack;
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
using System.Globalization;
using LightningLib.lndrpc;
using zapread.com.Models.Database;
using System.Text;
using System.Data.Entity.SqlServer;

namespace zapread.com.Controllers
{
    public class HomeController : Controller
    {
        private static DateTime lastLNCheck = DateTime.Now;

        [Route("robots.txt", Name = "GetRobotsText"), OutputCache(Duration = 86400)]
        public ContentResult RobotsText()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("user-agent: *");
            stringBuilder.AppendLine("disallow: ");
            //stringBuilder.Append("sitemap: ");
            //stringBuilder.AppendLine(this.Url.RouteUrl("GetSitemapXml", null, this.Request.Url.Scheme).TrimEnd('/'));

            return this.Content(stringBuilder.ToString(), "text/plain", Encoding.UTF8);
        }

        //[Route("sitemap.xml", Name = "GetSitemapXml"), OutputCache(Duration = 86400)]
        //public ContentResult SitemapXml()
        //{
        //}

        [OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult UserImage(int? size, string UserId)
        {
            if (size == null) size = 100;
            if (UserId != null)
            {
                // use the value passed
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
                // Alternative if userId was username
                else if (db.Users.Where(u => u.Name == UserId).Count() > 0)
                {
                    var userAppId = db.Users.Where(u => u.Name == UserId).FirstOrDefault().AppId;
                    var img = db.Users.Where(u => u.AppId == userAppId).First().ProfileImage;
                    if (img.Image != null)
                    {
                        Image png = Image.FromStream(new MemoryStream(img.Image));
                        Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size);
                        byte[] data = thumb.ToByteArray(ImageFormat.Png);

                        return File(data, "image/png");
                    }
                }

                // Use generated icon

                // Identicon
                //var icon = Identicon.FromValue(UserId, size: (int)size);
                //icon.SaveAsPng(ms);
                //return File(ms.ToArray(), "image/png");

                // RoboHash
                var imagesPath = Server.MapPath("~/bin");
                RoboHash.Net.RoboHash.ImageFileProvider = new RoboHash.Net.Internals.DefaultImageFileProvider(
                    basePath: imagesPath);
                var r = RoboHash.Net.RoboHash.Create(UserId);
                using (var image = r.Render(
                    set: null,
                    backgroundSet: RoboHash.Net.RoboConsts.Any,
                    color: null,
                    width: (int)size,
                    height: (int)size))
                {
                    Bitmap thumb = ImageExtensions.ResizeImage(image, (int)size, (int)size);
                    byte[] data = thumb.ToByteArray(ImageFormat.Png);
                    return File(data, "image/png");
                }
            }
        }

        protected async Task<List<Post>> GetPosts(int start, int count, string sort = "Score", int userId = 0)
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

            List<string> userLanguages;
            
            try
            {
                userLanguages = Request.UserLanguages.ToList().Select(l => l.Split(';')[0].Split('-')[0]).Distinct().ToList();

                if (userLanguages.Count() == 0)
                {
                    userLanguages.Add("en");
                }
            }
            catch
            {
                userLanguages = new List<string>() { "en" };
            }

            using (var db = new ZapContext())
            {
                DateTime t = DateTime.Now;

                var user = await db.Users
                    .Include(usr => usr.Settings)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (sort == "Score")
                {
                    IQueryable<Post> validposts;
                    if (userId > 0)
                    {
                        var ig = user.IgnoredGroups.Select(g => g.GroupId);
                        validposts = db.Posts.Where(p => !ig.Contains(p.Group.GroupId));

                        var allLang = user.Settings.ViewAllLanguages;

                        if (!allLang)
                        {
                            var languages = user.Languages == null ? new List<string>() { "en" } : user.Languages.Split(',').ToList();
                            validposts = validposts
                                .Where(p => p.Language == null || languages.Contains(p.Language));
                        }
                    }
                    else
                    {
                        validposts = db.Posts
                            .Where(p => p.Language == null || userLanguages.Contains(p.Language));
                    }

                    DateTime scoreStart = new DateTime(2018, 07, 01);

                    var sposts = await validposts
                        .Where(p => !p.IsDeleted)
                        .Where(p => !p.IsDraft)
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
                            hot = p.sign * p.order + p.dt / 90000
                        })
                        .OrderByDescending(p => p.hot)
                        .Select(p => p.p)
                        .Include(p => p.Group)
                        .Include(p => p.Comments)
                        .Include(p => p.Comments.Select(cmt => cmt.Parent))
                        .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                        .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                        .Include(p => p.Comments.Select(cmt => cmt.UserId))
                        .Include("UserId")
                        .AsNoTracking()
                        .Skip(start)
                        .Take(count)
                        .ToListAsync();

                    return sposts;
                }
                else if (sort == "Active")
                {
                    IQueryable<Post> validposts;
                    if (userId > 0)
                    {
                        var ig = user.IgnoredGroups.Select(g => g.GroupId);
                        validposts = db.Posts.Where(p => !ig.Contains(p.Group.GroupId));

                        var allLang = user.Settings.ViewAllLanguages;

                        if (!allLang)
                        {
                            var languages = user.Languages == null ? new List<string>() { "en" } : user.Languages.Split(',').ToList();
                            validposts = validposts
                                .Where(p => p.Language == null || languages.Contains(p.Language));
                        }
                    }
                    else
                    {
                        validposts = db.Posts
                            .Where(p => p.Language == null || userLanguages.Contains(p.Language));
                    }

                    DateTime scoreStart = new DateTime(2018, 07, 01);

                    // Use number of comments as score
                    var sposts = await validposts
                        .Where(p => !p.IsDeleted)
                        .Where(p => !p.IsDraft)
                        .Select(p => new
                        {
                            p,
                            s =  (Math.Abs((double)p.Comments.Count) < 1.0 ? 1.0 : 100000.0 * Math.Abs((double)p.Comments.Count)),    // Max (|x|,1)                                                           
                        })
                        .Select(p => new
                        {
                            p.p,
                            order = SqlFunctions.Log10(p.s),
                            sign = p.p.Comments.Count >= 0.0 ? 1.0 : -1.0,                              // Sign of s
                            dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.p.TimeStamp),    // time since start
                        })
                        .Select(p => new
                        {
                            p.p,
                            active = p.sign * p.order + p.dt / 2000000 // Reduced time effect
                        })
                        .OrderByDescending(p => p.active)
                        .Select(p => p.p)
                        .Include(p => p.Group)
                        .Include(p => p.Comments)
                        .Include(p => p.Comments.Select(cmt => cmt.Parent))
                        .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                        .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                        .Include(p => p.Comments.Select(cmt => cmt.UserId))
                        .Include("UserId")
                        .AsNoTracking()
                        .Skip(start)
                        .Take(count)
                        .ToListAsync();

                    return sposts;
                }
                else //(sort == "New")
                { 
                    IQueryable<Post> validposts;
                    if (userId > 0)
                    {
                        var ig = user.IgnoredGroups.Select(g => g.GroupId);
                        validposts = db.Posts.Where(p => !ig.Contains(p.Group.GroupId));

                        var allLang = user.Settings.ViewAllLanguages;

                        if (!allLang)
                        {
                            var languages = user.Languages == null ? new List<string>() { "en" } : user.Languages.Split(',').ToList();
                            validposts = validposts
                                .Where(p => p.Language == null || languages.Contains(p.Language));
                        }
                    }
                    else
                    {
                        validposts = db.Posts
                            .Where(p => p.Language == null || userLanguages.Contains(p.Language));
                    }

                    var posts = await validposts
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
                        .Skip(start)
                        .Take(count)
                        .ToListAsync();

                    return posts;
                }
            }
        }

        /// <summary>
        /// This method is run once when first launched to ensure database globals are in place.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Install()
        {
            using (var db = new ZapContext())
            {
                var zapreadGlobals = await db.ZapreadGlobals
                    .SingleOrDefaultAsync(i => i.Id == 1);

                // This is run only the first time the app is launched in the database.
                // The global entry should only be created once in the database.
                if (zapreadGlobals == null)
                {
                    // Initialize everything with zeros.
                    db.ZapreadGlobals.Add(new ZapReadGlobals()
                    {
                        Id = 1,
                        CommunityEarnedToDistribute = 0.0,
                        TotalDepositedCommunity = 0.0,
                        TotalEarnedCommunity = 0.0,
                        TotalWithdrawnCommunity = 0.0,
                        ZapReadEarnedBalance = 0.0,
                        ZapReadTotalEarned = 0.0,
                        ZapReadTotalWithdrawn = 0.0,
                        LNWithdraws = new List<LNTransaction>(),
                    });
                    db.SaveChanges();
                }
                return Json(new { result = "success" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DismissTour(int id)
        {
            if (SetOrUpdateUserTourCookie("hide") == "hide")
            {
                return Json(new { success = true, result = "success" });
            }
            return Json(new { success = false, result = "failure setting cookie" });
        }

        /// <summary>
        /// If the user is away for longer than 30 days, it presents the tour again.
        /// value is "hide"     : do not present the tour to the user
        ///          "show"     : present to user
        /// </summary>
        /// <returns></returns>
        private string SetOrUpdateUserTourCookie(string setValue = "", string defaultValue = "")
        {
            string cookieResultValue;

            //Check if user is returning
            if (HttpContext.Request.Cookies["ZapRead.com.Tour"] != null)
            {
                var cookie = HttpContext.Request.Cookies.Get("ZapRead.com.Tour");
                HttpContext.Response.Cookies.Remove("ZapRead.com.Tour");
                cookie.Expires = DateTime.Now.AddDays(365);   //update
                if (setValue != "")
                    cookie.Value = setValue;
                HttpContext.Response.SetCookie(cookie);
                cookieResultValue = cookie.Value;
            }
            else
            {
                HttpCookie cookie = new HttpCookie("ZapRead.com.Tour");
                cookie.Value = defaultValue;
                if (setValue != "")
                    cookie.Value = setValue;
                cookie.Expires = DateTime.Now.AddDays(365);
                HttpContext.Response.Cookies.Remove("ZapRead.com.Tour");
                HttpContext.Response.SetCookie(cookie);
                cookieResultValue = cookie.Value;
            }

            return cookieResultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sort">Score, New</param>
        /// <param name="l"></param>
        /// <param name="g">include subscribed groups null = yes</param>
        /// <param name="f">include subscribed followers null = yes</param>
        /// <returns></returns>
        [OutputCache(Duration = 600, VaryByParam = "*", Location=System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> Index(string sort, string l, int? g, int? f)
        {
            try
            {
                if (Request.IsAuthenticated && (l == null || l == "" || l == "0"))
                {
                    return RedirectToAction("Index", new { sort, l = "1", g, f });
                }
                else if (!Request.IsAuthenticated && (l == null || l == "" || l == "1"))
                {
                    return RedirectToAction("Index", new { sort, l = "0", g, f });
                }
            }
            catch
            {
                ; // Todo - fixup unit test
            }

            SetTourCookie();

            try
            {
                // Check for settled invoices which were not applied every 5 minutes
                if (DateTime.Now - lastLNCheck > TimeSpan.FromMinutes(5))
                {
                    lastLNCheck = DateTime.Now;
                    LndRpcClient lndClient;
                    using (var db = new ZapContext())
                    {
                        var gb = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                            .AsNoTracking()
                            .FirstOrDefault();

                        lndClient = new LndRpcClient(
                            host: gb.LnMainnetHost,
                            macaroonAdmin: gb.LnMainnetMacaroonAdmin,
                            macaroonRead: gb.LnMainnetMacaroonRead,
                            macaroonInvoice: gb.LnMainnetMacaroonInvoice);

                        // These are the unpaid invoices in database
                        var unpaidInvoices = db.LightningTransactions
                            .Where(t => t.IsSettled == false)
                            .Where(t => t.IsDeposit == true)
                            .Where(t => t.IsIgnored == false)
                            .Include(t => t.User)
                            .Include(t => t.User.Funds);

                        var website = await db.ZapreadGlobals
                        .SingleOrDefaultAsync(ix => ix.Id == 1);

                        //var invoiceDebug = unpaidInvoices.ToList();
                        foreach (var i in unpaidInvoices)
                        {
                            if (i.HashStr != null)
                            {
                                var inv = lndClient.GetInvoice(rhash: i.HashStr);
                                if (inv.settled != null && inv.settled == true)
                                {
                                    // Paid but not applied in DB
                                    var use = i.UsedFor;
                                    if (use == TransactionUse.VotePost)
                                    {
                                        if (false) // Disable for now
                                        {
                                            var vc = new VoteController();
                                            var v = new VoteController.Vote()
                                            {
                                                a = Convert.ToInt32(i.Amount),
                                                d = i.UsedForAction == TransactionUseAction.VoteDown ? 0 : 1,
                                                Id = i.UsedForId,
                                                tx = i.Id
                                            };
                                            await vc.Post(v);

                                            i.IsSpent = true;
                                            i.IsSettled = true;
                                            i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                        }
                                    }
                                    else if (use == TransactionUse.VoteComment)
                                    {
                                        ;
                                    }
                                    else if (use == TransactionUse.UserDeposit)
                                    {
                                        if (i.User == null)
                                        {
                                            // Not sure how to deal with this other than add funds to Community
                                            website.CommunityEarnedToDistribute += i.Amount;
                                            i.IsSpent = true;
                                            i.IsSettled = true;
                                            i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                        }
                                        else
                                        {
                                            // Deposit funds in user account
                                            var user = i.User;
                                            user.Funds.Balance += i.Amount;
                                            i.IsSettled = true;
                                            i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;

                                        }
                                    }
                                    else if (use == TransactionUse.Undefined)
                                    {
                                        if (i.User == null)
                                        {
                                            // Not sure how to deal with this other than add funds to Community
                                            website.CommunityEarnedToDistribute += i.Amount;
                                            i.IsSpent = true;
                                            i.IsSettled = true;
                                            i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                        }
                                        else
                                        {
                                            // Not sure what the user was doing - deposit into their account.
                                            ;
                                        }
                                    }
                                }
                                else
                                {
                                    // Not settled - check expiry
                                    var t1 = Convert.ToInt64(inv.creation_date);
                                    var tNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                    var tExpire = t1 + Convert.ToInt64(inv.expiry) + 10000; //Add a buffer time
                                    if (tNow > tExpire)
                                    {
                                        // Expired - let's stop checking this invoice
                                        i.IsIgnored = true;
                                    }
                                    else
                                    {
                                        int w = 1; // keep waiting
                                    }
                                }
                            }
                            else
                            {
                                // No hash string to look it up.  Must be an error somewhere.
                                i.IsIgnored = true;
                            }
                        }

                        db.SaveChanges();
                    }
                }

                using (var db = new ZapContext())
                {
                    User user = await GetCurrentUser(db);
                    var posts = await GetPosts(0, 10, sort ?? "Score", user != null ? user.Id : 0);
                    ValidateClaims(user);

                    List<GroupInfo> gi = GetUserGroups(user);
                    List<int> viewerIgnoredUsers = GetUserIgnoredUsers(user);

                    List<PostViewModel> postViews = new List<PostViewModel>();
                   
                    var groups = await db.Groups
                        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .AsNoTracking()
                        .ToListAsync();

                    foreach (var p in posts)
                    {
                        postViews.Add(new PostViewModel()
                        {
                            Post = p,
                            ViewerIsMod = user != null ? user.GroupModeration.Select(grp => grp.GroupId).Contains(p.Group.GroupId) : false,
                            ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                            ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                            ViewerIgnoredUser = user != null ? (user.IgnoringUsers != null ? p.UserId.Id != user.Id && user.IgnoringUsers.Select(usr => usr.Id).Contains(p.UserId.Id) : false) : false,
                            NumComments = 0,

                            ViewerIgnoredUsers = viewerIgnoredUsers,

                            GroupMemberCounts = groups.ToDictionary(i => i.GroupId, i => i.mc),
                            GroupPostCounts = groups.ToDictionary(i => i.GroupId, i => i.pc),
                            GroupLevels = groups.ToDictionary(i => i.GroupId, i => i.l),
                        });
                    }

                    PostsViewModel vm = new PostsViewModel()
                    {
                        Posts = postViews,
                        UserBalance = user == null ? 0 : Math.Floor(user.Funds.Balance),    // TODO: Should this be here?
                        Sort = sort == null ? "Score" : sort,
                        SubscribedGroups = gi,

                    };

                    return View(vm);
                }
            }
            catch (Exception e)
            {
                MailingService.Send(new UserEmailModel()
                {
                    Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                    Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n user: " + User.Identity.GetUserId() ?? "anonymous",
                    Email = "",
                    Name = "zapread.com Exception",
                    Subject = "Exception on index",
                });
                throw e;
            }
        }

        private static List<int> GetUserIgnoredUsers(User user)
        {
            List<int> viewerIgnoredUsers = new List<int>();

            if (user != null && user.IgnoringUsers != null)
            {
                viewerIgnoredUsers = user.IgnoringUsers.Select(usr => usr.Id).Where(usrid => usrid != user.Id).ToList();
            }

            return viewerIgnoredUsers;
        }

        private static List<GroupInfo> GetUserGroups(User user)
        {
            var gi = new List<GroupInfo>();
            if (user != null)
            {
                // Get list of user subscribed groups (with highest activity on top)
                int userid = user != null ? user.Id : 0;
                var userGroups = user.Groups
                    .Select(grp => new
                    {
                        IsModerator = grp.Moderators.Select(m => m.Id).Contains(userid),
                        IsAdmin = grp.Administrators.Select(m => m.Id).Contains(userid),
                        TotalIncome = grp.TotalEarned + grp.TotalEarnedToDistribute,
                        grp,
                    })
                    .OrderByDescending(grp => grp.TotalIncome)
                    .ToList();
                gi = userGroups.Select(grp => new GroupInfo()
                {
                    Id = grp.grp.GroupId,
                    Name = grp.grp.GroupName,
                    Icon = grp.grp.Icon,
                    Level = 1,
                    Progress = 36,
                    IsMod = grp.IsModerator,
                    IsAdmin = grp.IsAdmin,
                }).ToList();
            }
            return gi;
        }

        private void ValidateClaims(User user)
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

        private async Task<User> GetCurrentUser(ZapContext db)
        {
            string userid = null;

            if (User != null) // This is the case when testing unauthorized call
            {
                userid = User.Identity.GetUserId();
            }

            var user = await db.Users
                .Include(usr => usr.Settings)
                .Include(usr => usr.IgnoringUsers)
                .Include(usr => usr.Groups)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AppId == userid);
            return user;
        }

        private void SetTourCookie()
        {
            ViewBag.ShowTourModal = false;
            try
            {
                if (SetOrUpdateUserTourCookie(defaultValue: "show") != "hide")
                {
                    // User has not dismissed tour request
                    ViewBag.ShowTourModal = true;
                }
            }
            catch
            {
                ; //TODO proper exception handling
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
        public async Task<ActionResult> InfiniteScroll(int BlockNumber, string sort)
        {
            int BlockSize = 10;
            
            using (var db = new ZapContext())
            {
                var uid = User.Identity.GetUserId();
                var user = await db.Users.AsNoTracking()
                    .SingleOrDefaultAsync(u => u.AppId == uid);

                var posts = await GetPosts(BlockNumber, BlockSize, sort, user != null ? user.Id : 0);

                string PostsHTMLString = "";

                var groups = await db.Groups
                        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .AsNoTracking()
                        .ToListAsync();

                foreach (var p in posts)
                {
                    var pvm = new PostViewModel()
                    {
                        Post = p,
                        ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                        ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                        NumComments = 0,
                        GroupMemberCounts = groups.ToDictionary(i => i.GroupId, i => i.mc),
                        GroupPostCounts = groups.ToDictionary(i => i.GroupId, i => i.pc),
                        GroupLevels = groups.ToDictionary(i => i.GroupId, i => i.l),
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

        [HttpPost]
        public ActionResult SendFeedback(string msg, string loc)
        {
            String uid = "";

            uid = User.Identity.GetUserId();

            UserEmailModel message = new UserEmailModel();
            message.Email = "";
            message.Name = "ZapRead Feedback";
            message.Subject = "ZapRead Feedback";
            message.Body = msg + Environment.NewLine + " Location: " + loc + Environment.NewLine + Environment.NewLine + " User: " + uid;
            message.Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];
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

            model.Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];
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
                var user = await db.Users
                    .Include(usr => usr.ProfileImage)
                    .Include(usr => usr.ThumbImage)
                    .Include(usr => usr.Funds)
                    .Include(usr => usr.Settings)
                    .Where(u => u.AppId == userId)
                    .SingleOrDefaultAsync();

                if (user == null)
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
                else
                {
                    // ensure all properties are not null
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
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}