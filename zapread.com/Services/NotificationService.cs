using Microsoft.AspNet.SignalR;
using zapread.com.Hubs;

namespace zapread.com.Services
{
    public class NotificationService
    {
        public static void SendIncomeNotification(double amount, string userId, string reason, string clickUrl)
        {
            string message = "You just earned " + amount.ToString("0.##") + " Satoshi.";

            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            context.Clients.Group(groupName: userId).SendUserMessage(new { message, reason, hasReason = reason != null, clickUrl });
        }

        public static void SendPrivateMessage(string content, string userId, string reason, string clickUrl)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(groupName: userId).SendUserMessage(new { message = content, reason, hasReason = reason != null, clickUrl });
        }

        public static void SendPrivateChat(string HTMLString, string userId, string reason, string clickUrl)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(groupName: userId).SendUserChat(new { message = HTMLString, reason, hasReason = reason != null, clickUrl });
        }
    }
}