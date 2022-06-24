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
    public class Subscription
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 0 = Square
        /// </summary>
        public int Provider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Subscriptions")]
        public SubscriptionPlan Plan { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Subscription")]
        public virtual ICollection<SubscriptionPayment> Payments { get; set; }

        /// <summary>
        /// Zapread User with this subscription
        /// </summary>
        [InverseProperty("Subscriptions")]
        public User User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ActiveDate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastChecked { get; set; }
    }
}