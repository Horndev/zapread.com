using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    public class UserSettings
    {
        public int Id { get; set; }

        // Emails:
        public bool NotifyOnOwnPostCommented { get; set; }
        public bool NotifyOnOwnCommentReplied { get; set; }
        public bool NotifyOnNewPostSubscribedGroup { get; set; }
        public bool NotifyOnNewPostSubscribedUser { get; set; }
        public bool NotifyOnReceivedTip { get; set; }
        public bool NotifyOnPrivateMessage { get; set; }
        public bool NotifyOnMentioned { get; set; }

        // Alerts:
        public bool AlertOnOwnPostCommented { get; set; }
        public bool AlertOnOwnCommentReplied { get; set; }
        public bool AlertOnNewPostSubscribedGroup { get; set; }
        public bool AlertOnNewPostSubscribedUser { get; set; }
        public bool AlertOnReceivedTip { get; set; }
        public bool AlertOnPrivateMessage { get; set; }
        public bool AlertOnMentioned { get; set; }

        // Customization
        public string ColorTheme { get; set; }
        public bool CollapseDiscussions { get; set; }

        // Features
        public bool ShowTours { get; set; }
        public bool ShowOnline { get; set; }

        // Languages
        public bool ViewAllLanguages { get; set; }
        public bool ViewTranslatedLanguages { get; set; }
    }
}