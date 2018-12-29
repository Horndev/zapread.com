using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Controllers;

namespace zapread.com.Tests.Controllers
{
    [TestClass]
    public class GroupControllerTest
    {
        [TestMethod]
        public void TestGroupMembers()
        {
            // Arrange
            GroupController controller = new GroupController();

            // Act
            ViewResult result = controller.Members(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
