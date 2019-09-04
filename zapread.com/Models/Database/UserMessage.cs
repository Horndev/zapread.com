using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace zapread.com.Models.Database
{
    public class UserMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public string Title { get; set; }

        [InverseProperty("Messages")]
        public User To { get; set; }

        public User From { get; set; }
        public Post PostLink { get; set; }
        public Comment CommentLink { get; set; }

        public DateTime? TimeStamp { get; set; }

        public bool IsPrivateMessage { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }
}