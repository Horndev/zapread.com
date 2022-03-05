using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class Achievement
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// // An intrinsic assigned value to the achievement
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// // Navigation property
        /// </summary>
        [InverseProperty("Achievement")]
        public virtual ICollection<UserAchievement> Awarded { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserAchievement
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Achievements")]
        public User AchievedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Awarded")]
        public Achievement Achievement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? DateAchieved { get; set; }
    }
}