using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using zapread.com.App_Start;

namespace zapread.com
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            MvcSiteMapProviderConfig.Register(CompositionRoot.Compose());
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
