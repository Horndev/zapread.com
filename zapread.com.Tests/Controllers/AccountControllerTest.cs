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
    public class AccountControllerTest
    {
        [TestMethod]
        public void TestCaptcha()
        {
            string captcha = CaptchaService.GenerateCaptchaCode(7);
        }

        [TestMethod]
        public void TestAccountBalance()
        {
            // Arrange
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection{{"X-Requested-With", "XMLHttpRequest"}});
            request.SetupGet(x => x.IsAuthenticated).Returns(true);
            var context = new Mock<HttpContextBase>()
            {DefaultValue = DefaultValue.Mock};
            context.SetupGet(x => x.Request).Returns(request.Object);
            var responseObject = Mock.Get(context.Object.Response);
            responseObject.Setup(s => s.AddHeader("X-Frame-Options", "DENY"));
            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "f752739e-8d58-4bf5-a140-fc225cc5ebdb")); //test user
            var principal = new GenericPrincipal(identity, new[]{"user"});
            context.Setup(s => s.User).Returns(principal);
            AccountController controller = new AccountController();
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
            JsonResult result = controller.Balance().Result as JsonResult;
            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestAccountGetBalanceUnauthenticated()
        {
            // Arrange
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection{{"X-Requested-With", "XMLHttpRequest"}});
            request.SetupGet(x => x.IsAuthenticated).Returns(true);
            var context = new Mock<HttpContextBase>()
            {DefaultValue = DefaultValue.Mock};
            context.SetupGet(x => x.Request).Returns(request.Object);
            var responseObject = Mock.Get(context.Object.Response);
            responseObject.Setup(s => s.AddHeader("X-Frame-Options", "DENY"));
            AccountController controller = new AccountController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            // Act
            JsonResult result = controller.Balance().Result as JsonResult;
            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestAccountLoginStart()
        {
            // Arrange
            AccountController controller = new AccountController();
            // Act
            ViewResult result = controller.Login(returnUrl: "") as ViewResult;
            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestAccountLogin()
        {
            // Arrange
            var routes = new RouteCollection();
            var request = new Mock<HttpRequestBase>(MockBehavior.Strict);
            request.SetupGet(x => x.ApplicationPath).Returns("/");
            request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/a", UriKind.Absolute));
            request.SetupGet(x => x.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());
            var response = new Mock<HttpResponseBase>(MockBehavior.Strict);
            response.Setup(x => x.ApplyAppPathModifier("/post1")).Returns("http://localhost/post1");
            var context = new Mock<HttpContextBase>(MockBehavior.Strict);
            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Response).Returns(response.Object);
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authenticationManager = new Mock<IAuthenticationManager>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
            ApplicationUser testUser = new ApplicationUser()
            {Id = "f752739e-8d58-4bf5-a140-fc225cc5ebdb", UserName = "Test"};
            var claimsIdentity = new Mock<ClaimsIdentity>(MockBehavior.Loose);
            claimsIdentity.Setup(x => x.AddClaim(It.IsAny<Claim>()));
            userManager.Setup(x => x.FindByNameAsync("Test")).Returns(Task.FromResult(testUser));
            userManager.Setup(x => x.FindByIdAsync("f752739e-8d58-4bf5-a140-fc225cc5ebdb")).Returns(Task.FromResult(testUser));
            userManager.Setup(x => x.CreateIdentityAsync(It.IsAny<ApplicationUser>(), DefaultAuthenticationTypes.ApplicationCookie)).Returns(Task.FromResult(claimsIdentity.Object));
            context.SetupGet(x => x.Request).Returns(request.Object);
            AccountController controller = new AccountController(userManager.Object, signInManager.Object);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(context.Object, new RouteData()), routes);
            // Act
            LoginViewModel vm = new LoginViewModel()
            {UserName = "Test", Password = "Testing", RememberMe = false, };
            ActionResult result = controller.Login(model: vm, returnUrl: "").Result as ActionResult;
            // Assert
            Assert.IsNotNull(result);
        }
    }
}