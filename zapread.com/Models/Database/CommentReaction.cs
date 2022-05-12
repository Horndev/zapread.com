using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class CommentReaction
    {
        /// <summary>
        /// 
        /// </summary>
        public int CommentReactionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("CommentReactions")]
        public virtual ICollection<Comment> Comments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Reaction Reaction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp { get; set; }
    }
}