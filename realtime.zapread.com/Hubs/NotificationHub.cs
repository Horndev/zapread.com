using Microsoft.AspNetCore.SignalR;
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

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");

            await Clients.All.SendAsync("ReceiveMessage", "A", "message from server: connected!");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
