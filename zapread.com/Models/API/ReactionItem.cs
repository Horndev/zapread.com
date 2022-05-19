using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API
{
    /// <summary>
    /// 
    /// </summary>
    public class ReactionItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int ReactionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ReactionName { get; set; }

        /// <summary>
        /// UTF-8 based emoji.  If null, use Image
        /// </summary>
        public string ReactionIcon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> UserNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NumReactions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsApplied { get; set; }
    }
}