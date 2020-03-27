using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using zapread.com.Database;

namespace zapread.com.Hubs
{
    public class NotificationHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        // Send notification to all users
        public void NotifyInvoicePaid(string invoice)
        {
            Clients.All.notifyInvoicePaid(invoice);
        }

        // Send notification only to user
        public void NotifyInvoicePaid(string invoice, string userId)
        {
            Clients.Group(userId).notifyInvoicePaid(invoice);
        }

        public void SendUserMessage(string message, string userId)
        {
            Clients.Group(userId).sendUserMessage(message);
        }

        public override async Task OnConnected()
        {
            string name = Context.User.Identity.GetUserId();

            if (name != null)
            {
                await Groups.Add(Context.ConnectionId, name).ConfigureAwait(true);

                try
                {
                    using (var db = new ZapContext())
                    {
                        var user = await db.Users
                            .Include(usr => usr.Settings)
                            .SingleOrDefaultAsync(u => u.AppId == name).ConfigureAwait(true);

                        if (!user.Settings.ShowOnline)
                        {
                            user.DateLastActivity = DateTime.UtcNow;
                            user.IsOnline = true;
                        }
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }
                catch
                {

                }
            }
            await base.OnConnected().ConfigureAwait(true);
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.GetUserId();

            if (name != null)
            {
                try
                {
                    using (var db = new ZapContext())
                    {
                        var user = await db.Users
                            .Include(usr => usr.Settings)
                            .SingleOrDefaultAsync(u => u.AppId == name).ConfigureAwait(true);

                        if (!user.Settings.ShowOnline)
                        {
                            user.DateLastActivity = DateTime.UtcNow;
                        }
                        user.IsOnline = false;
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }
                catch
                {

                }
            }
            await base.OnDisconnected(stopCalled).ConfigureAwait(true);
        }
    }
}