using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using realtime.zapread.com.Hubs;
using realtime.zapread.com.Models.API;

namespace realtime.zapread.com.Controllers
{
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IHubContext<NotificationHub> _hub;

        public MessageController(ILogger<MessageController> logger, IHubContext<NotificationHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        [HttpPost]
        [Route("api/message/send")]
        public async Task<IActionResult> SendMessage([FromBody] UserMessage message)
        {
            await _hub.Clients.Group(groupName: message.toUserId).SendAsync("UserMessage", message.content, message.reason, message.clickUrl);
            return Ok();
        }
    }
}