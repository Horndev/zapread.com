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
    public class PostReaction
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int PostReactionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("PostReactions")]
        public virtual ICollection<Post> Posts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Reaction Reaction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public User User { get; set; }
    }
}