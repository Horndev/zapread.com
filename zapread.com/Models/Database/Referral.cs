using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Data for referral program
    /// </summary>
    public class Referral
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Who referred the user
        /// </summary>
        public string ReferredByAppId { get; set; }

        /// <summary>
        /// When the user was referred
        /// </summary>
        public DateTime? TimeStamp { get; set; }
    }
}