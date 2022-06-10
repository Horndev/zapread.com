using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class BannerAlert
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User this belongs to
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// info, danger, warning
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// User will not see it again
        /// </summary>
        public bool IsDismissed { get; set; }

        /// <summary>
        /// If hidden until SnoozeTime
        /// </summary>
        public bool IsSnoozed { get; set; }

        /// <summary>
        /// Can user dismiss it?  If true, then it can't be dismissed
        /// </summary>
        public bool IsSticky { get; set; }

        /// <summary>
        /// When Snooze should end
        /// </summary>
        public DateTime? SnoozeTime { get; set; }

        /// <summary>
        /// Don't show unless after this time
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Delete if after this time
        /// </summary>
        public DateTime? DeleteTime { get; set; }

        /// <summary>
        /// When it was created
        /// </summary>
        public DateTime? TimeStamp { get; set; }
    }
}