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
        public AboutMeViewModel AboutMe { get; set; }

        public int NumPosts { get; set; }
        public int NumFollowing { get; set; }
        public int NumFollowers { get; set; }

        public bool IsFollowing { get; set; }
        public bool IsIgnoring { get; set; }

        [Obsolete]
        public User User { get; set; }
        public string UserName { get; set; }

        public int UserId { get; set; }

        public string UserAppId { get; set; }

        public int UserProfileImageVersion { get; set; }
        public DateTime? DateJoined { get; set; }
        public Int64 Reputation { get; set; }

        public List<PostViewModel> ActivityPosts { get; set; }

        //[Obsolete]
        //public List<User> TopFollowing { get; set; }

        public IEnumerable<UserFollowView> TopFollowingVm { get; set; }

        //[Obsolete]
        //public List<User> TopFollowers { get; set; }

        public IEnumerable<UserFollowView> TopFollowersVm { get; set; }

        public double UserBalance { get; set; }

        public ManageUserGroupsViewModel UserGroups { get; set; }

        public UserAchievementsViewModel AchievementsViewModel { get; set; }
    }
}