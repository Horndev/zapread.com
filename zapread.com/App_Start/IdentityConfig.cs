using Base32;
using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using OtpSharp;
using System;
using System.Data.Entity.Utilities;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using zapread.com.Models;
using zapread.com.Services;

namespace zapread.com
{
    /// <summary>
    /// Send identity service emails
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        /// <summary>
        /// Send out email message
        /// </summary>
        /// <param name = "message">Message contents</param>
        /// <returns>void</returns>
        public async Task SendAsync(IdentityMessage message)
        {
            if (message == null)
            {
                return;
            }

            await MailingService.SendAsync(user: "Accounts", useSSL: true, message: new UserEmailModel()
            {Destination = message.Destination, Body = message.Body, Email = "", Name = "zapread.com", Subject = message.Subject, }).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SmsService : IIdentityMessageService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "message"></param>
        /// <returns></returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "store"></param>
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
        }

        /// <summary>
        /// Access method for checking if GA is enabled
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> IsEmailAuthenticatorEnabledAsync(string userAppId)
        {
            var user = await FindByIdAsync(userAppId).ConfigureAwait(true);
            return user.IsEmailAuthenticatorEnabled;
        }

        /// <summary>
        /// Access method for checking if GA is enabled
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> IsGoogleAuthenticatorEnabledAsync(string userAppId)
        {
            var user = await FindByIdAsync(userAppId).ConfigureAwait(true);
            return user.IsGoogleAuthenticatorEnabled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "options"></param>
        /// <param name = "context"></param>
        /// <returns></returns>
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {AllowOnlyAlphanumericUserNames = false, RequireUniqueEmail = true};
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator{RequiredLength = 6, RequireNonLetterOrDigit = false, RequireDigit = false, RequireLowercase = false, RequireUppercase = false, };
            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            //manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            //{
            //    MessageFormat = "Your security code is {0}"
            //});
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>{Subject = "Security Code", BodyFormat = "Your security code is {0}"});
            manager.RegisterTwoFactorProvider("Google Authenticator", new GoogleAuthenticatorTokenProvider());
            manager.EmailService = new EmailService();
            //manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            return manager;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GoogleAuthenticatorTokenProvider : IUserTokenProvider<ApplicationUser, string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "purpose"></param>
        /// <param name = "manager"></param>
        /// <param name = "user"></param>
        /// <returns></returns>
        public Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            return Task.FromResult((string)null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "purpose"></param>
        /// <param name = "token"></param>
        /// <param name = "manager"></param>
        /// <param name = "user"></param>
        /// <returns></returns>
        public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            long timeStepMatched = 0;
            var otp = new Totp(Base32Encoder.Decode(user.GoogleAuthenticatorSecretKey));
            bool valid = otp.VerifyTotp(token, out timeStepMatched, new VerificationWindow(2, 2));
            return Task.FromResult(valid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "token"></param>
        /// <param name = "manager"></param>
        /// <param name = "user"></param>
        /// <returns></returns>
        public Task NotifyAsync(string token, UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "manager"></param>
        /// <param name = "user"></param>
        /// <returns></returns>
        public Task<bool> IsValidProviderForUserAsync(UserManager<ApplicationUser, string> manager, ApplicationUser user)
        {
            return Task.FromResult(user.IsGoogleAuthenticatorEnabled);
        }
    }

    /// <summary>
    /// // Configure the application sign-in manager which is used in this application.
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userManager"></param>
        /// <param name = "authenticationManager"></param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) : base(userManager, authenticationManager)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "user"></param>
        /// <returns></returns>
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "options"></param>
        /// <param name = "context"></param>
        /// <returns></returns>
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "store"></param>
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> store) : base(store)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "options"></param>
        /// <param name = "context"></param>
        /// <returns></returns>
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var roleStore = new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>());
            return new ApplicationRoleManager(roleStore);
        }
    }
}