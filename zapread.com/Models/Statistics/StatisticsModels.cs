using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace zapread.com.Models
{
    public class StatisticsPoint
    {
        [Key]
        public int Id { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int NumNewUsers { get; set; }
        public int NumNewPosts { get; set; }
        public int NumNewComments { get; set; }
        public int NumNewImages { get; set; }
        public int NumPrivateMessages { get; set; }
        public int Deposited_Satoshi { get; set; }
        public int Withdrawn_Satoshi { get; set; }
        public int NumVotes { get; set; }
        public int NumTips { get; set; }
        public int TotalTipped { get; set; }
    }

    public class HourlyStatistics
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }

    public class DailyStatistics
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }

    public class WeeklyStatistics
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }

    public class MonthlyStatistics
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<StatisticsPoint> Data { get; set; }
    }
}