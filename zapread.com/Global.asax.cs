using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace zapread.com
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_BeginRequest()
        //{
        //    if (!Context.Request.IsSecureConnection)
        //    {
        //        // This is an insecure connection, so redirect to the secure version
        //        UriBuilder uri = new UriBuilder(Context.Request.Url);
        //        if (!uri.Host.Equals("localhost"))
        //        {
        //            uri.Port = 443;
        //            uri.Scheme = "https";
        //            Response.Redirect(uri.ToString());
        //        }
        //    }
        //}
    }
}
