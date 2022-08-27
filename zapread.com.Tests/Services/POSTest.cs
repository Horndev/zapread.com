using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
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
            if (false) // DEBUG
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


        [TestMethod]
        public void TestGetCustomerIdExistsInDB()
        {
            // Arrange
            var service = new PointOfSaleService();

            var context = new Mock<HttpContextBase>();
            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            // Act

            // Assert
            Assert.Inconclusive("TestGetCustomerIdExistsInDB not completed");
        }
    }
}
