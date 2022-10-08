using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Home
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeIndexViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
        public List<GroupInfo> SubscribedGroups { get; set; }
    }
}