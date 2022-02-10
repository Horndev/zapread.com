using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using realtime.zapread.com.Hubs;
using realtime.zapread.com.Models.API;
using System;
using System.Threading.Tasks;

namespace realtime.zapread.com.Controllers
{
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IHubContext<NotificationHub> _hub;

        public AuthController(ILogger<PaymentController> logger, IHubContext<NotificationHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        [HttpPost]
        [Route("api/auth/lnauthcb")]
        public async Task<IActionResult> SendMessage([FromBody] AuthMessage message)
        {
            if (String.IsNullOrEmpty(message.toUserId))
            {
                // await _hub.Clients.All.SendAsync("LnAuthLogin", message.Callback, message.Token);
            }
            else
            {
                // Broadcast to requested user
                await _hub.Clients.Group(groupName: message.toUserId).SendAsync("LnAuthLogin", message.Callback, message.Token);
            }
            return Ok();
        }
    }
}
