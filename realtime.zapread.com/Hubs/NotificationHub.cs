using Microsoft.AspNetCore.SignalR;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace realtime.zapread.com.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendUserChat(string userId, string message, string reason, string clickUrl)
        {
            await Clients.All.SendAsync("ReceiveMessage", userId, message);
        }

        public async Task SendMessage(string fromUserId, string toUserId, string message)
        {
            await Clients.Group(groupName: toUserId).SendAsync("SendUserChat", message, fromUserId);
        }

        // In the future I would like to have cross-subdomain cookie sharing with .NET 4.7 and .NET Core for authentication.  
        //  It is possible but not implemented yet in this project.
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var tokenValue = httpContext.Request.Query["a"];

            if (tokenValue.Count > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, tokenValue);

                await Clients.All.SendAsync("ReceiveMessage", tokenValue, "connected!");

                // Notify ZapRead stream:   api/v1/stream/notify/connected/{userAppId}
                RestClient client = new RestClient("http://localhost:27543/api/v1/");
                await client.ExecuteAsync(
                    new RestRequest("stream/notify/connected/{userAppId}", Method.GET)
                    .AddUrlSegment("userAppId", tokenValue));
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //await Groups.RemoveFromGroupAsync(Context.ConnectionId, "");
            //var c = Context;

            await base.OnDisconnectedAsync(exception);
        }
    }
}
