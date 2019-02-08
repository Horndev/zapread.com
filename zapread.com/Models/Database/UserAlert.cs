using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    public class UserAlert
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime? TimeStamp { get; set; }

        [InverseProperty("Alerts")]
        public User To { get; set; }

        public Post PostLink { get; set; }
        public Comment CommentLink { get; set; }

        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }
}