using System;
using System.Collections.Generic;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public AboutMeViewModel AboutMe { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumPosts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumFollowing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumFollowers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFollowing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIgnoring { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
        public User User { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserAppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UserProfileImageVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DateJoined { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 Reputation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<PostViewModel> ActivityPosts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<UserFollowView> TopFollowingVm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<UserFollowView> TopFollowersVm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double UserBalance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ManageUserGroupsViewModel UserGroups { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public UserAchievementsViewModel AchievementsViewModel { get; set; }
    }
}