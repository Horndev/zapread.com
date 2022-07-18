using Microsoft.Extensions.Configuration;
using Square;
using Square.Exceptions;
using Square.Models;
using Square.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models.Subscription;

namespace zapread.com.Services
{
    /// <summary>
    /// Implementation of interface for Square POS
    /// [TODO] abstract for other POS
    /// </summary>
    public class PointOfSaleService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="useTest"></param>
        public void CreateSubscription(bool useTest = false)
        {
            ISquareClient client = GetSquareClient(useTest);

            var body = new CreateCustomerRequest.Builder()
                .EmailAddress("steven.horn.mail+sqtest@gmail.com")
                .GivenName("Zelgada")
                .Build();

            var response = client.CustomersApi.CreateCustomer(body);

            var customerId = response.Customer.Id;

            // ensure attribute exists
            try
            {
                var attrdef = client.CustomerCustomAttributesApi.ListCustomerCustomAttributeDefinitions();

                if (attrdef.CustomAttributeDefinitions == null || !attrdef.CustomAttributeDefinitions.Select(a => a.Key == "zrappid").Any())
                {
                    var bodyCustomAttributeDefinition = new CustomAttributeDefinition.Builder()
                        .Key("zrappid")
                        .Name("Zapread APPID")
                        .Schema(JsonObject.FromJsonString("{\"$ref\":\"https://developer-production-s.squarecdn.com/schemas/v1/common.json#squareup.common.String\"}"))
                        .Description("The favorite movie of the customer.")
                        .Visibility("VISIBILITY_READ_WRITE_VALUES")
                        .Build();

                    var cabody = new CreateCustomerCustomAttributeDefinitionRequest.Builder(bodyCustomAttributeDefinition)
                                    .Build();

                    var caresponse = client.CustomerCustomAttributesApi.CreateCustomerCustomAttributeDefinition(cabody);
                }
            }
            catch (ApiException e)
            {
                Console.WriteLine(e.ToString());
            }

            // add zrid

            var bodyCustomAttribute = new CustomAttribute.Builder()
                .MValue(JsonValue.FromObject("64c17656-22a4-4a92-b11a-607b858beb0b"))
                .Build();

            var bodyAttribute = new UpsertCustomerCustomAttributeRequest.Builder(bodyCustomAttribute)
                .Build();

            var attrResponse = client.CustomerCustomAttributesApi.UpsertCustomerCustomAttribute(customerId, "zrappid", bodyAttribute);
        }

        public void AddCard(string customerId = null, bool useTest = false)
        {
            ISquareClient client = GetSquareClient(useTest);

            var userAppId = "64c17656-22a4-4a92-b11a-607b858beb0b";

            var bodyCardBillingAddress = new Address.Builder()
                .AddressLine1("500 Electric Ave")
                .AddressLine2("Suite 600")
                .Locality("New York")
                .AdministrativeDistrictLevel1("NY")
                .PostalCode("10003")
                .Country("US")
                .Build();
            var bodyCard = new Card.Builder()
                .CardholderName("Amelia Earhart")
                .BillingAddress(bodyCardBillingAddress)
                .CustomerId(customerId)
                .ReferenceId(userAppId)
                .ExpMonth(1)
                .ExpYear(2025)
                .Build();
            var body = new CreateCardRequest.Builder(
                    idempotencyKey: Guid.NewGuid().ToString(),
                    sourceId:"cnon:",
                    card: bodyCard)
                .Build();

            var response = client.CardsApi.CreateCard(body);
        }

        /// <summary>
        /// Gets the customers in Square, ensures that their zrappid is in database
        /// </summary>
        public void UpdateCustomers(bool useTest = false)
        {
            ISquareClient client = GetSquareClient(useTest);

            using (var db = new ZapContext())
            {
                try
                {
                    var customers = client.CustomersApi.ListCustomers();

                    // https://developer.squareup.com/docs/sdks/dotnet/common-square-api-patterns#pagination
                    do
                    {
                        foreach (var customer in customers.Customers)
                        {
                            var customerId = customer.Id;

                            var dbCustomer = db.Customers
                                .Where(c => c.CustomerId == customerId)
                                .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                                .FirstOrDefault();

                            if (dbCustomer == null)
                            {
                                // get zrappid
                                var customerAppId = client.CustomerCustomAttributesApi.RetrieveCustomerCustomAttribute(customerId, "zrappid");

                                if (customerAppId.Errors != null) { continue; }

                                var userAppId = Convert.ToString(customerAppId.CustomAttribute.MValue.GetStoredObject());

                                var user = db.Users
                                    .Where(u => u.AppId == userAppId)
                                    .FirstOrDefault();

                                if (user == null) { continue; }

                                var newCustomer = new zapread.com.Models.Database.Financial.Customer()
                                {
                                    Id = Guid.NewGuid(),
                                    CustomerId = customerId,
                                    User = user,
                                    Provider = (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction),
                                };

                                db.Customers.Add(newCustomer);

                                db.SaveChanges();
                            }
                        }
                        customers = client.CustomersApi.ListCustomers(cursor: customers.Cursor);
                    }
                    while (customers.Cursor != null);
                }
                catch (ApiException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        /// <summary>
        /// This method checks:
        /// 1 - All available subscription plans
        ///     - Synchronize to DB
        /// </summary>
        public void UpdateSubscriptionPlans(bool useTest = false)
        {
            ISquareClient client = GetSquareClient(useTest);

            var subscriptionPlans = client.CatalogApi.ListCatalog(types: "SUBSCRIPTION_PLAN");

            using (var db = new ZapContext())
            {
                foreach (var sp in subscriptionPlans.Objects)
                {
                    var planId = sp.Id;

                    // Don't update if disabled
                    if (isDisabled(sp))
                    {
                        // Check if it's in DB
                        var plan = db.SubscriptionPlans
                            .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                            .Where(p => p.PlanId == planId)
                            .FirstOrDefault();

                        if (plan != null)
                        {
                            // Update the plan to disabled
                            plan.IsDisabled = true;
                            db.SaveChanges();
                        }
                        continue;
                    };

                    var isInDB = db.SubscriptionPlans
                        .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                        .Where(p => !p.IsDisabled)
                        .Any(p => p.PlanId == planId);

                    if (!isInDB)
                    {
                        var plan = new Models.Database.Financial.SubscriptionPlan()
                        {
                            Id = Guid.NewGuid(),
                            PlanId = planId,
                            IsDisabled = false,
                            Cadence = sp.SubscriptionPlanData.Phases[0].Cadence,
                            Currency = sp.SubscriptionPlanData.Phases[0].RecurringPriceMoney.Currency,
                            Name = sp.SubscriptionPlanData.Name,
                            Price = sp.SubscriptionPlanData.Phases[0].RecurringPriceMoney.Amount.Value,
                            Provider = useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction
                        };

                        db.SubscriptionPlans.Add(plan);
                        db.SaveChanges();
                    }
                    else
                    {
                        // It's in databse - synchronize
                        var plan = db.SubscriptionPlans
                            .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                            .Where(p => p.PlanId == planId)
                            .Where(p => !p.IsDisabled)
                            .FirstOrDefault();

                        if (plan == null) continue;

                        plan.IsDisabled = isDisabled(sp);
                        plan.Cadence = sp.SubscriptionPlanData.Phases[0].Cadence;
                        plan.Currency = sp.SubscriptionPlanData.Phases[0].RecurringPriceMoney.Currency;
                        plan.Name = sp.SubscriptionPlanData.Name;
                        plan.Price = sp.SubscriptionPlanData.Phases[0].RecurringPriceMoney.Amount.Value;

                        db.SaveChanges();
                    }
                }
                // subscriptionPlans.Objects[0].PresentAtAllLocations; // if false, plan is disabled
                // subscriptionPlans.Objects[0].Id;  // This is the ID
                // subscriptionPlans.Objects[0].SubscriptionPlanData.Name // This is the name
                // subscriptionPlans.Objects[0].SubscriptionPlanData.Phases[0].Cadence  // == "MONTHLY"
                // subscriptionPlans.Objects[0].SubscriptionPlanData.Phases[0].RecurringPriceMoney.Amount //  == 500
                // subscriptionPlans.Objects[0].SubscriptionPlanData.Phases[0].RecurringPriceMoney.Currency // == "CAD"
            }
        }

        private static bool isDisabled(CatalogObject sp)
        {
            return sp.PresentAtAllLocations.HasValue && !sp.PresentAtAllLocations.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetSubscriptions(bool useTest = false)
        {
            ISquareClient client = GetSquareClient(useTest);

            //var bodyQueryFilterSourceNames = new List<string>();
            //bodyQueryFilterSourceNames.Add("My App");

            var body = new SearchSubscriptionsRequest.Builder()
                            //.Query(bodyQuery)
                            .Build();

            using (var db = new ZapContext())
            {
                try
                {
                    var response = client.SubscriptionsApi.SearchSubscriptions(body);

                    do
                    {
                        foreach (var sub in response.Subscriptions)
                        {
                            var dbSubscription = db.Subscriptions
                                .Where(s => s.SubscriptionId == sub.Id)
                                .FirstOrDefault();

                            if (dbSubscription == null)
                            {
                                // Possible new subscription

                                var customerId = sub.CustomerId;

                                //var customerAppId = client.CustomerCustomAttributesApi.RetrieveCustomerCustomAttribute(customerId, "zrappid");

                                // Check if customerId is known to db
                                var dbCustomer = db.Customers
                                    .Where(c => c.CustomerId == customerId)
                                    .Include(c => c.User)
                                    .FirstOrDefault();

                                if (dbCustomer == null) { continue; }

                                var planId = sub.PlanId;

                                var subscriptionPlan = db.SubscriptionPlans
                                    .Where(c => c.PlanId == planId)
                                    .FirstOrDefault();

                                if (subscriptionPlan == null) { continue; }

                                var startDate = sub.StartDate;

                                //var stopDate = DateTime.Parse(sub.CanceledDate);

                                var newSubscription = new Models.Database.Financial.Subscription()
                                {
                                    Id = Guid.NewGuid(),
                                    SubscriptionId = sub.Id,
                                    User = dbCustomer.User,
                                    Provider = dbCustomer.Provider,
                                    LastChecked = DateTime.UtcNow,
                                    Plan = subscriptionPlan,
                                    ActiveDate = DateTime.Parse(startDate),
                                    IsActive = sub.Status == "ACTIVE",
                                    Payments = new List<Models.Database.Financial.SubscriptionPayment>(),
                                };

                                db.Subscriptions.Add(newSubscription);
                                db.SaveChanges();

                                // get payments
                                foreach (var invoiceId in sub.InvoiceIds)
                                {
                                    var invoice = client.InvoicesApi.GetInvoice(invoiceId);
                                    if (invoice == null) { continue; }

                                    var dbInvoice = db.SubscriptionPayments
                                        .Where(p => p.InvoiceId == invoiceId)
                                        .FirstOrDefault();

                                    if (dbInvoice == null)
                                    {
                                        var payment = new Models.Database.Financial.SubscriptionPayment()
                                        {
                                            Id = Guid.NewGuid(),
                                            IsPaid = invoice.Invoice.Status == "PAID",
                                            Subscription = newSubscription,
                                            Timestamp = DateTime.Parse(invoice.Invoice.CreatedAt),
                                            ReceiptUrl = invoice.Invoice.PublicUrl,
                                            InvoiceId = invoiceId,
                                            BalanceAwarded = 0,
                                            BTCPrice = 0
                                        };
                                        db.SubscriptionPayments.Add(payment);
                                    }
                                    else
                                    {
                                        // Update
                                        dbInvoice.IsPaid = invoice.Invoice.Status == "PAID";
                                    }
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                // Sync?
                            }
                        }

                        body = new SearchSubscriptionsRequest.Builder()
                            .Cursor(response.Cursor)
                            //.Query(bodyQuery)
                            .Build();

                        response = client.SubscriptionsApi.SearchSubscriptions(body);
                    }
                    while (response.Cursor != null);
                }
                catch (ApiException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private static ISquareClient GetSquareClient(bool useTest = false)
        {
            ISquareClient client;
            var accessToken = useTest ? 
                System.Configuration.ConfigurationManager.AppSettings["SquareSandboxAccessToken"] 
                : System.Configuration.ConfigurationManager.AppSettings["SquareProductionAccessToken"];

            client = new SquareClient.Builder()
                .Environment(useTest ? Square.Environment.Sandbox : Square.Environment.Production)
                .AccessToken(accessToken)
                .Build();

            return client;
        }
    }
}