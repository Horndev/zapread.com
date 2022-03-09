using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.GroupViews
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupHoverViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int GroupLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int GroupPostCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int GroupMemberCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMember { get; set; }
    }
}