using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class AddReactionRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ReactionId { get; set; }
    }
}