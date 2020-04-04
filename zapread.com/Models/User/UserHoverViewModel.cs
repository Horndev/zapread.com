using System;

namespace zapread.com.Models.UserViews
{
    public class UserHoverViewModel
    {
        public int UserId { get; set; }
        public string AppId { get; set; }
        public int ProfileImageVersion { get; set; }
        public string Name { get; set; }
        public Int64 Reputation { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsIgnored { get; set; }
    }
}