using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using realtime.zapread.com.Hubs;
using realtime.zapread.com.Models.API;

namespace realtime.zapread.com.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<NotificationHub> _hub;

        public ChatController(ILogger<ChatController> logger, IHubContext<NotificationHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        [HttpGet]
        [Route("api/chat")]
        public string Get()
        {
            return "This endpoint manages zapread.com chats.";
        }

        [HttpGet]
        [Route("api/chat/test")]
        public string Test()
        {
            return "!";
        }

        [HttpPost]
        [Route("api/chat/send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            await _hub.Clients.Group(groupName: message.toUserId).SendAsync("SendUserChat", message.HTMLString, message.fromUserId);

            return Ok();
        }
    }
}
