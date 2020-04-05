using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    public class UserProcess
    {
        [Key]
        public int Id { get; set; }

        public string JobId { get; set; }

        public string JobName { get; set; }

        /// <summary>
        /// The user who owns this process.
        /// </summary>
        [InverseProperty("UserProcesses")]
        public virtual User User { get; set; }
    }
}