using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models.Subscription;

namespace zapread.com.Services
{
    /// <summary>
    /// Jobs to manage subscription accounts
    /// </summary>
    public class SubscriptionAccountsService
    {
        /// <summary>
        /// 
        /// </summary>
        public void CheckSubscriptionsSupporter(bool useTest = false)
        {
            using (var db = new ZapContext())
            {
                var subs = db.SubscriptionPlans
                    .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                    .Where(p => p.Name == "Supporter")
                    .SelectMany(p => p.Subscriptions)
                    .Where(s => s.IsActive)
                    .Where(s => !s.IsEnding)
                    .Where(s => s.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                    .ToList();
            }
        }
    }
}