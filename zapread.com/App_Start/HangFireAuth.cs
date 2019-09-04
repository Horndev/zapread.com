using Hangfire.Dashboard;
using Microsoft.Owin;

namespace zapread.com
{
    public class ZapReadHangFireAuthFilter : IDashboardAuthorizationFilter
    {
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