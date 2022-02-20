using System.Web.Mvc;
using System.Web.Routing;

namespace zapread.com
{
    /// <summary>
    /// MVC route configuration
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Register MVC routes
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.AppendTrailingSlash = true;

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
