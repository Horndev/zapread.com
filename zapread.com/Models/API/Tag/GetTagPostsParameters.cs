using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Tag
{
    /// <summary>
    /// Parameters for the GetPosts request
    /// </summary>
    public class GetTagPostsParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// Groupid of the group to get posts from
        /// </summary>
        public int? TagId { get; set; }

        /// <summary>
        /// Block of posts index - starting at 0
        /// </summary>
        public int? blockNumber { get; set; }

        /// <summary>
        /// Sort order for posts
        /// </summary>
        public string sort { get; set; }

        /// <summary>
        /// Number of posts per block
        /// </summary>
        public int? blockSize { get; set; }
    }
}