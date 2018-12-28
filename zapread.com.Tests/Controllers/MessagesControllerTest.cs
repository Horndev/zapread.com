using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Controllers;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class MessagesControllerTest
    {
        [TestMethod]
        public void TestRecentUnreadMessages()
        {
            // Arrange
            MessagesController controller = new MessagesController();

            // Act
            ActionResult result = controller.RecentUnreadMessages(1) as ActionResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
