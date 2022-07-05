using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using zapread.com.Controllers;
using zapread.com.Services;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class LightningControllerTest
    {
        [TestMethod]
        public void TestCheckPayment()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[]{"user"});
            context.Setup(s => s.User).Returns(principal);
            ILightningPayments lightningPayments = new LightningPayments();
            LightningController controller = new LightningController(lightningPayments);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            // Act
            JsonResult result = controller.CheckPayment("badinvoice").Result as JsonResult;
            // Assert
            Assert.IsNotNull(result);
        }
    }
}