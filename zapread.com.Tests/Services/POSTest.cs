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

                service.GetSubscriptions();
            }
        }

        [TestMethod]
        public void TestUpdateSubscriptionPlans()
        {
            if (false) // DEBUG
            {
                var service = new PointOfSaleService();

                service.UpdateSubscriptionPlans();
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
