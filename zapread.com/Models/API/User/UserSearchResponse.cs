using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class UserSearchResponse: ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<UserResultInfo> Users { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserResultInfo
    {
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
        public int ProfileImageVersion { get; set; }
    }
}