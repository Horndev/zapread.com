using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using zapread.com;
using zapread.com.Controllers;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void HomeIndex()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            HomeController controller = new HomeController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.Index(sort: "Score", l: "", g: null, f: null).Result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.AreEqual("About Zapread.com.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FAQ()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.FAQ() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Feedback()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Feedback() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FeedbackSuccess()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.FeedbackSuccess() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
