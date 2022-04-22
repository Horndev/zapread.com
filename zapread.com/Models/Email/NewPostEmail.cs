using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// Model for Post for when by a user being followed
    /// </summary>
    public class NewPostEmail : Postal.Email
    {
        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PostTitle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserAppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ProfileImageVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
    }
}