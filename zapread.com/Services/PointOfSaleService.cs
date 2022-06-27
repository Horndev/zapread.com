using Microsoft.Extensions.Configuration;
using Square;
using Square.Exceptions;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using zapread.com.Database;

namespace zapread.com.Services
{
    /// <summary>
    /// Implementation of interface for Square POS
    /// [TODO] abstract for other POS
    /// </summary>
    public class PointOfSaleService
    {
        public void GetSubscriptions ()
        {
            ISquareClient client;
            var accessToken = System.Configuration.ConfigurationManager.AppSettings["SquareProductionAccessToken"];

            client = new SquareClient.Builder()
                .Environment(Square.Environment.Production)
                .AccessToken(accessToken)
                .Build();

            //var bodyQueryFilterSourceNames = new List<string>();
            //bodyQueryFilterSourceNames.Add("My App");

            var body = new SearchSubscriptionsRequest.Builder()
                            //.Query(bodyQuery)
                            .Build();
            
            using (var db = new ZapContext())
            {
                try
                {
                    var subs = client.SubscriptionsApi.SearchSubscriptions(body);

                    foreach (var sub in subs.Subscriptions)
                    {
                        var customerId = sub.CustomerId;

                        // Check if customerId is known to db
                        var isZapReadUser = db.Customers
                            .Where(c => c.CustomerId == customerId)
                            .Any();

                    }
                }
                catch (ApiException e)
                {
                    ;
                }
            ;
            }
        }
    }
}