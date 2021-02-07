using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using zapread.com.Models;
using zapread.com.Services;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class MailingServiceTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var res = MailingService.SendAsync(user: "Accounts", useSSL: true,
                message: new UserEmailModel()
                {
                    Destination = "horn@zapread.com",
                    Body = "test message",
                    Email = "",
                    Name = "zapread.com",
                    Subject = "email subject",
                });

            var asyncres = res.Result;

            Assert.IsTrue(true);
        }
    }
}
