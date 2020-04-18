using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace realtime.zapread.com.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendUserChat(string message, string userId, string reason, string clickUrl)
        {
            await Clients.All.SendAsync("ReceiveMessage", userId, message);
        }
    }
}
