using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using zapread.com.Controllers;
using zapread.com.Models;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class AdminControllerTest
    {
        [TestMethod]
        public void TestGetPostStats()
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

            AdminController controller = new AdminController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            JsonResult result = controller.GetPostStats() as JsonResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
