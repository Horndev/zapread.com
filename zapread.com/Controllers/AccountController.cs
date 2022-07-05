using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Database.Financial;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// This controller handles user account aspects related to identity
    /// </summary>
    [Authorize]
    [RoutePrefix("Account")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        /// <summary>
        /// 
        /// </summary>
        public AccountController()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userManager"></param>
        /// <param name = "signInManager"></param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userManager"></param>
        /// <param name = "signInManager"></param>
        /// <param name = "roleManager"></param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            this.RoleManager = roleManager;
            this.SignInManager = signInManager;
            this.UserManager = userManager;
        }

        private async Task EnsureUserExists(string userAppId, ZapContext db, string refcode = null)
        {
            if (userAppId != null)
            {
                string refUserAppId = null;
                if (refcode != null)
                {
                    refUserAppId = await db.Users.Where(u => u.ReferralCode == refcode).Select(u => u.AppId).FirstOrDefaultAsync();
                }

                if (!db.Users.Where(u => u.AppId == userAppId).Any())
                {
                    // no user entry
                    User u = new User()
                    {AboutMe = "Nothing to tell.", AppId = userAppId, Name = UserManager.FindByIdAsync(userAppId).Result.UserName, ProfileImage = new UserImage(), ThumbImage = new UserImage(), Funds = new UserFunds(), Settings = new UserSettings(), DateJoined = DateTime.UtcNow, ReferralCode = CryptoService.GetNewRefCode(), // This is the code this user can hand out
 ReferralInfo = refUserAppId != null ? new Referral()
                    {ReferredByAppId = refUserAppId, TimeStamp = DateTime.UtcNow, } : null, };
                    db.Users.Add(u);
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }
                else
                {
                    var user = await db.Users.Include(u => u.ReferralInfo).FirstOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(true);
                    if (user.ReferralCode == null)
                    {
                        user.ReferralCode = CryptoService.GetNewRefCode();
                    }

                    if (refUserAppId != null && user.ReferralInfo == null)
                    {
                        user.ReferralInfo = new Referral()
                        {ReferredByAppId = refUserAppId, TimeStamp = DateTime.UtcNow, };
                    }

                    if (user.Settings == null)
                    {
                        user.Settings = new UserSettings()
                        {ColorTheme = "light", NotifyOnPrivateMessage = true, NotifyOnMentioned = true, NotifyOnNewPostSubscribedGroup = true, NotifyOnNewPostSubscribedUser = true, NotifyOnOwnCommentReplied = true, NotifyOnOwnPostCommented = true, NotifyOnReceivedTip = true, };
                    }

                    if (user.Languages == null)
                    {
                        user.Languages = "en";
                    }

                    if (user.LNTransactions == null)
                    {
                        user.LNTransactions = new List<LNTransaction>();
                    }

                    await db.SaveChangesAsync().ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// GET: /Account/Balance
        /// Returns the currently logged in user balance
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Balance()
        {
            XFrameOptionsDeny();
            double userBalance = 0.0;
            double userSpendOnlyBalance = 0.0;
            if (Request.IsAuthenticated)
            {
                var userAppId = User.Identity.GetUserId();
                var balanceInfo = await GetUserBalance(userAppId).ConfigureAwait(true);
                userBalance = Math.Floor(balanceInfo.Balance);
                userSpendOnlyBalance = Math.Floor(balanceInfo.SpendOnlyBalance);
                string balance = userBalance.ToString("0.##", CultureInfo.InvariantCulture);
                string spendOnlyBalance = userSpendOnlyBalance.ToString("0.##", CultureInfo.InvariantCulture);
                return Json(new
                {
                balance, spendOnlyBalance, balanceInfo.QuickVoteOn, QuickVoteAmount = balanceInfo.QuickVoteAmount > 0 ? balanceInfo.QuickVoteAmount : 1
                }

                , JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
            balance = "0", spendOnlyBalance = "0"
            }

            , JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Redirects to new Balance endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetBalance()
        {
            XFrameOptionsDeny();
            return RedirectToActionPermanent("Balance");
        }

        private async Task<UserBalanceInfo> GetUserBalance(string userAppId)
        {
            UserBalanceInfo balance;
            if (string.IsNullOrEmpty(userAppId))
            {
                return new UserBalanceInfo()
                {Balance = 0.0, SpendOnlyBalance = 0.0, QuickVoteAmount = 10, QuickVoteOn = false, };
            }

            try
            {
                using (var db = new ZapContext())
                {
                    var userBalance = await db.Users.Where(u => u.AppId == userAppId && u.Funds != null).AsNoTracking().Select(u => new UserBalanceInfo()
                    {Balance = u.Funds.Balance, SpendOnlyBalance = u.Funds.SpendOnlyBalance, QuickVoteAmount = u.Funds.QuickVoteAmount, QuickVoteOn = u.Funds.QuickVoteOn}).FirstOrDefaultAsync().ConfigureAwait(false);
                    if (userBalance == null)
                    {
                        // User not found in database, or not logged in
                        var userExists = await db.Users.Where(u => u.AppId == userAppId).AnyAsync().ConfigureAwait(false);
                        if (userExists)
                        {
                            // user exists and funds is null
                            var user_modified = await db.Users.Where(u => u.AppId == userAppId).Include(i => i.Funds).FirstOrDefaultAsync().ConfigureAwait(false);
                            user_modified.Funds = new UserFunds()
                            {Balance = 0.0, SpendOnlyBalance = 0.0, Id = user_modified.Id, TotalEarned = 0.0, QuickVoteOn = false, QuickVoteAmount = 10};
                            await db.SaveChangesAsync().ConfigureAwait(false);
                        }

                        return new UserBalanceInfo()
                        {Balance = 0.0, SpendOnlyBalance = 0.0, QuickVoteAmount = 10, QuickVoteOn = false, };
                    }

                    balance = userBalance;
                }
            }
            catch (Exception e)
            {
                BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                {Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"], Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n method: UserBalance" + "\r\n user: " + userAppId, Email = "", Name = "zapread.com Exception", Subject = "Account Controller error", }, "Accounts", true));
                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                balance = new UserBalanceInfo()
                {Balance = 0.0, SpendOnlyBalance = 0.0, QuickVoteAmount = 10, QuickVoteOn = false, };
            }

            return balance;
        }

        /// <summary>
        /// GET: /Account/GetLimboBalance
        /// Returns the currently logged in user balance
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetLimboBalance()
        {
            XFrameOptionsDeny();
            double userBalance = 0.0;
            if (Request.IsAuthenticated)
            {
                userBalance = await GetUserLimboBalance().ConfigureAwait(true);
            }

            string balance = userBalance.ToString("0.##", CultureInfo.InvariantCulture);
            return Json(new
            {
            balance
            }

            , JsonRequestBehavior.AllowGet);
        }

        private async Task<double> GetUserLimboBalance()
        {
            double balance;
            try
            {
                using (var db = new ZapContext())
                {
                    // Get the logged in user ID
                    var uid = User.Identity.GetUserId();
                    var user = await db.Users.Include(i => i.Funds).AsNoTracking().FirstOrDefaultAsync(u => u.AppId == uid).ConfigureAwait(true);
                    if (user == null)
                    {
                        // User not found in database, or not logged in
                        balance = 0.0;
                    }
                    else
                    {
                        if (user.Funds == null)
                        {
                            // Neets to be initialized
                            var user_modified = await db.Users.Include(i => i.Funds).FirstOrDefaultAsync(u => u.AppId == uid).ConfigureAwait(true);
                            user_modified.Funds = new UserFunds()
                            {Balance = 0.0, Id = user_modified.Id, TotalEarned = 0.0};
                            await db.SaveChangesAsync().ConfigureAwait(true);
                            user = user_modified;
                        }

                        balance = user.Funds.LimboBalance;
                    }
                }
            }
            catch (Exception)
            {
                // todo: add some error logging
                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                balance = 0.0;
            }

            return Math.Floor(balance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "days"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSpendingSum/{days?}")]
        public async Task<ActionResult> GetSpendingSum(string days)
        {
            XFrameOptionsDeny();
            double amount = 0.0;
            int numDays = Convert.ToInt32(days, CultureInfo.InvariantCulture);
            double totalAmount = 0.0;
            string userId = "?";
            try
            {
                // Get the logged in user ID
                userId = User.Identity.GetUserId();
                using (var db = new ZapContext())
                {
                    var sum = await db.Users.Include(i => i.SpendingEvents).Where(u => u.AppId == userId).SelectMany(u => u.SpendingEvents).Where(tx => DbFunctions.DiffDays(tx.TimeStamp, DateTime.Now) <= numDays).SumAsync(tx => (double? )tx.Amount).ConfigureAwait(true) ?? 0;
                    totalAmount = await db.Users.Include(i => i.SpendingEvents).Where(u => u.AppId == userId).SelectMany(u => u.SpendingEvents).SumAsync(tx => (double? )tx.Amount).ConfigureAwait(true) ?? 0;
                    amount = sum;
                }
            }
            catch (Exception e)
            {
                BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                {Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"], Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n method: GetSpendingSum" + "\r\n user: " + userId, Email = "", Name = "zapread.com Exception", Subject = "Account Controller error", }, "Accounts", true));
                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                amount = 0.0;
            }

            string value = amount.ToString("0.##");
            string total = totalAmount.ToString("0.##");
            return Json(new
            {
            value, total
            }

            , JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "days"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEarningsSum/{days?}")]
        public async Task<ActionResult> GetEarningsSum(string days)
        {
            XFrameOptionsDeny();
            double amount = 0.0;
            int numDays = Convert.ToInt32(days, CultureInfo.InvariantCulture);
            double totalAmount = 0.0;
            try
            {
                using (var db = new ZapContext())
                {
                    // Get the logged in user ID
                    var uid = User.Identity.GetUserId();
                    var sum = await db.Users.Include(i => i.EarningEvents).Where(u => u.AppId == uid).SelectMany(u => u.EarningEvents).Where(tx => DbFunctions.DiffDays(tx.TimeStamp, DateTime.Now) <= numDays) // Filter for time
                    .SumAsync(tx => tx.Amount).ConfigureAwait(true);
                    totalAmount = await db.Users.Include(i => i.EarningEvents).Where(u => u.AppId == uid).SelectMany(u => u.EarningEvents).SumAsync(tx => tx.Amount).ConfigureAwait(true);
                    amount = sum;
                }
            }
            catch (Exception)
            {
                // todo: add some error logging
                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                amount = 0.0;
            }

            string value = amount.ToString("0.##", CultureInfo.InvariantCulture);
            string total = totalAmount.ToString("0.##", CultureInfo.InvariantCulture);
            return Json(new
            {
            value, total
            }

            , JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "days"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetLNFlow/{days?}")]
        public async Task<ActionResult> GetLNFlow(string days)
        {
            XFrameOptionsDeny();
            var userAppId = User.Identity.GetUserId();
            double amount = 0.0;
            var balanceInfo = await GetUserBalance(userAppId).ConfigureAwait(true);
            double balance = Math.Floor(balanceInfo.Balance);
            double limboBalance = await GetUserLimboBalance().ConfigureAwait(true);
            int numDays = Convert.ToInt32(days, CultureInfo.InvariantCulture);
            try
            {
                using (var db = new ZapContext())
                {
                    var sum = await db.Users.Include(i => i.LNTransactions).Where(u => u.AppId == userAppId).SelectMany(u => u.LNTransactions).Where(tx => DbFunctions.DiffDays(tx.TimestampSettled, DateTime.Now) <= numDays) // Filter for time
                    .Select(tx => new
                    {
                    amt = tx.IsDeposit ? tx.Amount : -1.0 * tx.Amount
                    }).SumAsync(tx => tx.amt).ConfigureAwait(true);
                    amount = sum;
                }
            }
            catch (Exception)
            {
                // todo: add some error logging
                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                amount = 0.0;
            }

            string value = amount.ToString("0.##", CultureInfo.InvariantCulture);
            return Json(new
            {
            value, total = balance.ToString("0.##", CultureInfo.InvariantCulture), limbo = limboBalance.ToString("0.##", CultureInfo.InvariantCulture), }

            , JsonRequestBehavior.AllowGet);
        }

        /* Identity aspects*/
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// Login screen
        /// </summary>
        /// <param name = "returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            XFrameOptionsDeny();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name = "model"></param>
        /// <param name = "returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (model == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SignInStatus result;
            // Add administrator user impersonation code here (for debug)
            string userNameToImpersonate = null; //null; //"USERTOIMPERSONATE";
            if (userNameToImpersonate != null)
            {
                var userToImpersonate = await UserManager.FindByNameAsync(userNameToImpersonate).ConfigureAwait(true);
                var identityToImpersonate = await UserManager.CreateIdentityAsync(userToImpersonate, DefaultAuthenticationTypes.ApplicationCookie).ConfigureAwait(true);
                var authenticationManager = HttpContext.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                authenticationManager.SignIn(new AuthenticationProperties()
                {IsPersistent = false}, identityToImpersonate);
                result = SignInStatus.Success;
                model.UserName = userNameToImpersonate;
            }
            else
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                result = await SignInManager.PasswordSignInAsync(userName: model.UserName, password: model.Password, isPersistent: model.RememberMe, shouldLockout: false).ConfigureAwait(true);
            }

            switch (result)
            {
                case SignInStatus.Success:
                {
                    var userId = await UserManager.FindByNameAsync(model.UserName).ConfigureAwait(true); //User.Identity.GetUserId();
                    using (var db = new ZapContext())
                    {
                        await EnsureUserExists(userId.Id, db).ConfigureAwait(true);
                        // Apply claims
                        var u = db.Users.Where(us => us.AppId == userId.Id).First();
                        //await UserManager.AddClaimAsync(userId.Id, new Claim("ColorTheme", u.Settings.ColorTheme));
                        var identity = await UserManager.CreateIdentityAsync(userId, DefaultAuthenticationTypes.ApplicationCookie).ConfigureAwait(true);
                        identity.AddClaim(new Claim("ColorTheme", u.Settings.ColorTheme ?? "light"));
                        try
                        {
                            var authenticationManager = HttpContext.GetOwinContext().Authentication;
                            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            authenticationManager.SignIn(new AuthenticationProperties()
                            {IsPersistent = model.RememberMe}, identity);
                        }
                        catch (Exception)
                        {
                        // Need to better handle this
                        }
                    }

                    return RedirectToLocal(returnUrl);
                }

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new
                    {
                    ReturnUrl = returnUrl, RememberMe = model.RememberMe
                    }

                    );
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "provider"></param>
        /// <param name = "returnUrl"></param>
        /// <param name = "rememberMe"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            XFrameOptionsDeny();
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }

            return View(new VerifyCodeViewModel{Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe});
        }

        //
        // POST: /Account/VerifyCode
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "refcode">Referral code</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register(string refcode)
        {
            var captchaSrcB64 = CaptchaService.GetCaptchaB64(4, out string code);
            Session["Captcha"] = code; // Save to session (encrypted in cookie)
            XFrameOptionsDeny();
            var vm = new RegisterViewModel()
            {AcceptEmailsNotify = true, CaptchaSrcB64 = captchaSrcB64, RefCode = refcode, };
            return View(vm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> SendEmailConfirmation()
        {
            var userId = User.Identity.GetUserId();
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userId);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new
            {
            userId = userId, code = code
            }

            , protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userId, "Confirm your account", "Please confirm your account by clicking or navigating to the following link: <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>");
            return RedirectToAction(actionName: "Index", controllerName: "Manage", routeValues: null);
        }

        //
        // POST: /Account/Register
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            // Check Captcha
            var captchaCode = ControllerContext.HttpContext.Session["Captcha"].ToString();
            if (model.Captcha != captchaCode)
            {
                ModelState.AddModelError("Captcha", "Captcha does not match");
            }

            // Check Referral
            if (model.RefCode != null)
            {
                using (var db = new ZapContext())
                {
                    var refExists = await db.Users.Where(u => u.ReferralCode == model.RefCode).AnyAsync();
                    if (!refExists)
                    {
                        ModelState.AddModelError("RefCode", "Referral code not valid");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser{UserName = model.UserName, Email = model.Email};
                var result = await UserManager.CreateAsync(user, model.Password).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false).ConfigureAwait(true);
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id).ConfigureAwait(true);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                    {
                    userId = user.Id, code = code
                    }

                    , protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking or navigating to the following link: <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>").ConfigureAwait(true);
                    // Initialize ZapRead user with default parameters
                    var userId = await UserManager.FindByNameAsync(model.UserName).ConfigureAwait(true);
                    using (var db = new ZapContext())
                    {
                        await EnsureUserExists(userId.Id, db, model.RefCode).ConfigureAwait(true); // This creates the user entry if it doesn't already exist
                        var userSettings = await db.Users.Where(u => u.AppId == userId.Id).Select(u => u.Settings).FirstOrDefaultAsync();
                        if (userSettings != null)
                        {
                            userSettings.AlertOnMentioned = true;
                            userSettings.AlertOnNewPostSubscribedGroup = true;
                            userSettings.AlertOnNewPostSubscribedUser = true;
                            userSettings.AlertOnOwnCommentReplied = true;
                            userSettings.AlertOnOwnPostCommented = true;
                            userSettings.AlertOnPrivateMessage = true;
                            userSettings.AlertOnReceivedTip = true;
                            if (model.AcceptEmailsNotify)
                            {
                                userSettings.NotifyOnMentioned = true;
                                userSettings.NotifyOnNewPostSubscribedGroup = true;
                                userSettings.NotifyOnNewPostSubscribedUser = true;
                                userSettings.NotifyOnOwnCommentReplied = true;
                                userSettings.NotifyOnOwnPostCommented = true;
                                userSettings.NotifyOnPrivateMessage = true;
                                userSettings.NotifyOnReceivedTip = true;
                            }

                            await db.SaveChangesAsync();
                        }
                    }

                    try
                    {
                        ControllerContext.HttpContext.Session.Remove("Captcha");
                    }
                    catch (Exception)
                    {
                    // Not a big deal right now
                    }

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            var captchaSrcB64 = CaptchaService.GetCaptchaB64(4, out string newCode);
            Session["Captcha"] = newCode; // Save to session (encrypted in cookie)
            model.CaptchaSrcB64 = captchaSrcB64; // New image and code
            return View(model);
        }

        /// <summary>
        /// Based on code from https://stackoverflow.com/questions/47300251/how-to-use-windows-speech-synthesizer-in-asp-net-mvc
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> CaptchaAudio()
        {
            var captchaCode = ControllerContext.HttpContext.Session["Captcha"].ToString();
            var key = System.Configuration.ConfigurationManager.AppSettings["SpeechServicesKey"];
            var region = System.Configuration.ConfigurationManager.AppSettings["SpeechServicesRegion"];
            byte[] AudioData = await services.x64.zapread.com.SpeechServices.GetAudio(captchaCode, key, region);
            return File(AudioData, "audio/mpeg");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "Email"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public async Task<ActionResult> UpdateEmail(string Email)
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
                return Json(new
                {
                success = false
                }

                );
            var userInfo = await UserManager.FindByIdAsync(userAppId);
            if (userInfo.Email == Email)
            {
                return Json(new
                {
                success = false, message = "Requested email address is the same as existing."
                }

                );
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users.Include(u => u.Funds).Include(u => u.Funds.Locks).Where(u => u.AppId == userAppId).FirstOrDefaultAsync();
                if (user == null)
                    return Json(new
                    {
                    success = false
                    }

                    );
                string code = await UserManager.GenerateUserTokenAsync("LockAccount", userInfo.Id).ConfigureAwait(true);
                var lockUrl = Url.Action("LockAccount", "Account", new
                {
                userId = userAppId, code = code
                }

                , protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(userId: userInfo.Id, subject: "Email updated", body: "Your email address has been updated to " + Email + "<br/>" + "<br/>" + "If this was not requested by you, please <a href=\"" + lockUrl + "\">" + "click here</a> to lock your account. Contact accounts@zapread.com for assistance." + "<br/>").ConfigureAwait(true);
                userInfo.Email = Email;
                userInfo.EmailConfirmed = false;
                await UserManager.UpdateAsync(userInfo);
                code = await UserManager.GenerateEmailConfirmationTokenAsync(userAppId).ConfigureAwait(true);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                {
                userId = userAppId, code = code
                }

                , protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(userId: userInfo.Id, subject: "Confirm your email", body: "Your email address has been updated." + "<br/>" + "<br/>" + "Please confirm your new email by clicking or navigating to the following link: <br/><a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>" + "<br/>" + "<br/>" + "If this was not requested by you, please <a href=\"" + lockUrl + "\">" + "click here</a> to lock your account. Contact accounts@zapread.com for assistance.").ConfigureAwait(true);
                user.Funds.Locks.Add(new FundsLock()
                {WithdrawLocked = true, TimeStampStarted = DateTime.UtcNow, TimeStampExpired = DateTime.UtcNow + TimeSpan.FromDays(1), Description = "Email address updated", Reason = UserFundLockType.UserUpdatedEmail});
                await db.SaveChangesAsync();
                return Json(new
                {
                success = true
                }

                );
            }
        }

        /// <summary>
        /// Lock a user account, require admin to release
        /// </summary>
        /// <param name = "userId"></param>
        /// <param name = "code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> LockAccount(string userId, string code)
        {
            XFrameOptionsDeny();
            if (userId == null || code == null)
            {
                return View("Error");
            }

            // validate code
            if (!(await UserManager.VerifyUserTokenAsync(userId, "LockAccount", code)))
            {
                return View("Error");
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users.Include(u => u.Funds).Include(u => u.Funds.Locks).Where(u => u.AppId == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return View("Error");
                }

                var lockoutEnd = DateTime.UtcNow + TimeSpan.FromDays(365);
                // If not already locked - add a lock
                if (!user.Funds.Locks.Any(l => l.Reason == UserFundLockType.UserRequestedLock))
                {
                    user.Funds.Locks.Add(new FundsLock()
                    {TimeStampStarted = DateTime.UtcNow, TimeStampExpired = lockoutEnd, Reason = UserFundLockType.UserRequestedLock, DepositLocked = false, SpendLocked = true, TransferLocked = true, WithdrawLocked = true, });
                }

                await db.SaveChangesAsync();
                await UserManager.SetLockoutEnabledAsync(userId, true);
                await UserManager.SetLockoutEndDateAsync(userId, lockoutEnd);
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return View("Locked");
            }
        }

        //
        // GET: /Account/ConfirmEmail
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userId"></param>
        /// <param name = "code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            XFrameOptionsDeny();
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code).ConfigureAwait(true);
            if (result.Succeeded)
            {
                using (var db = new ZapContext())
                {
                    var user = await db.Users.Include(u => u.Funds).Include(u => u.Funds.Locks).Where(u => u.AppId == userId).FirstOrDefaultAsync();
                    var locksToRemove = user.Funds.Locks.Where(l => l.Reason == UserFundLockType.UserUpdatedEmail).ToList();
                    db.Locks.RemoveRange(locksToRemove);
                    //foreach(var l in locksToRemove)
                    //{    
                    //    user.Funds.Locks.Remove(l);
                    //}
                    await db.SaveChangesAsync();
                }
            }

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            XFrameOptionsDeny();
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email).ConfigureAwait(true);
                if (user == null) // || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id).ConfigureAwait(true);
                var callbackUrl = Url.Action("ResetPassword", "Account", new
                {
                userId = user.Id, code = code
                }

                , protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(userId: user.Id, subject: "Reset Password", body: "Your username is " + user.UserName + ".<br/>Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>, or pasting the following address into your browser: " + callbackUrl).ConfigureAwait(true);
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPasswordConfirmation()
        {
            XFrameOptionsDeny();
            return View();
        }

        //
        // GET: /Account/ResetPassword
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPassword(string code)
        {
            XFrameOptionsDeny();
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email).ConfigureAwait(true);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password).ConfigureAwait(true);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(result);
            return View();
        }

        /// <summary>
        /// GET: /Account/ResetPasswordConfirmation
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPasswordConfirmation()
        {
            XFrameOptionsDeny();
            return View();
        }

        /// <summary>
        /// POST: /Account/ExternalLogin
        /// </summary>
        /// <param name = "provider"></param>
        /// <param name = "returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // https://stackoverflow.com/questions/20737578/asp-net-sessionid-owin-cookies-do-not-send-to-browser
            ControllerContext.HttpContext.Session.RemoveAll();
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new
            {
            ReturnUrl = returnUrl
            }

            ));
        }

        //
        // GET: /Account/SendCode
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "returnUrl"></param>
        /// <param name = "rememberMe"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            XFrameOptionsDeny();
            var userId = await SignInManager.GetVerifiedUserIdAsync().ConfigureAwait(true);
            if (userId == null)
            {
                return View("Error");
            }

            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId).ConfigureAwait(true);
            var user = await UserManager.FindByIdAsync(userId).ConfigureAwait(true);
            if (!user.IsEmailAuthenticatorEnabled)
            {
                // remove "Email Code" from list if it is there
                userFactors.Remove("Email Code");
            }

            if (!user.IsGoogleAuthenticatorEnabled)
            {
                // remove "Google Authenticator"
                userFactors.Remove("Google Authenticator");
            }

            var factorOptions = userFactors.Select(purpose => new SelectListItem{Text = purpose, Value = purpose}).ToList();
            return View(new SendCodeViewModel{Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe});
        }

        //
        // POST: /Account/SendCode
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider).ConfigureAwait(true))
            {
                return View("Error");
            }

            return RedirectToAction("VerifyCode", new
            {
            Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe
            }

            );
        }

        //
        // GET: /Account/ExternalLoginCallback
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            XFrameOptionsDeny();
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync().ConfigureAwait(true);
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false).ConfigureAwait(true);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new
                    {
                    ReturnUrl = returnUrl, RememberMe = false
                    }

                    );
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel{Email = loginInfo.Email});
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "model"></param>
        /// <param name = "returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync().ConfigureAwait(true);
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser{UserName = model.UserName, Email = model.Email};
                var result = await UserManager.CreateAsync(user).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login).ConfigureAwait(true);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false).ConfigureAwait(true);
                        // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id).ConfigureAwait(true);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                        {
                        userId = user.Id, code = code
                        }

                        , protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking or navigating to the following link: <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>").ConfigureAwait(true);
                        // Initialize ZapRead user with default parameters
                        var userId = await UserManager.FindByNameAsync(model.UserName).ConfigureAwait(true);
                        using (var db = new ZapContext())
                        {
                            await EnsureUserExists(userId.Id, db).ConfigureAwait(true);
                            var userSettings = await db.Users.Where(u => u.AppId == userId.Id).Select(u => u.Settings).FirstOrDefaultAsync();
                            if (userSettings != null)
                            {
                                userSettings.AlertOnMentioned = true;
                                userSettings.AlertOnNewPostSubscribedGroup = true;
                                userSettings.AlertOnNewPostSubscribedUser = true;
                                userSettings.AlertOnOwnCommentReplied = true;
                                userSettings.AlertOnOwnPostCommented = true;
                                userSettings.AlertOnPrivateMessage = true;
                                userSettings.AlertOnReceivedTip = true;
                                if (model.AcceptEmailsNotify)
                                {
                                    userSettings.NotifyOnMentioned = true;
                                    userSettings.NotifyOnNewPostSubscribedGroup = true;
                                    userSettings.NotifyOnNewPostSubscribedUser = true;
                                    userSettings.NotifyOnOwnCommentReplied = true;
                                    userSettings.NotifyOnOwnPostCommented = true;
                                    userSettings.NotifyOnPrivateMessage = true;
                                    userSettings.NotifyOnReceivedTip = true;
                                }

                                await db.SaveChangesAsync();
                            }
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }

                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ExternalLoginFailure()
        {
            XFrameOptionsDeny();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Add parameter l = 1 to invalidate cache after login
            return RedirectToAction("Index", "Home", new
            {
            l = 1
            }

            );
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }

            public string RedirectUri { get; set; }

            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties{RedirectUri = RedirectUri};
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

#endregion
        private void XFrameOptionsDeny()
        {
            try
            {
                Response.AddHeader("X-Frame-Options", "DENY");
            }
            catch
            {
            // TODO: add error handling - temp fix for unit test.
            }
        }
    }
}