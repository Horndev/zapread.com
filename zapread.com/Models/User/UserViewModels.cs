using System.Collections.Generic;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    public class UserViewModel
    {
        public AboutMeViewModel AboutMe { get; set; }

        public int NumPosts { get; set; }
        public int NumFollowing { get; set; }
        public int NumFollowers { get; set; }

        public bool IsFollowing { get; set; }
        public bool IsIgnoring { get; set; }

        public User User { get; set; }

        public List<PostViewModel> ActivityPosts { get; set; }

        public List<User> TopFollowing { get; set; }

        public List<User> TopFollowers { get; set; }

        public double UserBalance { get; set; }

        public ManageUserGroupsViewModel UserGroups { get; set; }
    }
}