using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace realtime.zapread.com.Models.API
{
    public class ChatMessage
    {
        public string HTMLString { get; set; }
        public string toUserId { get; set; }
        public string  fromUserId { get; set; }
    }
}
