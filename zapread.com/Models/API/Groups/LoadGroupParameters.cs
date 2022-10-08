using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// At least one of groupid and groupName must be valid
    /// </summary>
    public class LoadGroupParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public int? groupId { get; set; }

        /// <summary>
        /// Group name
        /// </summary>
        public string groupName { get; set; }
    }
}