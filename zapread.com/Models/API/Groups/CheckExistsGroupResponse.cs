using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class CheckExistsGroupResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public bool exists { get; set; }
    }
}