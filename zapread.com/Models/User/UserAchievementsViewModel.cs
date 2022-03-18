using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UserAchievementsViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<UserAchievementViewModel> Achievements { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserAchievementViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ImageId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime DateAchieved { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
    }
}