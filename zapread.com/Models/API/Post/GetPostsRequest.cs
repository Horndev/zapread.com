using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class GetPostsRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? BlockNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? BlockSize { get; set; }
    }
}