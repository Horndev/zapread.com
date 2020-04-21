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
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IHubContext<NotificationHub> _hub;

        public PaymentController(ILogger<PaymentController> logger, IHubContext<NotificationHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        [HttpPost]
        [Route("api/payment/complete")]
        public async Task<IActionResult> SendMessage([FromBody] PaymentMessage message)
        {
            if (String.IsNullOrEmpty(message.toUserId))
            {
                // This was likely an anonymous vote - broadcast since the originator is not known
                await _hub.Clients.All.SendAsync("Payment", message.invoice, message.balance, message.txid);
            }
            else
            {
                // Broadcast to requested user
                await _hub.Clients.Group(groupName: message.toUserId).SendAsync("Payment", message.invoice, message.balance, message.txid);

            }
            return Ok();
        }
    }
}