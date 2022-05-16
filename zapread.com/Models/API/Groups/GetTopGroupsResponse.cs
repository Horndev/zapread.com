using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class GetTopGroupsResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<GroupInfo> Groups { get; set; }
    }
}