using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace zapread.com.Models
{
    /// <summary>
    /// // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// https://www.jerriepelser.com/blog/using-google-authenticator-asp-net-identity/
        /// </summary>
        public bool IsGoogleAuthenticatorEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsEmailAuthenticatorEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GoogleAuthenticatorSecretKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("UserName", this.UserName));

            return userIdentity;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// 
        /// </summary>
        public ApplicationDbContext()
            : base(System.Configuration.ConfigurationManager.AppSettings["SiteConnectionString"], 
                  throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}