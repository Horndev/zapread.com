﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using zapread.com.Controllers;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class GroupControllerTest
    {
        [TestMethod]
        public void TestGroupMembers()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            GroupController controller = new GroupController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.Members(1).Result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestGroupIndexLoggedIn()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            GroupController controller = new GroupController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.Index().Result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestGroupIndexAnonymous()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            IPrincipal principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            GroupController controller = new GroupController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.Index().Result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestGroupDetailAnonymous()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            IPrincipal principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            GroupController controller = new GroupController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.GroupDetail(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestGroupDetailLoggedIn()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            GroupController controller = new GroupController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.GroupDetail(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
