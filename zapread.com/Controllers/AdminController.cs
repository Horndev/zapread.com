using LightningLib.lndrpc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Hubs;
using zapread.com.Models;

namespace zapread.com.Controllers
{
    [Authorize(Roles = "Administrator", Users ="Zelgada")]
    public class AdminController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public AdminController()
        {

        }

        public AdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        #region Lightning Payments Admin

        /// <summary>
        /// This method checks all of the unpaid invoices posted in the database against the lnd node to see if they have been paid
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyInvoices()
        {
            var lndClient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings["LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings["LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings["LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings["LnMainnetMacaroonInvoice"]);

            using (var db = new ZapContext())
            {
                // These are the unpaid invoices
                var unpaidInvoices = db.LightningTransactions
                    .Where(t => t.IsSettled == false)
                    .Where(t => t.IsDeposit == true)
                    .Include(t => t.User);

                foreach(var i in unpaidInvoices)
                {
                    if (i.HashStr != null)
                    {
                        var inv = lndClient.GetInvoice(rhash: i.HashStr);
                        if (inv.settled != null && inv.settled == true)
                        {
                            // Paid but not applied in DB

                            var use = i.UsedFor;
                            if (use != TransactionUse.Undefined)
                            {
                                // Use case is recorded in database - perform action
                                var useid = i.UsedForId;

                                // Trigger any async listeners
                                if (use == TransactionUse.UserDeposit)
                                {
                                    var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                                    var user = i.User;
                                    double userBalance = 0.0;

                                    if (user == null)
                                    {
                                        // this should not happen? - verify.  Maybe this is the case for transactions related to votes?
                                        // throw new Exception("Error accessing user information related to settled LN Transaction.");
                                        //int z = 0;
                                    }
                                    else
                                    {
                                        // Update user balance - this is a deposit.
                                        // user.Funds.Balance += i.Amount;
                                        // userBalance = Math.Floor(user.Funds.Balance);
                                        // db.SaveChanges();
                                    }

                                    // Notify clients the invoice was paid.
                                    context.Clients.All.NotifyInvoicePaid(new { invoice = i.PaymentRequest, balance = userBalance, txid = i.Id });
                                }
                                else if (use == TransactionUse.Tip)
                                {

                                }
                                else if (use == TransactionUse.VotePost)
                                {

                                }
                                else if (use == TransactionUse.VoteComment)
                                {

                                }
                            }
                            else
                            {
                                // We can't perform any action on the invoice, but we should mark it as settled.
                                // Unfortunately, we don't know who paid the invoice so we can't credit the funds to any account.
                                // The lost funds should probably go to community pot in that case.

                                i.IsSettled = true;
                                i.TimestampSettled = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc) + TimeSpan.FromSeconds(Convert.ToInt64(inv.settle_date)); 
                            }
                        }
                        else if (inv.settled != null && inv.settled == false)
                        {
                            // Still waiting.

                            // TODO 
                        }
                    }
                    else
                    {
                        // Darn, the hashstring wasn't recorded for some reason.  Can't look up the invoice in LND.

                        // Hide this transaction from appearing next time.
                        i.IsSettled = true;
                        i.TimestampSettled = DateTime.UtcNow;
                    }
                }
                db.SaveChangesAsync();
            }
            return Json(new { result="success" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Icons
        public ActionResult Icons()
        {
           return View();
        }

        public class DataItem
        {
            public string Icon { get; set; }
            public string Graphic { get; set; }
            public int Id { get; set; }
        }

        public ActionResult AddIcon(string icon)
        {
            using (var db = new ZapContext())
            {
                if (icon == null)
                {
                    return Json(new { });
                }
                ZapIcon newIcon = new ZapIcon()
                {
                    Icon = icon,
                    Lib = "fa",
                    NumUses = 0,
                };
                db.Icons.Add(newIcon);
                db.SaveChanges();
            }
            return Json(new { });
        }

        public ActionResult DeleteIcon(int Id)
        {
            using (var db = new ZapContext())
            {
                var icon = db.Icons.Where(i => i.Id == Id).FirstOrDefault();

                if (icon == null)
                {
                    Json(new { result = "error" });
                }

                db.Icons.Remove(icon);
                db.SaveChanges();

                return Json(new { result = "success" });
            }
        }

        public ActionResult GetIcons(DataTableParameters dataTableParameters)
        {
            using (var db = new ZapContext())
            {
                var icons = db.Icons.OrderByDescending(i => i.Id).Skip(dataTableParameters.Start).Take(dataTableParameters.Length)
                    .ToList();

                var values = icons.Select(i => new DataItem()
                {
                    Icon = i.Icon,
                    Graphic = i.Icon,
                    Id = i.Id,
                }).ToList();


                int numrec = db.Icons.Count();

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
        #endregion

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetPostStats()
        {
            var endDate = DateTime.UtcNow;

            // The starting point for statistics
            var startDate = endDate.AddDays(-1 * 31);

            // The binning
            DateGroupType bin = DateGroupType.Day;

            using (var db = new ZapContext())
            {
                var allPosts = db
                    .Posts
                    .Where(x => x.TimeStamp > startDate && x.TimeStamp <= endDate);

                var allComments = db
                    .Comments
                    .Where(x => x.TimeStamp > startDate && x.TimeStamp <= endDate);

                var allSpends = db.SpendingEvents.Where(x => x.TimeStamp > startDate && x.TimeStamp <= endDate);

                var binnedPostStats = GroupPostsByDate(allPosts, bin, startDate);
                var binnedCommentStats = GroupCommentsByDate(allComments, bin, startDate);
                var binnedSpendingStats = GroupSpendsByDate(allSpends, bin, startDate);

                DateTime epochUTC = new DateTime(1970, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);

                var postStats = binnedPostStats.Select(x => new
                    {
                        x.Key,
                        Count = x.Count()
                    }).ToList()
                    .Select(x => new Stat
                    {
                        //TimeStamp = GetDate(bin, x.Key.Value, startDate),
                        TimeStampUtc = Convert.ToInt64((GetDate(bin, x.Key.Value, startDate) - epochUTC).TotalMilliseconds),
                        Count = x.Count
                    })
                    .OrderBy(x => x.TimeStampUtc)
                    .ToList();

                var commentStats = binnedCommentStats.Select(x => new
                    {
                        x.Key,
                        Count = x.Count()
                    }).ToList()
                    .Select(x => new Stat
                    {
                        //TimeStamp = GetDate(bin, x.Key.Value, startDate),
                        TimeStampUtc = Convert.ToInt64((GetDate(bin, x.Key.Value, startDate) - epochUTC).TotalMilliseconds),
                        Count = x.Count
                    })
                    .OrderBy(x => x.TimeStampUtc)
                    .ToList();

                var spendingStats = binnedSpendingStats.Select(x => new
                {
                    x.Key,
                    //Count = x.Count()
                    Sum = x.Sum(y => y.Amount)
                }).ToList()
                    .Select(x => new Stat
                    {
                        //TimeStamp = GetDate(bin, x.Key.Value, startDate),
                        TimeStampUtc = Convert.ToInt64((GetDate(bin, x.Key.Value, startDate) - epochUTC).TotalMilliseconds),
                        Count = Convert.ToInt32(x.Sum)
                    })
                    .OrderBy(x => x.TimeStampUtc)
                    .ToList();

                var maxPosts = postStats.Max(x => x.Count);
                var maxComments = commentStats.Max(x => x.Count);
                var maxPostComments = maxPosts > maxComments ? maxPosts : maxComments;
                var maxSpent = spendingStats.Max(x => x.Count);

                return Json(new { postStats, commentStats, spendingStats, maxPostComments, maxSpent }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Roles/Index" });
            }

            AdminViewModel vm;

            using (var db = new ZapContext())
            {
                var globals = db.ZapreadGlobals.Where(g => g.Id == 1)
                    .AsNoTracking()
                    .FirstOrDefault();

                var gd = db.Groups.Sum(g => g.TotalEarnedToDistribute);
                var LNdep = Convert.ToDouble(db.LightningTransactions.Where(t => t.IsSettled && t.IsDeposit).Sum(t => t.Amount))/100000000.0;
                var LNwth = Convert.ToDouble(db.LightningTransactions.Where(t => t.IsSettled && !t.IsDeposit).Sum(t => t.Amount)) / 100000000.0;

                // Calculate post and comment stats.
                var startDate = DateTime.Now.AddDays(-1 * 31);
                var allPosts = db
                    .Posts
                    .Where(x => x.TimeStamp > startDate);

                DateGroupType group = DateGroupType.Day;

                var groupedStats = GroupPostsByDate(allPosts, group, startDate);

                DateTime epochUTC = new DateTime(1970, 1, 1, 0,0,0, kind: DateTimeKind.Utc);

                var stats = groupedStats.Select(x => new
                    {
                        x.Key,
                        Count = x.Count()
                    })
                    .ToList()
                    .Select(x => new Stat
                    {
                        //TimeStamp = GetDate(group, x.Key.Value, startDate),
                        TimeStampUtc = Convert.ToInt64((GetDate(group, x.Key.Value, startDate) - epochUTC).TotalMilliseconds),
                        Count = x.Count
                    })
                    .OrderBy(x => x.TimeStampUtc)
                    .ToList();

                vm = new AdminViewModel()
                {
                    Globals = globals,
                    PendingGroupToDistribute = gd,
                    LNTotalDeposited = LNdep,
                    LNTotalWithdrawn = LNwth,
                };

                return View(vm);
            }
        }

        private IQueryable<IGrouping<int?, Post>> GroupPostsByDate(IQueryable<Post> products, DateGroupType group, DateTime startDate) 
        {
            switch (group)
            {
                case DateGroupType.Day:
                    return products.GroupBy(x => SqlFunctions.DateDiff("dd", startDate, x.TimeStamp));

                case DateGroupType.Week:
                    return products.GroupBy(x => SqlFunctions.DateDiff("ww", startDate, x.TimeStamp));

                case DateGroupType.Month:
                    return products.GroupBy(x => SqlFunctions.DateDiff("mm", startDate, x.TimeStamp));

                case DateGroupType.Quarter:
                    return products.GroupBy(x => SqlFunctions.DateDiff("qq", startDate, x.TimeStamp));

                case DateGroupType.Year:
                    return products.GroupBy(x => SqlFunctions.DateDiff("yy", startDate, x.TimeStamp));

                default:
                    throw new NotSupportedException($"Grouping by '{group}' is not supported");
            }
        }

        private IQueryable<IGrouping<int?, Comment>> GroupCommentsByDate(IQueryable<Comment> products, DateGroupType group, DateTime startDate)
        {
            switch (group)
            {
                case DateGroupType.Day:
                    return products.GroupBy(x => SqlFunctions.DateDiff("dd", startDate, x.TimeStamp));

                case DateGroupType.Week:
                    return products.GroupBy(x => SqlFunctions.DateDiff("ww", startDate, x.TimeStamp));

                case DateGroupType.Month:
                    return products.GroupBy(x => SqlFunctions.DateDiff("mm", startDate, x.TimeStamp));

                case DateGroupType.Quarter:
                    return products.GroupBy(x => SqlFunctions.DateDiff("qq", startDate, x.TimeStamp));

                case DateGroupType.Year:
                    return products.GroupBy(x => SqlFunctions.DateDiff("yy", startDate, x.TimeStamp));

                default:
                    throw new NotSupportedException($"Grouping by '{group}' is not supported");
            }
        }

        private IQueryable<IGrouping<int?, SpendingEvent>> GroupSpendsByDate(IQueryable<SpendingEvent> products, DateGroupType group, DateTime startDate)
        {
            switch (group)
            {
                case DateGroupType.Day:
                    return products.GroupBy(x => SqlFunctions.DateDiff("dd", startDate, x.TimeStamp));

                case DateGroupType.Week:
                    return products.GroupBy(x => SqlFunctions.DateDiff("ww", startDate, x.TimeStamp));

                case DateGroupType.Month:
                    return products.GroupBy(x => SqlFunctions.DateDiff("mm", startDate, x.TimeStamp));

                case DateGroupType.Quarter:
                    return products.GroupBy(x => SqlFunctions.DateDiff("qq", startDate, x.TimeStamp));

                case DateGroupType.Year:
                    return products.GroupBy(x => SqlFunctions.DateDiff("yy", startDate, x.TimeStamp));

                default:
                    throw new NotSupportedException($"Grouping by '{group}' is not supported");
            }
        }

        private DateTime GetDate(DateGroupType group, int diff, DateTime startDate)
        {
            switch (group)
            {
                case DateGroupType.Day:
                    return startDate.AddDays(diff);

                case DateGroupType.Week:
                    return startDate.AddDays(diff * 7);

                case DateGroupType.Month:
                    return startDate.AddMonths(diff);

                case DateGroupType.Quarter:
                    return startDate.AddMonths(diff * 3);

                case DateGroupType.Year:
                    return startDate.AddYears(diff);

                default:
                    throw new NotSupportedException($"Grouping by '{group}' is not supported");
            }
        }

        public PartialViewResult SiteAdminBarUserInfo(int userId)
        {
            using (var db = new ZapContext())
            {
                var vm = new SiteAdminBarUserInfoViewModel();

                if (!User.Identity.IsAuthenticated)
                {
                    return PartialView("_PartialSiteAdminBarUserInfo", model: vm);
                }

                var u = db.Users.AsNoTracking()
                    .Include("Funds")
                    .Where(usr => usr.Id == userId)
                    .FirstOrDefault();

                if (u == null)
                {
                    return PartialView("_PartialSiteAdminBarUserInfo", model: vm);
                }

                vm.Balance = Convert.ToInt32(Math.Floor(u.Funds.Balance));
                vm.TotalEarned = Convert.ToInt32(u.TotalEarned);

                return PartialView("_PartialSiteAdminBarUserInfo", model: vm);
            }
        }

        public PartialViewResult SiteAdminBar(string viewInfo)
        {
            ViewBag.ViewInfo = viewInfo;
            return PartialView("_PartialSiteAdminBar");
        }

        public PartialViewResult AdminAddUserToGroupRoleForm(string groupName)
        {
            var vm = new AddUserToGroupRoleModel();
            vm.GroupName = groupName;
            return PartialView("_PartialAddUserToGroupRoleForm", vm);
        }

        // Query the DB for all users starting with the prefix
        [HttpPost]
        public JsonResult GetUsers(string prefix)
        {
            using (var db = new ZapContext())
            {
                var matched = db.Users.Where(u => u.Name.StartsWith(prefix)).Select(u => u.Name).Take(30).ToList();
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

        [HttpPost]
        public JsonResult UpdateUserGroupRoles(string group, string user, bool isAdmin, bool isMod, bool isMember )
        {
            using (var db = new ZapContext())
            {
                var u = db.Users.Where(usr => usr.Name == user).FirstOrDefault();
                if (u == null)
                {
                    return Json(new { success = false, message = "User not found"});
                }

                var g = db.Groups.Where(grp => grp.GroupName == group).FirstOrDefault();
                if (g == null)
                {
                    return Json(new { success = false });
                }
                if (isAdmin)
                {
                    g.Administrators.Add(u);
                    u.GroupAdministration.Add(g);
                }
                else
                {
                    if (g.Administrators.Contains(u))
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
                    if (g.Moderators.Contains(u))
                    {
                        g.Moderators.Remove(u);
                        u.GroupModeration.Remove(g);
                    }
                }
                if (isMember)
                {
                    g.Members.Add(u);
                    u.Groups.Add(g);
                }
                else
                {
                    g.Members.Remove(u);
                    u.Groups.Remove(g);
                }
                db.SaveChanges();
            }
            return Json(new { success = true });
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
    }
}