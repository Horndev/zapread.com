using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.API.Admin
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