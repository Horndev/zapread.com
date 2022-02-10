using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Subscription
{
    /// <summary>
    /// 
    /// </summary>
    public class UnsubscribeIndexViewModel
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool Success { get; set; }
        public string Name { get; set; }
        public string UnsubFunction { get; set; }
        public string UserEmail { get; set; }
        public string UserUnsubscribeId { get; set; }
        public string SubscriptionType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}