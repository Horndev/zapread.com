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
    public class AddReactionResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public Reaction Reaction { get; set; }
    }
}