using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.API.User
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

        /// <summary>
        /// Reactions most used by user
        /// </summary>
        public List<ReactionItem> CommonReactions { get; set; }
    }
}