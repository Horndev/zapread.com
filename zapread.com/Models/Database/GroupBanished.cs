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
    public class GroupBanished
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int BanishedId { get; set; }

        /// <summary>
        /// Users with administration privilages
        /// </summary>
        [InverseProperty("GroupBanished")]
        public virtual User User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Banished")]
        public virtual Group Group { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int BanishmentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStampStarted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStampExpired { get; set; }
    }
}