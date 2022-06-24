using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class UserContentReport
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("UserReports")]
        public Post Post { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("UserReports")]
        public Comment Comment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("UserReports")]
        public User ReportedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public User ResolvedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsResolved { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// 0 = Other
        /// 1 = Spam
        /// 2 = NSFW
        /// </summary>
        public int ReportType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ResolveDate { get; set; }
    }
}