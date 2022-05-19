using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Tag
{
    /// <summary>
    /// 
    /// </summary>
    public class TagItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string link { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool newtag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int count { get; set; }
    }
}