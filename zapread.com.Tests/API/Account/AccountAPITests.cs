using System;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using zapread.com.API;
using zapread.com.Models.API.Account;
using zapread.com.Services;

namespace zapread.com.Tests.API.Account
{
    [TestClass]
    public class AccountAPITests
    {
        [TestMethod]
        public void ManageRequestAPIKey()
        {
            // Arrange
            var userMock = new Mock<IPrincipal>();
            userMock.Setup(p => p.IsInRole("Administrator")).Returns(true);
            userMock.SetupGet(p => p.Identity.Name).Returns("test");
            userMock.SetupGet(p => p.Identity.IsAuthenticated).Returns(true);

            var requestContext = new Mock<HttpRequestContext>();
            requestContext.Setup(x => x.Principal).Returns(userMock.Object);

            // Inject POSService
            var posService = new PointOfSaleService();

            var controller = new AccountController(posService)
            {
                RequestContext = requestContext.Object,
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            
            // Act
            APIKeyResponse result = controller.RequestAPIKey("test").Result;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
