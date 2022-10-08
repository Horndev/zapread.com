using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class NewChatEmail : Postal.Email
    {
        /// <summary>
        /// 
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReceived { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FromAppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int FromProfileImgVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
    }
}