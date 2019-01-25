using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using zapread.com.Models;
using Owin.Security.Providers.Reddit;
using Owin.Security.Providers.GitHub;
using Microsoft.Owin.Security.Twitter;
using Microsoft.Owin.Security;

namespace zapread.com
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                CookieName = "ZapreadUserAuth",
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            //app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: System.Configuration.ConfigurationManager.AppSettings["OAuth_Twitter_Key"],
            //   consumerSecret: System.Configuration.ConfigurationManager.AppSettings["OAuth_Twitter_Secret"]);

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = System.Configuration.ConfigurationManager.AppSettings["OAuth_Google_ClientId"],
                ClientSecret = System.Configuration.ConfigurationManager.AppSettings["OAuth_Google_Secret"],
            });

            app.UseRedditAuthentication(new RedditAuthenticationOptions()
            {
                ClientId = System.Configuration.ConfigurationManager.AppSettings["OAuth_Reddit_ClientId"],
                ClientSecret = System.Configuration.ConfigurationManager.AppSettings["OAuth_Reddit_Secret"],
            });

            app.UseGitHubAuthentication(new GitHubAuthenticationOptions()
            {
                ClientId = System.Configuration.ConfigurationManager.AppSettings["OAuth_Github_ClientId"],
                ClientSecret = System.Configuration.ConfigurationManager.AppSettings["OAuth_Github_Secret"],
            });

            //app.UseTwitterAuthentication(new TwitterAuthenticationOptions()
            //{
            //    ConsumerKey = System.Configuration.ConfigurationManager.AppSettings["OAuth_Twitter_Key"],
            //    ConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["OAuth_Twitter_Secret"],
            //    BackchannelCertificateValidator = new CertificateSubjectKeyIdentifierValidator(new[] {
            //        "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA – G2
            //        "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA – G3
            //        "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority – G5
            //        "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA – G4
            //        "5168FF90AF0207753CCCD9656462A212B859723B", //DigiCert SHA2 High Assurance Server C‎A
            //        "B13EC36903F8BF4701D498261A0802EF63642BC3" //DigiCert High Assurance EV Root CA
            //        })
            //});
        }
    }
}