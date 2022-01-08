using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class UserControllerTests
    {
        [TestMethod]
        public void UserIndex()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");

            // If using local DB, the user id is different
            var dbconnection = System.Configuration.ConfigurationManager.AppSettings["SiteConnectionString"];

            var appid = "f752739e-8d58-4bf5-a140-fc225cc5ebdb";
            if (dbconnection == "ZapreadLocal")
            {
                appid = "96b762df-5fb3-43ff-ba55-7da1fc9750c8";
            }

            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", appid)); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            UserController controller = new UserController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.Index(username: "test2").Result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestUserFollow()
        {

        }

        [TestMethod]
        public void TestUserToggleIgnore()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            UserController controller = new UserController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            JsonResult result = controller.ToggleIgnore(1) as JsonResult;

            // Assert
            Assert.IsNotNull(result);

        }
    }
}
