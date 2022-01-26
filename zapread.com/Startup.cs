using Hangfire;
using Microsoft.Owin;
using Owin;
using System.Web;

[assembly: OwinStartupAttribute(typeof(zapread.com.Startup))]
namespace zapread.com
{
    /// <summary>
    /// Startup configuration for application
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Main configuration injection
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            // Set DB used by Hangfire
            GlobalConfiguration.Configuration.UseSqlServerStorage(System.Configuration.ConfigurationManager.AppSettings["SiteConnectionString"]);

            ConfigureAuth(app);

            //app.MapSignalR();

            var options = new DashboardOptions
            {
                AppPath = VirtualPathUtility.ToAbsolute("~"),
                Authorization = new[] { new ZapReadHangFireAuthFilter() }
            };
            app.UseHangfireDashboard("/hangfire", options);
            app.UseHangfireServer();
        }
    }
}
