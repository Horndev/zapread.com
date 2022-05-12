using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class AddReactionResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<ReactionItem> Reactions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AlreadyReacted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool NotAvailable { get; set; }
    }
}