using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Database record for a community group
    /// </summary>
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        public string GroupName { get; set; }

        // When the group was created
        public DateTime? CreationDate { get; set; }

        [InverseProperty("Groups")]
        public virtual ICollection<User> Members { get; set; }

        [InverseProperty("IgnoredGroups")]
        public virtual ICollection<User> Ignoring { get; set; }

        [InverseProperty("GroupModeration")]
        public virtual ICollection<User> Moderators { get; set; }

        [InverseProperty("GroupAdministration")]
        public virtual ICollection<User> Administrators { get; set; }

        [InverseProperty("Group")]
        public virtual ICollection<Post> Posts { get; set; }

        // These earnings need to go out
        public double TotalEarnedToDistribute { get; set; }

        public double TotalEarned { get; set; }

        public int Tier { get; set; }

        public string Tags { get; set; }

        public string Icon { get; set; }

        // Unlockable description of group - set by admin
        public string ShortDescription { get; set; }

        // Posts in this group are in this language by default
        public string DefaultLanguage { get; set; }
    }
}