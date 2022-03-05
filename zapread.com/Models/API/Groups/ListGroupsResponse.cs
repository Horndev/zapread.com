using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class ListGroupsResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public int draw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int recordsTotal {get; set;}
        /// <summary>
        /// 
        /// </summary>
        public int recordsFiltered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<GroupInfo> data { get; set; }
    }
}