using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account
{
    /// <summary>
    /// 
    /// </summary>
    public class SubscribeRequest
    {
        /// <summary>
        /// Credit Card payment token
        /// </summary>
        public string CardToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VerificationToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PlanId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LastName { get; set; }
    }
}