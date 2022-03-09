using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using zapread.com.Controllers;
using zapread.com.Models;

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

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authenticationManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);

            var claimsIdentity = new Mock<ClaimsIdentity>(MockBehavior.Loose);

            claimsIdentity.Setup(x => x.AddClaim(It.IsAny<Claim>()));

            IList<UserLoginInfo> userlogins = new List<UserLoginInfo>();

            userManager.Setup(x => x.GetPhoneNumberAsync(It.IsAny<string>())).Returns(Task.FromResult("123"));
            userManager.Setup(x => x.GetTwoFactorEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            userManager.Setup(x => x.GetLoginsAsync(It.IsAny<string>())).Returns(Task.FromResult(userlogins));

            // Act
            ViewResult result = controller.Index(sort: "Score", l: "0", g: null, f: null).Result as ViewResult;

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
        public void Search()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            var result = controller.Search("fast");

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

        //[TestMethod]
        //public void PostsByScore()
        //{
        //    // Arrange
        //    var context = new Mock<HttpContextBase>();

        //    var identity = new GenericIdentity("test");
        //    IPrincipal principal = new GenericPrincipal(identity, new[] { "user" });
        //    context.Setup(s => s.User).Returns(principal);

        //    HomeController controller = new HomeController();

        //    var routeData = new RouteData();
        //    routeData.Values.Add("Controller", "Home");
        //    routeData.Values.Add("Action", "TopPosts");

        //    context.Setup(c => c.Items).Returns(new Dictionary<object, object>());

        //    controller.ControllerContext = new ControllerContext(context.Object, routeData, controller);

        //    // Act
        //    JsonResult result = controller.TopPosts(sort: "Score").Result as JsonResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //}
    }
}
