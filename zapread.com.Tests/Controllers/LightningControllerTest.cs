using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Controllers;
using zapread.com.Services;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class LightningControllerTest
    {
        [TestMethod]
        public void TestCheckPayment()
        {
            // Arrange
            ILightningPayments lightningPayments = new LightningPayments();

            LightningController controller = new LightningController(lightningPayments);

            // Act
            JsonResult result = controller.CheckPayment("badinvoice").Result as JsonResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
