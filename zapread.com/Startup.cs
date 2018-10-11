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

            using (var db = new ZapContext())
            {
                var g = db.ZapreadGlobals.FirstOrDefault(i => i.Id == 1);

                // This is run only the first time the app is launched in the database.
                if (g == null)
                {
                    db.ZapreadGlobals.Add(new Models.ZapReadGlobals()
                    {
                        Id = 1,
                        CommunityEarnedToDistribute = 0.0,
                        TotalDepositedCommunity = 0.0,
                        TotalEarnedCommunity = 0.0,
                        TotalWithdrawnCommunity = 0.0,
                        ZapReadEarnedBalance = 0.0,
                        ZapReadTotalEarned = 0.0,
                        ZapReadTotalWithdrawn = 0.0,
                        LNWithdraws = new List<LNTransaction>(),
                    });
                    db.SaveChanges();
                }
            }
        }
    }
}
