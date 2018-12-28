using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    public class MessagesViewModel
    {
        public List<UserMessage> Messages = new List<UserMessage>();
        public List<UserAlert> Alerts = new List<UserAlert>();
    }

    public class UnreadModel
    {
        public int NumUnread = 0;
    }

    public class RecentUnreadMessagesViewModel
    {
        public List<UserMessage> Messages = new List<UserMessage>();
    }
}