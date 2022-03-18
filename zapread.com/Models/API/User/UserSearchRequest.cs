using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class UserSearchRequest
    {
        /// <summary>
        /// Username string or substring
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Max { get; set; }
    }
}