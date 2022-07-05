using Microsoft.Owin.Security;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "currentPrincipal"></param>
        /// <param name = "key"></param>
        /// <param name = "value"></param>
        public static void AddUpdateClaim(this IPrincipal currentPrincipal, string key, string value)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return;
            // check for existing claim and remove it
            var existingClaim = identity.FindFirst(key);
            if (existingClaim != null)
                identity.RemoveClaim(existingClaim);
            // add new claim
            identity.AddClaim(new Claim(key, value));
            if (HttpContext.Current != null)
            {
                var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identity), new AuthenticationProperties()
                {IsPersistent = true});
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "currentPrincipal"></param>
        /// <param name = "key"></param>
        /// <returns></returns>
        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return null;
            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            if (claim == null)
            {
                return "";
            }

            return claim.Value;
        }
    }
}