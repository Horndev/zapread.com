using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
using System.Threading.Tasks;
using System.Web;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Models.Subscription;

namespace zapread.com.Services
{
    /// <summary>
    /// Implementation of interface for Square POS
    /// [TODO] abstract for other POS
    /// </summary>
    public class PointOfSaleService : IPointOfSaleService
    {
        private async Task<string> getOrCreateCustomerId(ISquareClient client, bool useTest, string userAppId, string customerEmail)
        {
            using (var db = new ZapContext())
            {
                var customerId = await db.Customers
                    .Where(c => c.User.AppId == userAppId)
                    .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                    .Select(c => c.CustomerId)
                    .FirstOrDefaultAsync();

                if (customerId == null)
                {
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        var applicationUser = userManager.FindById(userAppId);

                        var receiptEmail = "";

                        if (string.IsNullOrEmpty(customerEmail))
                        {
                            if (applicationUser != null && applicationUser.EmailConfirmed)
                            {
                                receiptEmail = applicationUser.Email;
                            }
                        }
                        else
                        {
                            receiptEmail = customerEmail;
                        }

                        customerId = createCustomer(
                            client: client,
                            emailAddress: receiptEmail,
                            name: applicationUser.UserName,
                            appId: userAppId);
                    }
                    var user = db.Users
                        .Where(u => u.AppId == userAppId)
                        .FirstOrDefault();

                    if (user == null) { return null; }

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
                return customerId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppId"></param>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public async Task<bool> Unsubscribe(string userAppId, string subscriptionId)
        {
            bool useTest = System.Configuration.ConfigurationManager.AppSettings["SquareEnvironment"] == "Sandbox";

            ISquareClient client = GetSquareClient(useTest);

            var pauseSubscriptionBody = new PauseSubscriptionRequest.Builder()
                .PauseReason("User request") // TODO - we can collect more specifics
                .Build();

            var result = await client.SubscriptionsApi.PauseSubscriptionAsync(subscriptionId, pauseSubscriptionBody);
                //.CancelSubscriptionAsync(subscriptionId);

            if (result.Errors != null) { return false; }

            using (var db = new ZapContext())
            {
                var dbSub = await db.Subscriptions
                    .Where(s => s.SubscriptionId == subscriptionId)
                    .FirstOrDefaultAsync();

                if (dbSub == null) { return false; }

                dbSub.IsEnding = true;
                
                if (result.Actions[0].Type == "PAUSE")
                {
                    dbSub.PauseActionId = result.Actions[0].Id;
                    dbSub.PauseDate = DateTime.Parse(result.Actions[0].EffectiveDate);
                }

                await db.SaveChangesAsync();

                return await Task.FromResult<bool>(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppId"></param>
        /// <param name="token"></param>
        /// <param name="verificationToken"></param>
        /// <param name="planId"></param>
        /// <param name="customerEmail"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public async Task<bool> Subscribe(string userAppId, string token, string verificationToken, string planId, string customerEmail, string firstName, string lastName)
        {
            bool useTest = System.Configuration.ConfigurationManager.AppSettings["SquareEnvironment"] == "Sandbox";

            ISquareClient client = GetSquareClient(useTest);

            // Ensure customer exists for new subscriptions
            var customerId = await getOrCreateCustomerId(
                client,
                useTest, 
                userAppId,
                customerEmail);

            // Check if customer is already subscribed to this plan now or in the past with a valid payment method.
            using (var db = new ZapContext())
            {
                // Check if user already has a subscription to this plan.
                var userSub = await db.Subscriptions
                    .Where(s => s.Plan.PlanId == planId)
                    .Where(s => s.User.AppId == userAppId)
                    .Where(s => s.IsActive == false || (s.IsActive == true && s.IsEnding))
                    .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                    .FirstOrDefaultAsync();

                if (userSub != null)
                {
                    try
                    {
                        if (userSub.IsEnding 
                            && !string.IsNullOrEmpty(userSub.PauseActionId) 
                            && userSub.PauseDate.HasValue 
                            && userSub.PauseDate.Value > DateTime.Now)
                        {
                            // Not yet cancelled - let's remove the scheduled cancel.
                            var result = await client.SubscriptionsApi.DeleteSubscriptionActionAsync(userSub.SubscriptionId, userSub.PauseActionId);
                        }
                        else
                        {
                            // It was cancelled and expired some time ago, start it again
                            var resumeSubscriptionBody = new ResumeSubscriptionRequest.Builder()
                            //.ResumeEffectiveDate(resumeDateStr) // YYYY-MM-DD
                            .ResumeChangeTiming("IMMEDIATE")
                            .Build();

                            var result = await client.SubscriptionsApi.ResumeSubscriptionAsync(userSub.SubscriptionId, resumeSubscriptionBody);
                        }
                        //var resumeDateStr = DateTime.Now.ToString("yyyy-MM-dd");

                        // Resume with default parameters

                        userSub.IsEnding = false;
                        userSub.IsActive = true;
                        await db.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }

            var cardId = AddCard(
                client, 
                customerId, 
                token, 
                verificationToken, 
                firstName, 
                lastName, 
                userAppId);

            // subscribe...  https://developer.squareup.com/docs/subscriptions-api/walkthrough#subscriptions-walkthrough-create=customer
            string subscriptionId = await SubscribeCustomer(planId, useTest, client, customerId, cardId, userAppId);

            return await Task.FromResult<bool>(true);
        }

        private static async Task<string> SubscribeCustomer(string planId, bool useTest, ISquareClient client, string customerId, string cardId, string userAppId)
        {
            //var bodyPriceOverrideMoney = new Money.Builder()
            //    .Amount(100L)
            //    .Currency("USD")
            //    .Build();

            var bodySource = new SubscriptionSource.Builder()
                .Name("Zapread.com")
                .Build();

            var locationId = useTest ?
                System.Configuration.ConfigurationManager.AppSettings["SquareZRSandboxLocationId"]
                : System.Configuration.ConfigurationManager.AppSettings["SquareZRProductionLocationId"];

            var body = new CreateSubscriptionRequest.Builder(
                    locationId: locationId,
                    planId: planId,
                    customerId: customerId)
                .IdempotencyKey(Guid.NewGuid().ToString())
                //.StartDate("2021-10-20")
                //.TaxPercentage("5")
                //.PriceOverrideMoney(bodyPriceOverrideMoney)
                .CardId(cardId)
                //.Timezone("America/Los_Angeles")
                .Source(bodySource)
                .Build();

            var subscriptionId = "";

            CreateSubscriptionResponse result = null;

            try
            {
                result = await client.SubscriptionsApi.CreateSubscriptionAsync(body);

                // add subscription to db
                subscriptionId = result.Subscription.Id;
            }
            catch (ApiException e) { };

            // Sync and save to local database
            using (var db = new ZapContext())
            {
                var dbPlan = await db.SubscriptionPlans
                    .Where(p => p.PlanId == planId)
                    .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                    .FirstOrDefaultAsync();

                var dbUser = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefaultAsync();

                // remove all existing subscriptions
                var subs = await db.Subscriptions
                    .Where(p => p.Provider == (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction))
                    .Where(s => s.User.AppId == userAppId)
                    .ToListAsync();

                foreach (var s in subs)
                {
                    // Remove the old subscriptions
                    try
                    {
                        var cancelResult = await client.SubscriptionsApi.CancelSubscriptionAsync(s.SubscriptionId);
                        db.Subscriptions.Remove(s);
                    }
                    catch 
                    { 

                    };
                }

                var newSubscription = new zapread.com.Models.Database.Financial.Subscription()
                {
                    Id = Guid.NewGuid(),
                    Plan = dbPlan,
                    User = dbUser,
                    Provider = (useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction),
                    ActiveDate = DateTime.Now,
                    EndDate = result == null ? DateTime.Now + TimeSpan.FromDays(31) :DateTime.Parse(result.Subscription.ChargedThroughDate),
                    IsActive = true,
                    IsEnding = false,
                    SubscriptionId = subscriptionId
                };

                db.Subscriptions.Add(newSubscription);

                await db.SaveChangesAsync();

                return subscriptionId;
            }
        }

        private string createCustomer(ISquareClient client, string emailAddress, string name, string appId)
        {
            var bodyBuilder = new CreateCustomerRequest.Builder();

            if (!string.IsNullOrEmpty(emailAddress)) bodyBuilder = bodyBuilder.EmailAddress(emailAddress);

            var body = bodyBuilder.GivenName(name).Build();

            var response = client.CustomersApi.CreateCustomer(body);

            var customerId = response.Customer.Id;

            var bodyCustomAttribute = new CustomAttribute.Builder()
                .MValue(JsonValue.FromObject(appId))
                .Build();

            var bodyAttribute = new UpsertCustomerCustomAttributeRequest.Builder(bodyCustomAttribute)
                .Build();

            var attrResponse = client.CustomerCustomAttributesApi.UpsertCustomerCustomAttribute(customerId, "zrappid", bodyAttribute);

            return customerId;
        }

        /// <summary>
        /// Creates a custom attribute which can be attached to customers
        /// </summary>
        /// <param name="useTest"></param>
        public void CreateZRCustomerAttribute(bool useTest = false)
        {
            ISquareClient client = GetSquareClient(useTest);

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
                        .Description("User Application ID in ZR Database")
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useTest"></param>
        public void CreateSubscription(bool useTest = false)
        {
            ISquareClient client = GetSquareClient(useTest);

            CreateZRCustomerAttribute(useTest);

            var customerId = createCustomer(
                client: client,
                emailAddress: "steven.horn.mail+sqtest@gmail.com",
                name: "Zelgada",
                appId: "64c17656-22a4-4a92-b11a-607b858beb0b");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="customerId"></param>
        /// <param name="token"></param>
        /// <param name="verificationToken"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="userAppId"></param>
        public string AddCard(ISquareClient client, string customerId, string token, string verificationToken, string firstName, string lastName, string userAppId)
        {
            //var userAppId = "64c17656-22a4-4a92-b11a-607b858beb0b";

            Address bodyCardBillingAddress = null;
            if (client.Environment == Square.Environment.Sandbox)
            {
                bodyCardBillingAddress = new Address.Builder()
                //.AddressLine1("500 Electric Ave")
                //.AddressLine2("Suite 600")
                //.Locality("New York")
                //.AdministrativeDistrictLevel1("NY")
                //.PostalCode("10003")
                //.Country("US")
                .Build();
            }
            else
            {
                // Should not be required?
                bodyCardBillingAddress = new Address.Builder().Build();
            }

            var bodyCard = new Card.Builder()
                .CardholderName(firstName + " " + lastName)
                .BillingAddress(bodyCardBillingAddress)
                .CustomerId(customerId)
                .ReferenceId(userAppId) //An optional user-defined reference ID that associates this card with another entity in an external system.
                .Build();

            // DEBUG - sandbox
            if (client.Environment == Square.Environment.Sandbox)
                token = "cnon:card-nonce-ok";

            var bodyBuilder = new CreateCardRequest.Builder(
                    idempotencyKey: Guid.NewGuid().ToString(),
                    sourceId: token,
                    card: bodyCard);

            // verified Buyer - requred for EU/UK
            //if (!string.IsNullOrEmpty(verificationToken)) bodyBuilder = bodyBuilder.VerificationToken(verificationToken);

            var body = bodyBuilder.Build();
            try
            {
                var response = client.CardsApi.CreateCard(body);
                return response.Card.Id;
            }
            catch (Exception e)
            {
                ;
                throw e;
            }
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
                            Provider = useTest ? POSProviderTypes.SquareSandbox : POSProviderTypes.SquareProduction,
                            DescriptionHTML = sp.SubscriptionPlanData.Name + " Description",
                            Subtitle = ""
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