using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class GetPostResponse : ZapReadResponse
    {
        /// <summary>
        /// The posts related to the group API request
        /// </summary>
        public PostViewModel Post { get; set; }
    }
}