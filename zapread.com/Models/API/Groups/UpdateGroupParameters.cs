using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateGroupParameters: AddGroupParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public int GroupId { get; set; }
    }
}