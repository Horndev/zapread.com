using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class GetMoreCommentsParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Rootshown { get; set; }

        /// <summary>
        /// Default -1 means root
        /// </summary>
        public int ParentCommentId { get; set; }
    }
}