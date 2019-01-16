using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
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

        public override Task OnConnected()
        {
            string name = Context.User.Identity.GetUserId();

            if (name != null)
            {
                Groups.Add(Context.ConnectionId, name);

                try
                {
                    using (var db = new ZapContext())
                    {
                        var user = db.Users
                            .SingleOrDefault(u => u.AppId == name);

                        user.DateLastActivity = DateTime.UtcNow;
                        user.IsOnline = true;
                        db.SaveChanges();
                    }
                }
                catch
                {

                }
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.GetUserId();

            if (name != null)
            {
                try
                {
                    using (var db = new ZapContext())
                    {
                        var user = db.Users
                            .SingleOrDefault(u => u.AppId == name);

                        user.DateLastActivity = DateTime.UtcNow;
                        user.IsOnline = false;
                        db.SaveChanges();
                    }
                }
                catch
                {

                }
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}