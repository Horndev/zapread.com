using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Tracking hangfire jobs
    /// </summary>
    public class UserProcess
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// The user who owns this process.
        /// </summary>
        [InverseProperty("UserProcesses")]
        public virtual User User { get; set; }
    }
}