﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Database record for a user account
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string AppId { get; set; }
        public string AboutMe { get; set; }
        public DateTime? DateJoined { get; set; }
        public DateTime? DateLastActivity { get; set; }

        public Int64 Reputation { get; set; }

        public string PGPPubKey { get; set; }

        public bool IsOnline { get; set; }

        // Comma-separated list of language codes. e.g.: en,es,it,fr
        public string Languages { get; set; }

        //Earnings including direct, group, community
        public double TotalEarned { get; set; }
        public virtual ICollection<EarningEvent> EarningEvents { get; set; }
        public virtual ICollection<SpendingEvent> SpendingEvents { get; set; }

        public virtual UserSettings Settings { get; set; }
        public virtual UserFunds Funds { get; set; }
        public virtual UserImage ThumbImage { get; set; }
        public virtual UserImage ProfileImage { get; set; }

        [InverseProperty("IgnoredByUsers")]
        public virtual ICollection<User> IgnoringUsers { get; set; }

        [InverseProperty("IgnoringUsers")]
        public virtual ICollection<User> IgnoredByUsers { get; set; }

        public virtual ICollection<Post> PostVotesUp { get; set; }
        public virtual ICollection<Post> PostVotesDown { get; set; }
        public virtual ICollection<Comment> CommentVotesUp { get; set; }
        public virtual ICollection<Comment> CommentVotesDown { get; set; }

        [InverseProperty("Following")]
        public virtual ICollection<User> Followers { get; set; }

        [InverseProperty("Followers")]
        public virtual ICollection<User> Following { get; set; }

        [InverseProperty("Members")]
        public virtual ICollection<Group> Groups { get; set; }

        [InverseProperty("Ignoring")]
        public virtual ICollection<Group> IgnoredGroups { get; set; }

        [InverseProperty("Moderators")]
        public virtual ICollection<Group> GroupModeration { get; set; }

        [InverseProperty("Administrators")]
        public virtual ICollection<Group> GroupAdministration { get; set; }

        public ICollection<LNTransaction> LNTransactions { get; set; }

        [InverseProperty("To")]
        public virtual ICollection<UserMessage> Messages { get; set; }

        [InverseProperty("To")]
        public virtual ICollection<UserAlert> Alerts { get; set; }

        public static implicit operator User(string v)
        {
            throw new NotImplementedException();
        }
    }
}