using Microsoft.VisualStudio.TestTools.UnitTesting;
using zapread.com.Helpers;

namespace zapread.com.Tests.Helpers
{
    [TestClass]
    public class NumbersTest
    {
        [TestMethod]
        public void TestAbbr()
        {
            double LargePostive = 123456789.0;
            double LargeNegative = -95421351.0;

            string LP = LargePostive.ToAbbrString();
            string LN = LargeNegative.ToAbbrString();

            Assert.AreEqual("123.5M", LP);
            Assert.AreEqual("-95.4M", LN);
        }
    }
}
