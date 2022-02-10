using RestSharp;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;

namespace zapread.com.Services
{
    /// <summary>
    /// For sending realtime notifications
    /// </summary>
    public class NotificationService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="invoice"></param>
        /// <param name="userBalance"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async static Task SendPaymentNotification(string userId, string invoice, double userBalance, int txid)
        {
            //var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //context.Clients.All.NotifyInvoicePaid(new { invoice = i.PaymentRequest, balance = userBalance, txid = i.Id });

            var url = ConfigurationManager.AppSettings.Get("wshost");
            url = url + "/api/";
            RestClient client = new RestClient(url);
            var request = (new RestRequest("payment/complete", Method.POST) { RequestFormat = DataFormat.Json })
                .AddJsonBody(new
                {
                    toUserId = userId,
                    invoice,
                    balance = userBalance,
                    txid,
                });

            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
                // Good!
            }
        }

        public async static Task SendLnAuthLoginNotification(string userId, string callback, string token)
        {
            var url = ConfigurationManager.AppSettings.Get("wshost");
            url = url + "/api/";
            RestClient client = new RestClient(url);
            var request = (new RestRequest("auth/lnauthcb", Method.POST) { RequestFormat = DataFormat.Json })
                .AddJsonBody(new
                {
                    toUserId = userId,
                    Callback = callback,
                    Token = token,
                });

            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
                // Good!
            }
        }

        public async static Task SendIncomeNotification(double amount, string userId, string reason, string clickUrl)
        {
            string message = "You just earned " + amount.ToString("0.##", CultureInfo.InvariantCulture) + " Satoshi.";

            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //context.Clients.Group(groupName: userId).SendUserMessage(new { message, reason, hasReason = reason != null, clickUrl });

            // Use Proxy Service with SignalR
            await PostMessage(userId, reason, clickUrl, message).ConfigureAwait(true);
        }

        private static async Task PostMessage(string userId, string reason, string clickUrl, string message)
        {
            var url = ConfigurationManager.AppSettings.Get("wshost");
            url = url + "/api/";
            RestClient client = new RestClient(url);
            var request = (new RestRequest("message/send", Method.POST) { RequestFormat = DataFormat.Json })
                .AddJsonBody(new
                {
                    content = message,
                    toUserId = userId,
                    clickUrl,
                    reason = reason != null ? reason : "Message Received",
                });

            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
                // Good!
            }
        }

        public async static Task SendPrivateMessage(string content, string userId, string reason, string clickUrl)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //context.Clients.Group(groupName: userId).SendUserMessage(new { message = content, reason, hasReason = reason != null, clickUrl });
            await PostMessage(userId, reason, clickUrl, content).ConfigureAwait(true);
        }

        public async static Task SendPrivateChat(string HTMLString, string userId, string senderUserId, string clickUrl)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //context.Clients.Group(groupName: userId).SendUserChat(new { message = HTMLString, reason, hasReason = reason != null, clickUrl });

            // Use Proxy Service with SignalR
            var url = ConfigurationManager.AppSettings.Get("wshost");
            url = url + "/api/";
            RestClient client = new RestClient(url);
            var request = (new RestRequest("chat/send", Method.POST){ RequestFormat = DataFormat.Json })
                .AddJsonBody(new
                {
                    HTMLString,
                    toUserId = userId,
                    fromUserId = senderUserId,
                });

            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
                // Good!
            }
        }
    }
}