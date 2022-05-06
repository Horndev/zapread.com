using System;

namespace zapread.com.Models.UserViews
{
    /// <summary>
    /// 
    /// </summary>
    public class UserHoverViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ProfileImageVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 Reputation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFollowing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsIgnored { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOnline { get; set; }
        /// <summary>
        /// Hover is on currently logged in user
        /// </summary>
        public bool IsSelf { get; set; }
    }
}