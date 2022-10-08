using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using zapread.com.Controllers;
using zapread.com.Services;

namespace zapread.com.Tests.Lightning
{
    [TestClass]
    public class LNPaymentTests
    {
        [TestMethod]
        public void TestValidateInvoice()
        {
        // TODO - need to set up Moq for database and LND
        //var context = new Mock<HttpContextBase>();
        //var identity = new GenericIdentity("test");
        //identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
        //var principal = new GenericPrincipal(identity, new[] { "user" });
        //context.Setup(s => s.User).Returns(principal);
        //var mockPayments = new Mock<ILightningPayments>();
        //LightningController controller = new LightningController(paymentsService: mockPayments.Object);
        //controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
        //// Act
        //ViewResult result = controller.ValidatePaymentRequest(request: "fake").Result as ViewResult;
        //// Assert
        //Assert.IsNotNull(result);
        }
    }
}