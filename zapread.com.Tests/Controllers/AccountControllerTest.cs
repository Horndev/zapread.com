using System;
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
    public class AccountControllerTest
    {
        [TestMethod]
        public void TestAccountBalance()
        {
            // Arrange
            AccountController controller = new AccountController();

            // Act
            PartialViewResult result = controller.Balance() as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestAccountGetBalanceUnauthenticated()
        {
            // Arrange
            var request = new Mock<HttpRequestBase>();

            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection {
                    {"X-Requested-With", "XMLHttpRequest"}
                });

            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);

            
            AccountController controller = new AccountController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            JsonResult result = controller.GetBalance() as JsonResult;

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
            {
                Id = "f752739e-8d58-4bf5-a140-fc225cc5ebdb"
            };

            userManager.Setup(x => x.FindByNameAsync("Test")).Returns(Task.FromResult(testUser));

            //var request = new Mock<HttpRequestBase>();

            //var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);

            AccountController controller = new AccountController(userManager.Object, signInManager.Object);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(context.Object, new RouteData()), routes);

            // Act
            LoginViewModel vm = new LoginViewModel()
            {
                UserName = "Test",
                Password = "Testing",
                RememberMe = false,
            };

            ActionResult result = controller.Login(model: vm, returnUrl: "").Result as ActionResult;

            // Assert
            Assert.IsNotNull(result);
        }

    }

    public class MockUserStore : IUserStore<ApplicationUser>
    {
        public MockUserStore()
        {
        }

        public Task CreateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> FindByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }
}
