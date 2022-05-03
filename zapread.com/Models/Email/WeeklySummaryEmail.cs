using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class WeeklySummaryEmail : Postal.Email
    {
        /// <summary>
        /// 
        /// </summary>
        public string RefCode { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public double TotalEarnedWeek { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalEarnedLastWeek { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalEarnedReferral { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalEarnedPosts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalEarnedComments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalGroupPayments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<TopGroup> TopGroups { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalCommunityPayments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalPostsWeek { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalCommentsWeek { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TopGroup
    {
        /// <summary>
        /// 
        /// </summary>
        public int GroupId { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double AmountEarned { get; set; }
    }
}