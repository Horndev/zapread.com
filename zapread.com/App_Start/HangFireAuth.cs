using Hangfire.Dashboard;
using Microsoft.Owin;

namespace zapread.com
{
    /// <summary>
    /// Authorization for the hangfire dashboard
    /// </summary>
    public class ZapReadHangFireAuthFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Method to return whether request is authorized
        /// </summary>
        /// <param name = "context"></param>
        /// <returns></returns>
        public bool Authorize(DashboardContext context)
        {
            // In case you need an OWIN context, use the next line, `OwinContext` class
            // is the part of the `Microsoft.Owin` package.
            var owinContext = new OwinContext(context.GetOwinEnvironment());
            var isAdmin = owinContext.Authentication.User.IsInRole("Administrator");
            return isAdmin;
        }
    }
}