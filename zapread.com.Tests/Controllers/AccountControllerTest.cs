using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Controllers;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void TestBalance()
        {
            // Arrange
            AccountController controller = new AccountController();

            // Act
            ActionResult result = controller.Balance() as ActionResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
