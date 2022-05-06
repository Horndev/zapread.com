using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Database record for a comment
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Int64 CommentId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Comments")]
        public Post Post { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Replies")]
        public Comment Parent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Parent")]
        public virtual ICollection<Comment> Replies { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual User UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStampEdited { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("CommentVotesUp")]
        public virtual ICollection<User> VotesUp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("CommentVotesDown")]
        public virtual ICollection<User> VotesDown { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Comments")]
        public virtual ICollection<CommentReaction> CommentReactions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsReply { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double TotalEarned { get; set; }
    }
}