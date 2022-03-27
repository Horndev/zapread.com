using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateThemeParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public int GroupId { get; set; 
        }
        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }
    }
}