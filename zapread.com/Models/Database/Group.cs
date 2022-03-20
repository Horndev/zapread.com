using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Database record for a community group
    /// </summary>
    public class Group
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int GroupId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// // When the group was created
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Users who are members of this group
        /// </summary>
        [InverseProperty("Groups")]
        public virtual ICollection<User> Members { get; set; }
        
        /// <summary>
        /// Users who are ignoring this group and will not see posts from it
        /// </summary>
        [InverseProperty("IgnoredGroups")]
        public virtual ICollection<User> Ignoring { get; set; }
        
        /// <summary>
        /// Users with moderation privilages
        /// </summary>
        [InverseProperty("GroupModeration")]
        public virtual ICollection<User> Moderators { get; set; }
        
        /// <summary>
        /// Users with administration privilages
        /// </summary>
        [InverseProperty("GroupAdministration")]
        public virtual ICollection<User> Administrators { get; set; }

        /// <summary>
        /// Users with administration privilages
        /// </summary>
        [InverseProperty("Group")]
        public virtual ICollection<GroupBanished> Banished { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Group")]
        public virtual ICollection<Post> Posts { get; set; }

        /// <summary>
        /// // These earnings need to go out
        /// </summary>
        public double TotalEarnedToDistribute { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalEarned { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Tier { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Tags { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// // Posts in this group are in this language by default
        /// </summary>
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// Image which can be used instead of an icon to represent the group
        /// </summary>
        public UserImage GroupImage { get; set; }

        /// <summary>
        /// Group can add a background image over the header.
        /// </summary>
        public UserImage GroupHeaderImage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CustomTemplate { get; set; }
    }
}