using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    public class GroupAdminBarViewModel
    {
        public int GroupId { get; set; }
        public int Tier { get; set; }
    }

    public class NewGroupViewModel
    {
        [Display(Name = "Name")]
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "A valid group name 2 to 50 characters long is required.")]
        public string GroupName { get; set; }

        public string Icon { get; set; }
        public List<string> Icons { get; set; }

        [Display(Name = "Tags")]
        [StringLength(256, ErrorMessage = "Too many tags.  Maximum 256 characters allowed.")]
        public string Tags { get; set; }

        [Display(Name = "Language")]
        //[StringLength(2, ErrorMessage = "Error in language code.")]
        public string Language { get; set; }

        // Not displayed
        public List<string> Languages { get; set; }
    }

    public class GroupAdminTagsViewModel
    {
        public int GroupId { get; set; }
        [Display(Name = "Tags")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "Too many tags.  Maximum 256 characters allowed.")]
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
        public List<PostViewModel> Posts { get; set; }
        public GroupInfo GroupInfo { get; set; }
        public List<int> Upvoted;
        public List<int> Downvoted;
        public Group Group { get; set; }
        public double UserBalance;

        public bool IsGroupAdmin { get; set; }
        public bool IsGroupMod { get; set; }
        public bool IsIgnored { get; set; }

        [Display(Name = "Tags")]
        [StringLength(256, ErrorMessage = "Too many tags.  Maximum 256 characters allowed.")]
        public string Tags { get; set; }

        public bool HasMorePosts { get; set; }
    }

    /// <summary>
    /// Used for viewing a specific group member
    /// </summary>
    public class GroupMemberViewModel
    {
        public string UserName { get; set; }

        /// <summary>
        /// For thumbnail
        /// </summary>
        public string AppId { get; set; }

        public string AboutMe { get; set; }

        public bool IsSiteAdministrator { get; set; }

        public bool IsGroupAdministrator { get; set; }

        public bool IsModerator { get; set; }

        public bool IsOnline { get; set; }

        public DateTime? LastSeen { get; set; }
    }

    /// <summary>
    /// Used for viewing a set of group members
    /// </summary>
    public class GroupMembersViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Icon { get; set; }
        public double TotalEarned { get; set; }
        public List<GroupMemberViewModel> Members { get; set; }
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