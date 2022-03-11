using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.GroupViews
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupLinkViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
        public Group Group { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFirstPost { get; set; }
    }
}