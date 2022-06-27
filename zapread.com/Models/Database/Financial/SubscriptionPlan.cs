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
    public class SubscriptionPlan
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
        public string PlanId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Cadence { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Plan")]
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}