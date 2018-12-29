using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using zapread.com.Controllers;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void TestAccountBalance()
        {
            // Arrange
            AccountController controller = new AccountController();

            // Act
            PartialViewResult result = controller.Balance() as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestAccountGetBalanceUnauthenticated()
        {
            var request = new Mock<HttpRequestBase>();

            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection {
                    {"X-Requested-With", "XMLHttpRequest"}
                });

            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);

            // Arrange
            AccountController controller = new AccountController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            JsonResult result = controller.GetBalance() as JsonResult;

            // Assert
            Assert.IsNotNull(result);
        }


    }
}
