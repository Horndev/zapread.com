using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    public class NewGroupViewModel
    {
        [Display(Name = "Name")]
        [Required]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "A valid group name 2 - 60 characters long is required.")]
        public string GroupName { get; set; }

        public string Icon { get; set; }
        public List<string> Icons { get; set; }

        [Display(Name = "Tags")]
        [StringLength(256, ErrorMessage = "Too many tags.  Maximum 256 characters allowed.")]
        public string Tags { get; set; }
    }

    public class GroupAdminTagsViewModel
    {
        public int GroupId { get; set; }
        [Display(Name = "Tags")]
        [StringLength(256, ErrorMessage = "Too many tags.  Maximum 256 characters allowed.")]
        public string Tags { get; set; }
    }

    public class GroupAdminIconsViewModel
    {
        public int GroupId { get; set; }
        public string Icon { get; set; }
        public List<string> Icons { get; set; }
    }

    public class GroupViewModel
    {
        public List<GroupInfo> SubscribedGroups;
        public List<Post> Posts { get; set; }
        public GroupInfo GroupInfo { get; set; }
        public List<int> Upvoted;
        public List<int> Downvoted;
        public Group Group { get; set; }
        public double UserBalance;

        public bool IsGroupAdmin { get; set; }
        public bool IsGroupMod { get; set; }

        [Display(Name = "Tags")]
        [StringLength(256, ErrorMessage = "Too many tags.  Maximum 256 characters allowed.")]
        public string Tags { get; set; }

        public bool HasMorePosts { get; set; }
    }

    public class GroupsViewModel
    {
        public List<GroupInfo> Groups { get; set; }
        public string TotalPosts { get; set; }
    }

    /// <summary>
    /// This is the view model for the group
    /// </summary>
    public class GroupInfo
    {
        public int Id { get; set; }
        public string CreatedddMMMYYYY { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public string Icon { get; set; }
        public int Level { get; set; }
        public int NumMembers { get; set; }
        public int NumPosts { get; set; }
        public int UserPosts { get; set; }
        public int Progress { get; set; }
        public bool IsMember { get; set; }
        public bool IsLoggedIn { get; set; }
        public List<User> Members { get; set; }
        public bool IsMod { get; set; }
        public bool IsAdmin { get; set; }
    }
}