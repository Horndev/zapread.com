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
        [Key]
        public Int64 CommentId { get; set; }

        [InverseProperty("Comments")]
        public Post Post { get; set; }

        [InverseProperty("Replies")]
        public Comment Parent { get; set; }

        [InverseProperty("Parent")]
        public virtual ICollection<Comment> Replies { get; set; }

        public virtual User UserId { get; set; }
        public DateTime? TimeStamp { get; set; }
        public DateTime? TimeStampEdited { get; set; }

        public string Text { get; set; }

        public int Score { get; set; }

        [InverseProperty("CommentVotesUp")]
        public virtual ICollection<User> VotesUp { get; set; }
        [InverseProperty("CommentVotesDown")]
        public virtual ICollection<User> VotesDown { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsReply { get; set; }

        public double TotalEarned { get; set; }
    }
}