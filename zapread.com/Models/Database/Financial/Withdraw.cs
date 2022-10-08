using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database.Financial
{
    /// <summary>
    ///
    /// </summary>
    public class Withdraw
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Withdraws")]
        public User User { get; set; }

        /// <summary>
        /// The lightning invoice itself
        /// </summary>
        public string Invoice { get; set; }

        /// <summary>
        /// The amount of the invoice in Satoshi
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// When the user agreed to the withdraw
        /// </summary>
        public DateTime? ValidationTimestamp { get; set; }
    }
}