using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class GetUserInteractionInfo : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsFollowing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsBlocking { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIgnoring { get; set; }
    }
}