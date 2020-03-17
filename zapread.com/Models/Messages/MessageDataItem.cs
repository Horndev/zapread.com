using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Messages
{
    // used for The message table in /Messages/GetMessagesTable/
    public class MessageDataItem
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string FromID { get; set; }
        public string Date { get; set; }
        public string Link { get; set; }
        public string Anchor { get; set; }
        public string Message { get; set; }
    }
}