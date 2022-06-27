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
    public class SubscriptionPayment
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Payments")]
        public Subscription Subscription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double BTCPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double BalanceAwarded { get; set; }
    }
}