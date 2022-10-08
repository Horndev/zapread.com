using System;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AuditUserViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Username { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Stat
    {
        /// <summary>
        /// 
        /// </summary>
        public Int64 TimeStampUtc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DateGroupType
    {
        /// <summary>
        /// 
        /// </summary>
        Day,
        /// <summary>
        /// 
        /// </summary>
        Week,
        /// <summary>
        /// 
        /// </summary>
        Month,
        /// <summary>
        /// 
        /// </summary>
        Quarter,
        /// <summary>
        /// 
        /// </summary>
        Year
    }
}