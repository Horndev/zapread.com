using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class GetUserInfoResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserProfileImageVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long Reputation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AboutMe { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<UserAchievementViewModel> Achievements { get; set; }
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
    }
}