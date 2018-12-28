using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com;
using zapread.com.Controllers;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            var identity = new GenericIdentity("test");
            var principal = new GenericPrincipal(identity, null);
    

            Thread.CurrentPrincipal = principal;
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index(sort: "Score", l: "").Result as ViewResult;

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
