using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Controllers;
using zapread.com.Helpers;
using zapread.com.Services;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class POSTest
    {
        [TestMethod]
        public void TestSubscriptions()
        {
            if (false) // DEBUG
            {
                var service = new PointOfSaleService();

                //service.GetSubscriptions();

                //service.CreateSubscription(useTest: true);
                //service.AddCard(customerId: "P88K5BGMFGZCBE8XVCFJJBP3CR", useTest: true);
            }
        }

        [TestMethod]
        public void TestUpdateSubscriptionPlans()
        {
            if (true) // DEBUG
            {
                var service = new PointOfSaleService();

                service.UpdateSubscriptionPlans(useTest: true);
            }
        }

        [TestMethod]
        public void TestUpdateSubscribers()
        {
            if (false) // DEBUG
            {
                var service = new PointOfSaleService();

                service.UpdateCustomers();
            }
        }
    }
}
