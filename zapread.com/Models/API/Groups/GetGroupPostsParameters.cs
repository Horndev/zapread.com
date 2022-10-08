using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// Parameters for the GetPosts request
    /// </summary>
    public class GetGroupPostsParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public string groupName { get; set; }
        /// <summary>
        /// Groupid of the group to get posts from
        /// </summary>
        public int? groupId { get; set; }

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