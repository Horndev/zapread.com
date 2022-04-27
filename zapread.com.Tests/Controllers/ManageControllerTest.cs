using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using zapread.com.Controllers;
using zapread.com.Models;
using zapread.com.Services;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class ManageControllerTest
    {
        [TestMethod]
        public void ManageSetupTest()
        {
            // Arrange
            Mock<HttpContextBase> context;
            Mock<ApplicationUserManager> userManager;
            Mock<ApplicationSignInManager> signInManager;
            SetupUserLoggedIn(out context, out userManager, out signInManager);

            // Act
            ManageController controller = new ManageController(userManager.Object, signInManager.Object, new EventService());
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Assert
            Assert.IsNotNull(controller.UserManager);
            Assert.IsNotNull(controller.SignInManager);
        }

        [TestMethod]
        public void ManageIndex()
        {
            // Arrange
            Mock<HttpContextBase> context;
            Mock<ApplicationUserManager> userManager;
            Mock<ApplicationSignInManager> signInManager;
            SetupUserLoggedIn(out context, out userManager, out signInManager);

            ManageController controller = new ManageController(userManager.Object, signInManager.Object, new EventService());
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.Index(null).Result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ManageUpdateUserAlias_TestEmpty()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

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

            ManageController controller = new ManageController(userManager.Object, signInManager.Object, new EventService());
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            JsonResult result = controller.UpdateUserAlias("").Result as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            IDictionary<string, object> data = new RouteValueDictionary(result.Data);

            Assert.IsTrue(condition: (string) data["result"] == "Failure");

            // Act
            result = controller.UpdateUserAlias(Uri.UnescapeDataString("%E2%80%8F")).Result as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            data = new RouteValueDictionary(result.Data);

            Assert.IsTrue(condition: (string)data["result"] == "Failure");
        }

        [TestMethod]
        public void ManageUpdateUserAlias_TestSpaces()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

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

            ManageController controller = new ManageController(userManager.Object, signInManager.Object, new EventService());
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            JsonResult result = controller.UpdateUserAlias("Bad Username").Result as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            IDictionary<string, object> data = new RouteValueDictionary(result.Data);

            Assert.IsTrue(condition: (string)data["Result"] == "Failure");
        }

        [TestMethod]
        public void ManageFinancial_LoggedIn()
        {
            // Arrange
            Mock<HttpContextBase> context;
            Mock<ApplicationUserManager> userManager;
            Mock<ApplicationSignInManager> signInManager;
            SetupUserLoggedIn(out context, out userManager, out signInManager);

            ManageController controller = new ManageController(userManager.Object, signInManager.Object, new EventService());
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            ViewResult result = controller.Financial() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        private static void SetupUserLoggedIn(out Mock<HttpContextBase> context, out Mock<ApplicationUserManager> userManager, out Mock<ApplicationSignInManager> signInManager)
        {
            context = new Mock<HttpContextBase>();
            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[] { "user" });
            context.Setup(s => s.User).Returns(principal);

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authenticationManager = new Mock<IAuthenticationManager>();
            signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
            var claimsIdentity = new Mock<ClaimsIdentity>(MockBehavior.Loose);

            claimsIdentity.Setup(x => x.AddClaim(It.IsAny<Claim>()));

            IList<UserLoginInfo> userlogins = new List<UserLoginInfo>();

            userManager.Setup(x => x.GetPhoneNumberAsync(It.IsAny<string>())).Returns(Task.FromResult("123"));
            userManager.Setup(x => x.GetTwoFactorEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            userManager.Setup(x => x.IsGoogleAuthenticatorEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            userManager.Setup(x => x.IsEmailAuthenticatorEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            userManager.Setup(x => x.GetLoginsAsync(It.IsAny<string>())).Returns(Task.FromResult(userlogins));
        }
    }
}
