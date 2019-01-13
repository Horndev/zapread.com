using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

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
            //Clients.All.notifyInvoicePaid(invoice);
            Clients.Group(userId).notifyInvoicePaid(invoice);
        }

        public override Task OnConnected()
        {
            string name = Context.User.Identity.GetUserId();//.Name;

            if (name != null)
            {
                Groups.Add(Context.ConnectionId, name);
            }
            
            return base.OnConnected();
        }
    }
}