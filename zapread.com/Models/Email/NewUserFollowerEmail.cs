using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class NewUserFollowerEmail : Postal.Email
    {
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
    }
}