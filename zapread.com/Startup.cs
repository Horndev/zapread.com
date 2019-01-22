using Microsoft.Owin;
using Owin;
using zapread.com.Database;
using System.Linq;
using System.Collections.Generic;
using zapread.com.Models;

[assembly: OwinStartupAttribute(typeof(zapread.com.Startup))]
namespace zapread.com
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            app.MapSignalR();
        }
    }
}
