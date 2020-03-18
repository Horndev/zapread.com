using System;
using System.Collections.Generic;
using zapread.com.Models.Database;

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
        [Obsolete]
        public List<UserMessage> Messages = new List<UserMessage>();

        public List<UserMessageVm> MessagesVm { get; set; }
    }

    public class UserMessageVm
    {
        public int Id { get; set; }

        public string FromName { get; set; }

        public string FromAppId { get; set; }

        public int FromProfileImageVersion { get; set; }

        public bool IsComment { get; set; }

        public int PostId { get; set; }

        public bool IsPrivateMessage { get; set; }

        public string Content { get; set; }
    }

    public class RecentUnreadAlertsViewModel
    {
        [Obsolete]
        public List<UserAlert> Alerts = new List<UserAlert>();
        public List<UserAlertVm> AlertsVm { get; set; }
    }

    public class UserAlertVm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool HasPostLink { get; set; }
        public int PostLinkPostId { get; set; }
        public string PostLinkPostTitle { get; set; }
        public string Content { get; set; }
    }

    public class ChatMessageViewModel
    {
        public UserMessage Message { get; set; }

        public DateTime TimeStamp { get; set; }
        public string Content { get; set; }
        public string FromName { get; set; }
        public string FromAppId { get; set; }
        public bool IsReceived { get; set; }
        public User From { get; set; }
        public User To { get; set; }
        public int FromProfileImgVersion { get; set; }
    }

    public class ChatMessagesViewModel
    {
        public User OtherUser { get; set; }
        public User ThisUser { get; set; }
        public List<ChatMessageViewModel> Messages = new List<ChatMessageViewModel>();
    }
}