using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Controllers;
using zapread.com.Helpers;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class SanitizationTest
    {
        [TestMethod]
        public void TestSanitizeXSS()
        {
            var text = @"<script>window.location.href = 'https://www.google.com';</script>";

            var clean = text.SanitizeXSS();

            Assert.IsTrue(clean == "");

            var text2 = @"Ś ę ę ć ż ł ł ł ó ż ł ó ż ć";

            var clean2 = text2.SanitizeXSS();

            Assert.IsTrue(clean2 != "");
        }
    }
}
