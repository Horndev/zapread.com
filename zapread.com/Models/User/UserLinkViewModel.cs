using System;
using zapread.com.Models.Database;

namespace zapread.com.Models.UserViews
{
    /// <summary>
    /// 
    /// </summary>
    public class UserLinkViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
        public User User { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserAppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsIgnored { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFirstPost { get; set; }
    }
}