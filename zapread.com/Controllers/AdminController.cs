using Hangfire;
using Hangfire.Storage;
using LightningLib.DataEncoders;
using LightningLib.lndrpc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Admin;
using zapread.com.Models.API.Account;
using zapread.com.Models.Database;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public AdminController()
        {
            // Empty constructor
        }

        public AdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        /// <summary>
        /// Controller for listing of vote events
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Votes()
        {
            using (var db = new ZapContext())
            {
                var votes = await db.SpendingEvents.Take(100).ToListAsync();
                return View();
            }
        }

        [AllowAnonymous] // Needed since we are doing this during install
        [HttpPost, Route("Admin/Install/GrantAdmin"), ValidateJsonAntiForgeryToken]
        public async Task<ActionResult> GrantAdmin(string adminKey, string grantUser)
        {
            var isenabled = System.Configuration.ConfigurationManager.AppSettings["EnableInstall"];
            if (!Convert.ToBoolean(isenabled))
            {
                return Json(new { success = false, message = "Install disabled." });
            }

            using (var db = new ZapContext())
            {
                var adminKeySetting = System.Configuration.ConfigurationManager.AppSettings["AdminMasterPassword"];
                if (adminKey != adminKeySetting)
                {
                    return Json(new { success = false, message = "Invalid Key." });
                }

                var u = await db.Users
                    .Where(usr => usr.Name == grantUser)
                    .FirstOrDefaultAsync();

                // Ensure the role exists
                if (!(await RoleManager.RoleExistsAsync("Administrator")))
                {
                    // role does not exist
                    var createResult = await RoleManager.CreateAsync(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                    {
                        Name = "Administrator"
                    });

                    if (!createResult.Succeeded)
                    {
                        return Json(new { success = false, message = "Error Creating Role" });
                    }
                }

                var addResult = await this.UserManager.AddToRoleAsync(u.AppId, "Administrator");

                if (!addResult.Succeeded)
                {
                    return Json(new { success = false, message = "Error Adding to Role" });
                }

                return Json(new { success = true });
            }
        }

        #region Lightning Payments Admin

        /// <summary>
        /// This method checks all of the unpaid invoices posted in the database against the lnd node to see if they have been paid
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyInvoices()
        {
            LndRpcClient lndClient;
            using (var db = new ZapContext())
            {
                var g = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    .AsNoTracking()
                    .FirstOrDefault();

                lndClient = new LndRpcClient(
                host: g.LnMainnetHost,
                macaroonAdmin: g.LnMainnetMacaroonAdmin,
                macaroonRead: g.LnMainnetMacaroonRead,
                macaroonInvoice: g.LnMainnetMacaroonInvoice);
            }

            using (var db = new ZapContext())
            {
                // These are the unpaid invoices
                var unpaidInvoices = db.LightningTransactions
                    .Where(t => t.IsSettled == false)
                    .Where(t => t.IsDeposit == true)
                    .Where(t => t.IsIgnored == false)
                    .Include(t => t.User)
                    .Include(t => t.User.Funds);

                var numinv = unpaidInvoices.Count();

                foreach (var i in unpaidInvoices)
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
                                        var name = user.Name;
                                        user.Funds.Balance += i.Amount;
                                        userBalance = Math.Floor(user.Funds.Balance);
                                        i.IsSettled = true;
                                        i.TimestampSettled = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc) + TimeSpan.FromSeconds(Convert.ToInt64(inv.settle_date));
                                        //db.SaveChangesAsync();
                                    }

                                    // Notify clients the invoice was paid.
                                    await NotificationService.SendPaymentNotification(user.AppId, i.PaymentRequest, userBalance, i.Id).ConfigureAwait(true);
                                }
                                else if (use == TransactionUse.Tip)
                                {
                                    // TODO
                                }
                                else if (use == TransactionUse.VotePost)
                                {
                                    // TODO
                                }
                                else if (use == TransactionUse.VoteComment)
                                {
                                    // TODO
                                }
                            }
                            else
                            {
                                // We can't perform any action on the invoice, but we should mark it as settled.
                                // Unfortunately, we don't know who paid the invoice so we can't credit the funds to any account.
                                // The lost funds should probably go to community pot in that case.
                                var amt = i.Amount;
                                i.IsIgnored = true;
                            }
                        }
                        else if (inv.settled != null && inv.settled == false)
                        {
                            // Still waiting.
                            // nothing to do
                            // check if invoice is expired - ignore if it is.
                            i.IsIgnored = false;
                        }
                        else
                        {
                            // Check if expired
                            var start = Convert.ToInt64(inv.creation_date);
                            var end = start + Convert.ToInt64(inv.expiry);
                        }
                    }
                    else
                    {
                        // Darn, the hashstring wasn't recorded for some reason.  Can't look up the invoice in LND.
                        // Hide this transaction from appearing next time.
                        //i.IsSettled = true;
                        //i.TimestampSettled = DateTime.UtcNow;
                        i.IsIgnored = true;
                    }
                }

                db.SaveChanges();
            }
            return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// Lists the icons used by groups
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Admin/Group/Icons/List")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> SetGroupIcon(DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                return Json(new { success = false });
            }

            using (var db = new ZapContext())
            {
                var iconsQuery = db.Groups
                    .OrderBy(g => g.GroupId)
                    .Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        g.Icon, // Legacy
                        IconId = g.GroupImage == null ? 0 : g.GroupImage.ImageId
                    });

                //.Skip(dataTableParameters.Start).Take(dataTableParameters.Length).ToListAsync().ConfigureAwait(false)

                //db.Icons.OrderByDescending(i => i.Id).Skip(dataTableParameters.Start).Take(dataTableParameters.Length)
                //.ToList();

                //var values = icons.Select(i => new DataItem()
                //{
                //    Icon = i.Icon,
                //    Graphic = i.Icon,
                //    Id = i.Id,
                //}).ToList();

                int numrec = iconsQuery.Count();

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = await iconsQuery
                        .Skip(dataTableParameters.Start)
                        .Take(dataTableParameters.Length)
                        .ToListAsync().ConfigureAwait(false)
                };

                return Json(ret);
            }
        }
        #endregion

        #region Achievements

        public ActionResult Achievements()
        {
            var vm = new AdminAchievementsViewModel();
            return View(vm);
        }

        public ActionResult GetAchievements(DataTableParameters dataTableParameters)
        {
            using (var db = new ZapContext())
            {
                var icons = db.Achievements
                    .Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Description,
                        a.Value,
                        Awarded = a.Awarded.Count(),
                    })
                    .OrderByDescending(i => i.Id)
                    .Skip(dataTableParameters.Start).Take(dataTableParameters.Length);
                    //.ToList();

                var values = icons.Select(i => new 
                {
                    i.Id,
                    i.Name,
                    i.Description,
                    i.Value,
                    i.Awarded,
                }).ToList();

                int numrec = db.Achievements.Count();

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

        [HttpPost, Route("Admin/Achievements/Description/Update")]
        public async Task<JsonResult> AdminUpdateAchievementDescription(int id, string description)
        {
            using (var db = new ZapContext())
            {
                var a = await db.Achievements
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (a == null)
                {
                    return Json(new { success = false, message = "Achievement not found." });
                }

                a.Description = description;
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
        }

        [HttpPost, Route("Admin/Achievements/Name/Update")]
        public async Task<JsonResult> AdminUpdateAchievementName(int id, string name)
        {
            using (var db = new ZapContext())
            {
                var a = await db.Achievements
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (a == null)
                {
                    return Json(new { success = false, message = "Achievement not found." });
                }

                a.Name = name;
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
        }

        [HttpPost, Route("Admin/Achievements/Grant")]
        public async Task<JsonResult> AdminGrantAchievement(int id, string username)
        {
            using (var db = new ZapContext())
            {
                var a = await db.Achievements
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (a == null)
                {
                    return Json(new { success = false, message = "Achievement not found." });
                }

                var user = await db.Users
                    .Include(u => u.Achievements)
                    .FirstOrDefaultAsync(i => i.Name == username);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var ua = new UserAchievement()
                {
                    AchievedBy = user,
                    Achievement = a,
                    DateAchieved = DateTime.UtcNow,
                };

                user.Achievements.Add(ua);

                //a.Name = name;
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
        }

        [HttpPost, Route("Admin/Achievements/Upload")]
        public JsonResult AdminUploadAchievementImage(HttpPostedFileBase file, string id)
        {
            if (file.ContentLength > 0)
            {
                //string _FileName = Path.GetFileName(file.FileName);
                Image img = Image.FromStream(file.InputStream);

                // Images should retain aspect ratio
                double ar = Convert.ToDouble(img.Width) / Convert.ToDouble(img.Height); // Aspect ratio
                int max_wh = 20; // desired max width or height
                int newHeight = img.Height;
                int newWidth = img.Width;
                if (img.Height > img.Width)
                {
                    newHeight = max_wh;
                    newWidth = Convert.ToInt32(Convert.ToDouble(max_wh) * ar);
                }
                else
                {
                    newWidth = max_wh;
                    newHeight = Convert.ToInt32(Convert.ToDouble(max_wh) / ar);
                }

                var bmp = new Bitmap((int)max_wh, (int)max_wh);
                var graph = Graphics.FromImage(bmp);
                var brush = new SolidBrush(Color.Transparent);

                graph.InterpolationMode = InterpolationMode.High;
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.SmoothingMode = SmoothingMode.AntiAlias;
                graph.FillRectangle(brush, new RectangleF(0, 0, max_wh, max_wh));
                graph.DrawImage(img, ((int)max_wh - newWidth) / 2, ((int)max_wh - newHeight) / 2, newWidth, newHeight);
                byte[] data = bmp.ToByteArray(ImageFormat.Png);

                using (var db = new ZapContext())
                {
                    if (Convert.ToInt32(id) == -1)
                    {
                        // new Achievement
                        Achievement a = new Achievement()
                        {
                            Image = data
                        };
                        db.Achievements.Add(a);
                        db.SaveChanges();
                        return Json(new { success = true, result = "success", a.Id });
                    }
                    else
                    {
                        var aid = Convert.ToInt32(id);
                        var a = db.Achievements.FirstOrDefault(ac => ac.Id == aid);
                        if (a == null)
                        {
                            return Json(new { success = false, message = "Id does not exist." });
                        }
                        a.Image = data;
                        db.SaveChanges();
                        return Json(new { success = true, result = "success", a.Id });
                    }
                }
            }
            return Json(new { success=true, result = "success" });
        }

        [HttpPost, Route("Admin/AddAchievement")]
        public async Task<ActionResult> AddAchievement(int id, string name, string description, int value)
        {
            using (var db = new ZapContext())
            {
                var a = await db.Achievements
                    .FirstOrDefaultAsync(ac => ac.Id == id);

                if (a == null)
                {
                    a = new Achievement()
                    {
                        Name = name,
                        Description = description,
                        Value = value
                    };
                    db.Achievements.Add(a);
                }
                else
                {
                    a.Name = name;
                    a.Description = description;
                    a.Value = value;
                }
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
        }

        public async Task<ActionResult> DeleteAchievement(int id)
        {
            using (var db = new ZapContext())
            {
                var a = await db.Achievements
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (a == null)
                {
                    Json(new { success = false });
                }

                db.Achievements.Remove(a);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetPostStats()
        {
            var endDate = DateTime.UtcNow;
            DateTime epochUTC = new DateTime(1970, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);

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

                List<Stat> postStats = GetPostStats(epochUTC, startDate, bin, binnedPostStats);
                List<Stat> commentStats = GetCommentStats(epochUTC, startDate, bin, binnedCommentStats);
                List<Stat> spendingStats = GetSpendingStats(epochUTC, startDate, bin, binnedSpendingStats);

                var maxPosts = postStats.Any() ? postStats.Max(x => x.Count) : 0;
                var maxComments = commentStats.Any() ? commentStats.Max(x => x.Count) : 0;
                var maxPostComments = maxPosts > maxComments ? maxPosts : maxComments;
                var maxSpent = spendingStats.Any() ? spendingStats.Max(x => x.Count) : 0;

                return Json(new { postStats, commentStats, spendingStats, maxPostComments, maxSpent }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<Stat> GetSpendingStats(DateTime epochUTC, DateTime startDate, DateGroupType bin, IQueryable<IGrouping<int?, SpendingEvent>> binnedSpendingStats)
        {
            return binnedSpendingStats.Select(x => new
            {
                x.Key,
                Sum = x.Sum(y => y.Amount)
            }).ToList()
            .Select(x => new Stat
            {
                TimeStampUtc = Convert.ToInt64((GetDate(bin, x.Key.Value, startDate) - epochUTC).TotalMilliseconds),
                Count = Convert.ToInt32(x.Sum)
            })
            .OrderBy(x => x.TimeStampUtc)
            .ToList();
        }

        private List<Stat> GetCommentStats(DateTime epochUTC, DateTime startDate, DateGroupType bin, IQueryable<IGrouping<int?, Comment>> binnedCommentStats)
        {
            return binnedCommentStats.Select(x => new
            {
                x.Key,
                Count = x.Count()
            }).ToList()
            .Select(x => new Stat
            {
                TimeStampUtc = Convert.ToInt64((GetDate(bin, x.Key.Value, startDate) - epochUTC).TotalMilliseconds),
                Count = x.Count
            })
            .OrderBy(x => x.TimeStampUtc)
            .ToList();
        }

        private List<Stat> GetPostStats(DateTime epochUTC, DateTime startDate, DateGroupType bin, IQueryable<IGrouping<int?, Post>> binnedPostStats)
        {
            return binnedPostStats.Select(x => new
            {
                x.Key,
                Count = x.Count()
            }).ToList()
            .Select(x => new Stat
            {
                TimeStampUtc = Convert.ToInt64((GetDate(bin, x.Key.Value, startDate) - epochUTC).TotalMilliseconds),
                Count = x.Count
            })
            .OrderBy(x => x.TimeStampUtc)
            .ToList();
        }

        // GET Admin/UserBalance
        [Route("Admin/UserBalance/{username}")]
        public async Task<JsonResult> UserBalance(string username)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { value = 0 }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new ZapContext())
            {

                var user = await db.Users.Where(u => u.Name.Trim() == username.Trim())
                    .Include(usr => usr.Funds)
                    .AsNoTracking().SingleOrDefaultAsync();

                if (user == null)
                {
                    // User doesn't exist.
                    return Json(new { value = 0 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { value = Math.Floor(user.Funds.Balance) }, JsonRequestBehavior.AllowGet);
            }
        }

        [Route("Admin/UserLimboBalance/{username}")]
        public async Task<JsonResult> UserLimboBalance(string username)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { value = 0 }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new ZapContext())
            {

                var user = await db.Users.Where(u => u.Name.Trim() == username.Trim())
                    .Include(usr => usr.Funds)
                    .AsNoTracking().SingleOrDefaultAsync();

                if (user == null)
                {
                    // User doesn't exist.
                    return Json(new { value = 0 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { value = Math.Floor(user.Funds.LimboBalance) }, JsonRequestBehavior.AllowGet);
            }
        }

        #region audit

        [Route("Admin/Audit/Transaction/{id}")]
        public ActionResult AuditTransaction(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/" });
            }

            using (var db = new ZapContext())
            {
                var website = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    .AsNoTracking()
                    .FirstOrDefault();

                if (website == null)
                {
                    throw new Exception("Unable to load website settings.");
                }

                LndRpcClient lndClient = new LndRpcClient(
                    host: website.LnMainnetHost,
                    macaroonAdmin: website.LnMainnetMacaroonAdmin,
                    macaroonRead: website.LnMainnetMacaroonRead,
                    macaroonInvoice: website.LnMainnetMacaroonInvoice);

                var payments = lndClient.GetPayments(include_incomplete: true);

                var t = db.LightningTransactions
                    .Include(tr => tr.User)
                    .Include(tr => tr.User.Funds)
                    .FirstOrDefault(tr => tr.Id == id);

                var decoded = lndClient.DecodePayment(t.PaymentRequest);

                var pmt = payments.payments.Where(p => p.payment_hash == t.HashStr).FirstOrDefault();

                return Json(new { });
            }
        }

        // GET: Admin/Audit/{username}
        [Route("Admin/Audit/{username}")]
        public ActionResult Audit(string username)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Admin/Audit/" + username });
            }

            if (username == null)
            {
                return RedirectToAction(actionName: "Index", controllerName: "Admin");
            }

            using (var db = new ZapContext())
            {
                var user = db.Users.Where(u => u.Name.Trim() == username.Trim())
                    .Include(usr => usr.Funds)
                    .AsNoTracking().FirstOrDefault();

                if (user == null)
                {
                    // User doesn't exist.
                    // TODO: send to user not found error page
                    return RedirectToAction("Index", "Admin");
                }

                var vm = new AuditUserViewModel()
                {
                    Username = username,
                };

                return View(vm);
            }
        }

        public class AuditDataItem
        {
            public string Created { get; set; }
            public string Time { get; set; }
            public string Type { get; set; }
            public string Amount { get; set; }
            public string URL { get; set; }
            public string Memo { get; set; }
            public bool Settled { get; set; }
            public string PaymentHash { get; set; }
            public int id { get; set; }
        }

        [HttpPost, Route("Admin/GetLNTransactions/{username}")]
        public ActionResult GetLNTransactions(string username, [System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            using (var db = new ZapContext())
            {
                User user;
                user = db.Users
                        .Include(usr => usr.LNTransactions)
                        .Where(u => u.Name.Trim() == username.Trim())
                        .SingleOrDefault();

                var pageTxns = user.LNTransactions
                    //.Where(tx => tx.TimestampSettled != null)
                    .OrderByDescending(tx => tx.TimestampCreated)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToList();

                var values = pageTxns.Select(t => new AuditDataItem()
                {
                    Created = t.TimestampCreated == null ? "" : t.TimestampCreated.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Time = t.TimestampSettled == null ? "" : t.TimestampSettled.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Type = t.IsDeposit ? "Deposit" : "Withdrawal",
                    Amount = Convert.ToString(t.Amount),
                    Memo = t.Memo,
                    Settled = t.IsSettled,
                    PaymentHash = t.HashStr,
                    id = t.Id,
                }).ToList();

                int numrec = user.LNTransactions.Count();

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

        [HttpPost, Route("Admin/GetEarningEvents/{username}")]
        public ActionResult GetEarningEvents(string username, [System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            using (var db = new ZapContext())
            {
                User u;
                u = db.Users
                        .Include(usr => usr.LNTransactions)
                        .Where(usr => usr.Name.Trim() == username.Trim())
                        .SingleOrDefault();

                var pageEarnings = u.EarningEvents
                    .OrderByDescending(e => e.TimeStamp)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToList();

                var values = pageEarnings.Select(t => new AuditDataItem()
                {
                    Time = t.TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Amount = t.Amount.ToString("0.##"),
                    Type = t.Type == 0 ? (t.OriginType == 0 ? "Post" : t.OriginType == 1 ? "Comment" : t.OriginType == 2 ? "Tip" : "Unknown") : t.Type == 1 ? "Group" : t.Type == 2 ? "Community" : "Unknown",
                }).ToList();

                int numrec = u.EarningEvents.Count();

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

        [HttpPost, Route("Admin/GetSpendingEvents/{username}")]
        public ActionResult GetSpendingEvents(string username, [System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            using (var db = new ZapContext())
            {
                User u;
                u = db.Users
                        .Include(usr => usr.LNTransactions)
                        .Include(usr => usr.SpendingEvents)
                        .Include(usr => usr.SpendingEvents.Select(s => s.Post))
                        .Include(usr => usr.SpendingEvents.Select(s => s.Group))
                        .Include(usr => usr.SpendingEvents.Select(s => s.Comment))
                        .Include(usr => usr.SpendingEvents.Select(s => s.Comment).Select(c => c.Post))
                        //.Where(usr => usr.Name == "renepickhardt").First(); //Debug issue observed by this user
                        .Where(usr => usr.Name.Trim() == username.Trim())
                        .SingleOrDefault();

                var pageSpendings = u.SpendingEvents
                    .OrderByDescending(e => e.TimeStamp)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToList();

                var values = pageSpendings.Select(t => new AuditDataItem()
                {
                    Time = t.TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Type = t.Post != null ? "Post " + t.Post.PostId.ToString() : (
                           t.Comment != null ? "Comment " + t.Comment.CommentId.ToString() : (
                           t.Group != null ? "Group " + t.Group.GroupId.ToString() :
                           "Other")),
                    Amount = Convert.ToString(t.Amount),
                    URL = t.Post != null ? Url.Action("Detail", "Post") + "/" + Convert.ToString(t.Post.PostId) : (
                            t.Comment != null ? Url.Action("Detail", "Post") + "/" + t.Comment.Post.PostId.ToString() : (
                            t.Group != null ? Url.Action("Index", "Group") :
                            "")),
                }).ToList();

                int numrec = u.SpendingEvents.Count();

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

        [HttpGet, Route("Admin/GetLNFlow/{username}/{days?}")]
        public ActionResult GetLNFlow(string username, string days)
        {
            double amount = 0.0;
            int numDays = Convert.ToInt32(days);
            try
            {
                using (var db = new ZapContext())
                {
                    // Get the logged in user ID
                    var uid = User.Identity.GetUserId();
                    var userTxns = db.Users
                            .Include(i => i.LNTransactions)
                            .Where(u => u.Name.Trim() == username.Trim())
                            .SelectMany(u => u.LNTransactions);

                    var sum = userTxns
                        .Where(tx => DbFunctions.DiffDays(tx.TimestampSettled, DateTime.Now) <= numDays)   // Filter for time
                        .Select(tx => new { amt = tx.IsDeposit ? tx.Amount : -1.0 * tx.Amount })
                        .Sum(tx => tx.amt);

                    amount = sum;
                }
            }
            catch (Exception)
            {
                // todo: add some error logging

                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                amount = 0.0;
            }
            string value = amount.ToString("0.##");
            return Json(new { value }, JsonRequestBehavior.AllowGet);
        }

        // Function to check if LN invoice was settled
        [HttpGet, Route("Admin/CheckLNInvoice/{id}")]
        public ActionResult CheckLNInvoice(int id)
        {
            using (var db = new ZapContext())
            {
                return Json(new { });
            }
        }

        #endregion

        #region Lightning

        [HttpGet]
        [Route("Admin/Lightning/Payments")]
        public ActionResult LightningPayments()
        {
            return View();
        }

        [Route("Admin/Lightning")]
        public async Task<ActionResult> Lightning()
        {
            // Re-register Periodic hangfire monitor

            RecurringJob.AddOrUpdate<LNTransactionMonitor>(
                x => x.CheckLNTransactions(),
                Cron.MinuteInterval(5));

            using (var db = new ZapContext())
            {
                var g = await db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                var vm = new AdminLightningViewModel()
                {
                    LnMainnetHost = g.LnMainnetHost,
                    LnPubkey = g.LnPubkey,
                    LnMainnetMacaroonAdmin = g.LnMainnetMacaroonAdmin,
                    LnMainnetMacaroonInvoice = g.LnMainnetMacaroonInvoice,
                    LnMainnetMacaroonRead = g.LnMainnetMacaroonRead,
                };

                return View(vm);
            }
        }

        [HttpPost, Route("Admin/Lightning/Update")]
        public async Task<ActionResult> LightningUpdate(string LnMainnetHost, string LnPubkey, string LnMainnetMacaroonAdmin, string LnMainnetMacaroonInvoice, string LnMainnetMacaroonRead)
        {
            using (var db = new ZapContext())
            {
                var g = await db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    .FirstOrDefaultAsync();

                g.LnMainnetHost = LnMainnetHost;
                g.LnPubkey = LnPubkey;
                g.LnMainnetMacaroonAdmin = LnMainnetMacaroonAdmin;
                g.LnMainnetMacaroonInvoice = LnMainnetMacaroonInvoice;
                g.LnMainnetMacaroonRead = LnMainnetMacaroonRead;

                await db.SaveChangesAsync();

                return Json(new { result = "success" });
            }
        }

        [HttpPost, Route("Admin/Lightning/Macaroon/Upload")]
        public JsonResult LightningUploadInvoice(HttpPostedFileBase file, string macaroonType)
        {
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }
            HexEncoder h = new HexEncoder();
            var macaroon = h.EncodeData(fileData);

            return Json(new { result = "success", macaroon });
        }

        #endregion

        #region Users

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [Route("Admin/Users")]
        public ActionResult Users()
        {
            // Redirect to login screen if not authenticated.
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Admin/Users/" });
            }

            return View();
        }

        /// <summary>
        /// Queries the users for a paging table.
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost, Route("Admin/UsersTable")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> UsersTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            using (var db = new ZapContext())
            {
                var sorts = dataTableParameters.Order;

                // Build our query
                var pageUsersQS = db.Users.AsNoTracking();
                IOrderedQueryable<User> pageUsersQ = null;

                foreach (var s in sorts)
                {
                    if (s.Dir == "asc")
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "DateJoined")
                            pageUsersQ = pageUsersQS.OrderBy(q => q.DateJoined);
                        else if (dataTableParameters.Columns[s.Column].Name == "IsOnline")
                            pageUsersQ = pageUsersQS.OrderBy(q => q.IsOnline);
                        else if (dataTableParameters.Columns[s.Column].Name == "LastSeen")
                            pageUsersQ = pageUsersQS.OrderBy(q => q.DateLastActivity);
                        else if (dataTableParameters.Columns[s.Column].Name == "NumPosts")
                            pageUsersQ = pageUsersQS.OrderBy(q => q.Posts.Count);
                        else if (dataTableParameters.Columns[s.Column].Name == "NumComments")
                            pageUsersQ = pageUsersQS.OrderBy(q => q.Comments.Count);
                        else if (dataTableParameters.Columns[s.Column].Name == "Balance")
                            pageUsersQ = pageUsersQS.OrderBy(q => q.Funds == null ? 0 : q.Funds.Balance);
                    }
                    else
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "DateJoined")
                            pageUsersQ = pageUsersQS.OrderByDescending(q => q.DateJoined);
                        if (dataTableParameters.Columns[s.Column].Name == "IsOnline")
                            pageUsersQ = pageUsersQS.OrderByDescending(q => q.IsOnline);
                        else if (dataTableParameters.Columns[s.Column].Name == "LastSeen")
                            pageUsersQ = pageUsersQS.OrderByDescending(q => q.DateLastActivity);
                        else if (dataTableParameters.Columns[s.Column].Name == "NumPosts")
                            pageUsersQ = pageUsersQS.OrderByDescending(q => q.Posts.Count);
                        else if (dataTableParameters.Columns[s.Column].Name == "NumComments")
                            pageUsersQ = pageUsersQS.OrderByDescending(q => q.Comments.Count);
                        else if (dataTableParameters.Columns[s.Column].Name == "Balance")
                            pageUsersQ = pageUsersQS.OrderByDescending(q => q.Funds == null ? 0 : q.Funds.Balance);
                    }
                }

                if (pageUsersQ == null)
                {
                    pageUsersQ = pageUsersQS.OrderByDescending(q => q.Id);
                }

                var pageUsers = await pageUsersQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .Select(u => new
                    {
                        UserName = u.Name,
                        u.DateJoined,
                        u.DateLastActivity,
                        NumPosts = u.Posts.Count,
                        NumComments = u.Comments.Count,
                        Balance = u.Funds != null ? u.Funds.Balance : 0,
                        u.AppId,
                        u.Id,
                        u.IsOnline,
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);

                // Tidy up formatting
                var values = pageUsers
                    .Select(u => new
                    {
                        u.UserName,
                        DateJoined = u.DateJoined != null ? u.DateJoined.Value.ToString("o", CultureInfo.InvariantCulture) : "?",
                        LastSeen = u.DateLastActivity != null ? u.DateLastActivity.Value.ToString("o", CultureInfo.InvariantCulture) : "?",
                        u.NumPosts,
                        u.NumComments,
                        Balance = (u.Balance / 100000000.0).ToString("F8", CultureInfo.InvariantCulture),
                        u.AppId,
                        u.Id,
                        u.IsOnline,
                    }).ToList();

                int numrec = db.Users.Count();

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

        #region CRON

        [HttpPost, Route("Admin/Jobs/Run")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult RunJob(string jobid)
        {
            if (jobid == null)
            {
                return Json(new { success = false });
            }

            if (jobid == "CheckLNTransactions")
            {
                RecurringJob.Trigger("LNTransactionMonitor.CheckLNTransactions");
                return Json(new { success = true });
            }

            if (jobid == "CommunityPayout")
            {
                RecurringJob.Trigger("PayoutsService.CommunityPayout");
                return Json(new { success = true });
            }

            if (jobid == "GroupsPayout")
            {
                RecurringJob.Trigger("PayoutsService.GroupsPayout");
                return Json(new { success = true });
            }

            if (jobid == "CheckAchievements")
            {
                RecurringJob.Trigger("AchievementsService.CheckAchievements");
                return Json(new { success = true });
            }

            if (jobid == "LNNodeMonitor.UpdateHourly")
            {
                RecurringJob.Trigger("LNNodeMonitor.UpdateHourly");
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost, Route("Admin/Jobs/Install")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult InstallJob(string jobid)
        {
            if (jobid == null)
            {
                return Json(new { success = false });
            }

            if (jobid == "CheckLNTransactions")
            {
                RecurringJob.AddOrUpdate<LNTransactionMonitor>(
                    x => x.CheckLNTransactions(),
                    Cron.MinuteInterval(5));
                return Json(new { success = true });
            }

            if (jobid == "CommunityPayout")
            {
                RecurringJob.AddOrUpdate<PayoutsService>(
                    x => x.CommunityPayout(),
                    Cron.Daily(0, 0));
                return Json(new { success = true });
            }

            if (jobid == "GroupsPayout")
            {
                RecurringJob.AddOrUpdate<PayoutsService>(
                    x => x.GroupsPayout(),
                    Cron.Daily(0, 0));
                return Json(new { success = true });
            }

            if (jobid == "CheckAchievements")
            {
                RecurringJob.AddOrUpdate<AchievementsService>(
                    x => x.CheckAchievements(),
                    Cron.Hourly(0));
                return Json(new { success = true });
            }

            if (jobid == "LNNodeMonitor.UpdateHourly")
            {
                RecurringJob.AddOrUpdate<LNNodeMonitor>(
                    x => x.UpdateHourly(),
                    Cron.Hourly(0));
                return Json(new { success = true });
            }
            

            return Json(new { success = false });
        }

        [HttpPost, Route("Admin/Jobs/Remove")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult RemoveJob(string jobid)
        {
            if (jobid == null)
            {
                return Json(new { success = false });
            }

            if (jobid == "CheckLNTransactions")
            {
                RecurringJob.RemoveIfExists("LNTransactionMonitor.CheckLNTransactions");
                return Json(new { success = true });
            }

            if (jobid == "CommunityPayout")
            {
                RecurringJob.RemoveIfExists("PayoutsService.CommunityPayout");
                return Json(new { success = true });
            }

            if (jobid == "GroupsPayout")
            {
                RecurringJob.RemoveIfExists("PayoutsService.GroupsPayout");
                return Json(new { success = true });
            }

            if (jobid == "CheckAchievements")
            {
                RecurringJob.RemoveIfExists("AchievementsService.CheckAchievements");
                return Json(new { success = true });
            }

            if (jobid == "LNNodeMonitor.UpdateHourly")
            {
                RecurringJob.RemoveIfExists("LNNodeMonitor.UpdateHourly");
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        public ActionResult Jobs()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Admin/Jobs/" });
            }

            var vm = new AdminJobsViewModel();

            //RecurringJob.
            var manager = new RecurringJobManager();

            var jobs = JobStorage.Current.GetMonitoringApi();

            var recurringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();

            vm.RecurringJobs = recurringJobs;

            return View(vm);
        }

        #endregion

        #region Admin Panel

        // GET: Admin
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Admin" });
            }

            AdminViewModel vm;

            using (var db = new ZapContext())
            {
                var globals = db.ZapreadGlobals.Where(g => g.Id == 1)
                    .AsNoTracking()
                    .FirstOrDefault();

                double gd = 0;
                double LNdep = 0;
                double LNwth = 0;
                double LNfee = 0;
                if (db.Groups.Count() > 1)
                {
                    gd = db.Groups.Sum(g => g.TotalEarnedToDistribute);
                }
                if (db.LightningTransactions.Any())
                {
                    LNdep = Convert.ToDouble(db.LightningTransactions.Where(t => t.IsSettled && t.IsDeposit).Sum(t => t.Amount)) / 100000000.0;
                    var settledWithdraws = db.LightningTransactions.Where(t => t.IsSettled && !t.IsDeposit).ToList();
                    var sumSettledWithdraws = settledWithdraws.Sum(t => t.Amount);

                    LNwth = Convert.ToDouble(sumSettledWithdraws) / 100000000.0;
                    LNfee = Convert.ToDouble(db.LightningTransactions.Where(t => t.IsSettled).Sum(t => t.FeePaid_Satoshi ?? 0)) / 100000000.0;
                }

                // Calculate post and comment stats.
                var startDate = DateTime.Now.AddDays(-1 * 31);
                var allPosts = db
                    .Posts
                    .Where(x => x.TimeStamp > startDate);

                DateGroupType group = DateGroupType.Day;

                var groupedStats = GroupPostsByDate(allPosts, group, startDate);

                DateTime epochUTC = new DateTime(1970, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);

                //Node Statistics
                long localActive = 0;
                long remoteActive = 0;
                long channelBalance = 0;
                if (db.LNNodes.Any())
                {
                    localActive = db.LNNodes.First().Channels
                        .Where(c => c.IsOnline)
                        .Sum(c => c.ChannelHistory.Last().LocalBalance_MilliSatoshi);

                    remoteActive = db.LNNodes.First().Channels
                        .Where(c => c.IsOnline)
                        .Sum(c => c.ChannelHistory.Last().RemoteBalance_MilliSatoshi);

                    channelBalance = db.LNNodes.First().Channels
                        .Where(c => c.IsOnline)
                        .Sum(c => c.Capacity_MilliSatoshi);
                }

                vm = new AdminViewModel()
                {
                    Globals = globals,
                    PendingGroupToDistribute = gd,
                    LNTotalDeposited = LNdep,
                    LNTotalWithdrawn = LNwth,
                    LNFeesPaid = LNfee,
                    LNCapacity = channelBalance / 100000000.0,
                    LNLocalBalance = localActive / 100000000.0,
                    LNRemoteBalance = remoteActive / 100000000.0,
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

        #endregion

        #region Admin Bar

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
                vm.NumPosts = Convert.ToInt32(u.Posts.Where(p => !p.IsDeleted && !p.IsDraft).Count());

                var appUser = UserManager.FindById(u.AppId);
                vm.Email = appUser.Email;

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
            var vm = new AddUserToGroupRoleViewModel
            {
                GroupName = groupName
            };
            return PartialView("_PartialAddUserToGroupRoleForm", vm);
        }

        // Query the DB for all users starting with the prefix
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
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
        [ValidateJsonAntiForgeryToken]
        public JsonResult UpdateUserGroupRoles(string group, string user, bool isAdmin, bool isMod, bool isMember)
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
                if (isAdmin)
                {
                    g.Administrators.Add(u);
                    u.GroupAdministration.Add(g);
                }
                else
                {
                    if (g.Administrators.Contains(u))
                    {
                        if (g.Administrators.Count() == 1)
                        {
                            // do not remove last admin
                            return Json(new { success = true, message = "Unable to remove last administrator from group" });
                        }
                        {
                            g.Administrators.Remove(u);
                            u.GroupAdministration.Remove(g);
                        }
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

        #endregion

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