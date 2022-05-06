using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
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
        /// Alternative - reaction as an image (custom)
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
    }
}