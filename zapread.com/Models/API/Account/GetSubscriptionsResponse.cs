﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account
{
    /// <summary>
    /// 
    /// </summary>
    public class GetSubscriptionsResponse : ZapReadResponse
    {
        /// <summary>
        /// List of subscriptions
        /// </summary>
        public List<SubscriptionItem> Subscriptions { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public class SubscriptionItem
        {
            /// <summary>
            /// 
            /// </summary>
            public string Id { get; set; }

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
            public bool IsEnding { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Subtitle { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string DescriptionHTML { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Price { get; set; }

            /// <summary>
            /// D
            /// </summary>
            public bool IsSubscribed { get; set; }
        }
    }
}