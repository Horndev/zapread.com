using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Controllers;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class SanitizationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var text = @"<script>window.location.href = 'https://www.google.com';</script>";

            var clean = PostController.SanitizePostXSS(text);

            Assert.IsTrue(clean == "");

            var text2 = @"Ś ę ę ć ż ł ł ł ó ż ł ó ż ć";

            var clean2 = PostController.SanitizePostXSS(text2);

            Assert.IsTrue(clean2 != "");
        }
    }
}
