using System;
using System.Collections.Generic;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MessagesViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public List<UserMessage> Messages = new List<UserMessage>();
        /// <summary>
        /// 
        /// </summary>
        public List<UserAlert> Alerts = new List<UserAlert>();
    }

    /// <summary>
    /// 
    /// </summary>
    public class UnreadModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int NumUnread = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecentUnreadMessagesViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
        public List<UserMessage> Messages = new List<UserMessage>();

        /// <summary>
        /// 
        /// </summary>
        public List<UserMessageVm> MessagesVm { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserMessageVm
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FromAppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int FromProfileImageVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsComment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPrivateMessage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RecentUnreadAlertsViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
        public List<UserAlert> Alerts = new List<UserAlert>();
        /// <summary>
        /// 
        /// </summary>
        public List<UserAlertVm> AlertsVm { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserAlertVm
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool HasPostLink { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PostLinkPostId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PostLinkPostTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChatMessageViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public UserMessage Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FromAppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsReceived { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public User From { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public User To { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FromProfileImgVersion { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChatMessagesViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public User OtherUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public User ThisUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ChatMessageViewModel> Messages = new List<ChatMessageViewModel>();
    }
}