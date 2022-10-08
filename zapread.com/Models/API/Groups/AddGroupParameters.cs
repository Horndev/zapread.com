using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class AddGroupParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ImageId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Captcha { get; set; }
    }
}