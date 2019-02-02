using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Database record for a post
    /// </summary>
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        public int Score { get; set; }
        public string PostTitle { get; set; }
        public DateTime? TimeStamp { get; set; }
        public DateTime? TimeStampEdited { get; set; }                  // When the post was last edited, or null
        public string Content { get; set; }
        public double TotalEarned { get; set; }

        // User who made post
        [InverseProperty("Posts")]
        public virtual User UserId { get; set; }

        // Language post is written in
        public string Language { get; set; }

        [InverseProperty("Posts")]
        public virtual Group Group { get; set; }
        public virtual ICollection<User> VotesUp { get; set; }
        public virtual ICollection<User> VotesDown { get; set; }

        [InverseProperty("Post")]
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<UserImage> Images { get; set; }

        // Post flags
        public bool IsDeleted { get; set; }
        public bool IsSticky { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsDraft { get; set; }
        public bool IsPublished { get; set; }
    }
}