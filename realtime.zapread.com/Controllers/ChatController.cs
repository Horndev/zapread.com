using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace realtime.zapread.com.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
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
        public string SendMessage(string message)
        {
            return message;
        }
    }
}
