using RestSharp;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;

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
        /// <param name = "groupId"></param>
        /// <param name = "userId"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public async static Task NotifyGroupAdminAdded(int groupId, int userId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var notificationInfo = await db.Users.Where(u => u.Id == userId).Select(u => new
                {
                UserAppId = u.AppId, }).FirstOrDefaultAsync().ConfigureAwait(true);
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                await PostMessage(userId: notificationInfo.UserAppId, reason: "Group administration granted", clickUrl: urlHelper.Action(actionName: "GroupDetail", controllerName: "Group", routeValues: new
                {
                id = groupId
                }

                ), message: "You are now an administrator of " + db.Groups.Where(g => g.GroupId == groupId).Select(g => g.GroupName).FirstOrDefault()).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "groupId"></param>
        /// <param name = "userId"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public async static Task NotifyGroupModAdded(int groupId, int userId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var notificationInfo = await db.Users.Where(u => u.Id == userId).Select(u => new
                {
                UserAppId = u.AppId
                }).FirstOrDefaultAsync().ConfigureAwait(true);
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                await PostMessage(userId: notificationInfo.UserAppId, reason: "Group moderation granted", clickUrl: urlHelper.Action(actionName: "GroupDetail", controllerName: "Group", routeValues: new
                {
                id = groupId
                }

                ), message: "You are now a moderator of " + db.Groups.Where(g => g.GroupId == groupId).Select(g => g.GroupName).FirstOrDefault()).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userIdFollowed"></param>
        /// <param name = "userIdFollowing"></param>
        /// <returns></returns>
        public async static Task NotifyNewUserFollowing(int userIdFollowed, int userIdFollowing)
        {
            using (var db = new ZapContext())
            {
                var user = await db.Users.Where(u => u.Id == userIdFollowed).FirstOrDefaultAsync();
            // Not yet implemented
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "postId"></param>
        /// <returns></returns>
        public async static Task NotifyNewPostToFollowers(int postId)
        {
            // Not yet implemented
            await Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <returns></returns>
        public async static Task NotifyPostCommentToFollowers(long commentId)
        {
            // Not yet implemented
            await Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <returns></returns>
        public async static Task NotifyPostCommentToAuthor(long commentId)
        {
            // Not yet implemented
            await Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <returns></returns>
        public async static Task NotifyPostCommentReplyToAuthor(long commentId)
        {
            // Not yet implemented
            await Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <returns></returns>
        public async static Task NotifyUserMentionedInComment(long commentId)
        {
            // Not yet implemented
            await Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "postId"></param>
        /// <returns></returns>
        public async static Task NotifyUserMentionedInPost(long postId)
        {
            // Not yet implemented
            await Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "chatId"></param>
        /// <returns></returns>
        public async static Task NotifyNewChat(int chatId)
        {
            // await NotificationService.SendPrivateMessage(cleanContent, receiver.AppId, "Private Message From " + sender.Name, Url.Action("Chat", "Messages", new { username = sender.Name }));
            using (var db = new ZapContext())
            {
                var msg = await db.Messages.Where(m => m.Id == chatId).Select(m => new
                {
                ReceiverOnline = m.To.IsOnline, ReceiverAppId = m.To.AppId, SenderName = m.From.Name, Content = m.Content, }).FirstOrDefaultAsync();
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                await SendPrivateMessage(msg.Content, msg.ReceiverAppId, "Private Message From " + msg.SenderName, urlHelper.Action("Chat", "Messages", new
                {
                username = msg.SenderName
                }

                ));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userId"></param>
        /// <param name = "invoice"></param>
        /// <param name = "userBalance"></param>
        /// <param name = "txid"></param>
        /// <returns></returns>
        public async static Task SendPaymentNotification(string userId, string invoice, double userBalance, int txid)
        {
            //var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //context.Clients.All.NotifyInvoicePaid(new { invoice = i.PaymentRequest, balance = userBalance, txid = i.Id });
            var url = ConfigurationManager.AppSettings.Get("wshost");
            url = url + "/api/";
            RestClient client = new RestClient(url);
            var request = (new RestRequest("payment/complete", Method.Post)
            {RequestFormat = DataFormat.Json}).AddJsonBody(new
            {
            toUserId = userId, invoice, balance = userBalance, txid, }

            );
            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
            // Good!
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userId"></param>
        /// <param name = "callback"></param>
        /// <param name = "token"></param>
        /// <returns></returns>
        public async static Task SendLnAuthLoginNotification(string userId, string callback, string token)
        {
            var url = ConfigurationManager.AppSettings.Get("wshost");
            url = url + "/api/";
            RestClient client = new RestClient(url);
            var request = (new RestRequest("auth/lnauthcb", Method.Post)
            {RequestFormat = DataFormat.Json}).AddJsonBody(new
            {
            toUserId = userId, Callback = callback, Token = token, }

            );
            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
            // Good!
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "amount"></param>
        /// <param name = "userId"></param>
        /// <param name = "reason"></param>
        /// <param name = "clickUrl"></param>
        /// <returns></returns>
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
            var request = (new RestRequest("message/send", Method.Post)
            {RequestFormat = DataFormat.Json}).AddJsonBody(new
            {
            content = message, toUserId = userId, clickUrl, reason = reason != null ? reason : "Message Received", }

            );
            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
            // Good!
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "content"></param>
        /// <param name = "userId"></param>
        /// <param name = "reason"></param>
        /// <param name = "clickUrl"></param>
        /// <returns></returns>
        public async static Task SendPrivateMessage(string content, string userId, string reason, string clickUrl)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //context.Clients.Group(groupName: userId).SendUserMessage(new { message = content, reason, hasReason = reason != null, clickUrl });
            await PostMessage(userId, reason, clickUrl, content).ConfigureAwait(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "HTMLString"></param>
        /// <param name = "userId"></param>
        /// <param name = "senderUserId"></param>
        /// <param name = "clickUrl"></param>
        /// <returns></returns>
        public async static Task SendPrivateChat(string HTMLString, string userId, string senderUserId, string clickUrl)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //context.Clients.Group(groupName: userId).SendUserChat(new { message = HTMLString, reason, hasReason = reason != null, clickUrl });
            // Use Proxy Service with SignalR
            var url = ConfigurationManager.AppSettings.Get("wshost");
            url = url + "/api/";
            RestClient client = new RestClient(url);
            var request = (new RestRequest("chat/send", Method.Post)
            {RequestFormat = DataFormat.Json}).AddJsonBody(new
            {
            HTMLString, toUserId = userId, fromUserId = senderUserId, }

            );
            var response = await client.ExecuteAsync(request).ConfigureAwait(true);
            if (response.IsSuccessful)
            {
            // Good!
            }
        }
    }
}