using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class GetReactionsResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<ReactionItem> Reactions { get; set; }
    }
}