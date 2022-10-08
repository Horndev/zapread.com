using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class StatisticsPoint
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumNewUsers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumNewPosts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumNewComments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumNewImages { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumPrivateMessages { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Deposited_Satoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Withdrawn_Satoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumVotes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumTips { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TotalTipped { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HourlyStatistics
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DailyStatistics
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WeeklyStatistics
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MonthlyStatistics
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }
}