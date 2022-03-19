using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Database record for a post
    /// </summary>
    public class Post
    {
        [Key]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int PostId { get; set; }

        public int Score { get; set; }
        public long Impressions { get; set; }                           // Number of times post was rendered
        public string PostTitle { get; set; }
        public DateTime? TimeStamp { get; set; }
        public DateTime? TimeStampEdited { get; set; }                  // When the post was last edited, or null
        public string Content { get; set; }
        public double TotalEarned { get; set; }

        // User who made post
        [InverseProperty("Posts")]
        public virtual User UserId { get; set; }

        /// <summary>
        /// Not used
        /// </summary>
        [InverseProperty("HiddenPosts")]
        public virtual User HiddenBy { get; set; }

        [InverseProperty("FollowingPosts")]
        public virtual ICollection<User> FollowedByUsers { get; set; }

        [InverseProperty("IgnoringPosts")]
        public virtual ICollection<User> IgnoredByUsers { get; set; }

        // Language post is written in
        public string Language { get; set; }

        [InverseProperty("Posts")]
        public virtual Group Group { get; set; }

        [InverseProperty("PostVotesUp")]
        public virtual ICollection<User> VotesUp { get; set; }
        [InverseProperty("PostVotesDown")]
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}