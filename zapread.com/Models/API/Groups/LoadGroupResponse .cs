using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class LoadGroupResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public int groupId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public GroupInfo group { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsLoggedIn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
    }
}