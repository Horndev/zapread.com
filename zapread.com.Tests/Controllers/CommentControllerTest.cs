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
using zapread.com.Services;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class CommentControllerTest
    {
        [TestMethod]
        public void TestAddComment()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            var mock = new Mock<ControllerContext>();
            // If using local DB, the user id is different
            var dbconnection = System.Configuration.ConfigurationManager.AppSettings["SiteConnectionString"];
            var appid = "f752739e-8d58-4bf5-a140-fc225cc5ebdb";
            if (dbconnection == "ZapreadLocal")
            {
                appid = "96b762df-5fb3-43ff-ba55-7da1fc9750c8";
            }

            var identity = new GenericIdentity("test");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", appid)); //test user
            var principal = new GenericPrincipal(identity, new[]{"user"});
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
            var routeData = new RouteData();
            routeData.Values.Add("controller", "MessagesController");
            routeData.Values.Add("action", "GetMessage");
            mock.SetupGet(m => m.RouteData).Returns(routeData);
            var view = new Mock<IView>();
            var engine = new Mock<IViewEngine>();
            var viewEngineResult = new ViewEngineResult(view.Object, engine.Object);
            engine.Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(viewEngineResult);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(engine.Object);
            CommentController controller = new CommentController(new EventService());
            controller.ControllerContext = new ControllerContext(context.Object, routeData, controller);
            var newComment = new CommentController.NewComment()
            {IsDeleted = true, CommentContent = "test", IsReply = true, CommentId = 1, PostId = 1, IsTest = true, };
            // Act
            JsonResult result = controller.AddComment(newComment).Result as JsonResult;
            // Assert
            Assert.IsNotNull(result);
        }
    }
}