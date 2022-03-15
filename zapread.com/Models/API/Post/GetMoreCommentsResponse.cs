using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class GetMoreCommentsResponse : ZapReadResponse
    {
        /// <summary>
        /// A ; separated list of comment IDs
        /// </summary>
        public string Shown { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PostCommentsViewModel> Comments { get; set; }

        /// <summary>
        /// If false, then there are no more comments to fetch from the server after this request
        /// </summary>
        public bool HasMoreComments { get; set; }
    }
}