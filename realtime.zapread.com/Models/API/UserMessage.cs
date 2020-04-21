using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace realtime.zapread.com.Models.API
{
    public class UserMessage
    {
        public string toUserId { get; set; }

        public string clickUrl { get; set; }

        public string content { get; set; }

        public string reason { get; set; }
    }
}
