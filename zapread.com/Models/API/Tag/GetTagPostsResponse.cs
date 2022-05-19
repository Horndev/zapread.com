using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Tag
{
    /// <summary>
    /// Response for the group api get posts response in a batch
    /// </summary>
    public class GetTagPostsResponse : ZapReadResponse
    {
        /// <summary>
        /// There are more posts in the group queue after this batch
        /// </summary>
        public bool HasMorePosts { get; set; }

        /// <summary>
        /// The posts related to the group API request
        /// </summary>
        public List<PostViewModel> Posts { get; set; }

        /// <summary>
        /// Flag to quickly indicate if the request was successful
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public bool success { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}