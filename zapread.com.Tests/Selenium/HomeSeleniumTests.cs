using System;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace zapread.com.Tests.Selenium
{
    [TestClass]
    public class HomeSeleniumTests
    {
        public TestContext TestContext { get; set; }
        public IWebDriver driver { get; set; }

        [TestInitialize]
        public void TestIntialize()
        {
            string testMethodName = TestContext.TestName;
        }

        [TestMethod]
        public void TestChrome70Login()
        {
            String username = Environment.GetEnvironmentVariable("BROWSERSTACK_USERNAME");
            if (username == null)
            {
                username = ConfigurationManager.AppSettings.Get("user");
            }
            String accesskey = Environment.GetEnvironmentVariable("BROWSERSTACK_ACCESS_KEY");
            if (accesskey == null)
            {
                accesskey = ConfigurationManager.AppSettings.Get("key");
            }

            // This did not work.
            //var caps = new RemoteSessionSettings();
            ////caps.AddMetadataSetting("name", TestContext.TestName);
            //caps.AddMetadataSetting("browserstack.user", username);
            //caps.AddMetadataSetting("browserstack.key", accesskey);
            //caps.AddMetadataSetting("browserName", "Chrome");
            //caps.AddMetadataSetting("platform", "Windows 10");
            //driver = new RemoteWebDriver(new Uri("http://hub-cloud.browserstack.com/wd/hub/"), caps);

            var options = new ChromeOptions();
            options.AcceptInsecureCertificates = true;

            options.AddAdditionalCapability("Project", "ZapRead.com", true);
            options.AddAdditionalCapability("buildName", "0.3-beta", true);
            options.AddAdditionalCapability("sessionName", TestContext.TestName, true);
            options.AddAdditionalCapability("browserstack.user", username, true);
            options.AddAdditionalCapability("browserstack.key", accesskey, true);
            options.AddAdditionalCapability("version", "70", true);
            options.AddAdditionalCapability("platform", "WINDOWS", true);
            RemoteWebDriver driver = new RemoteWebDriver(
                new Uri("http://hub-cloud.browserstack.com/wd/hub/"),
                options);

            driver.Navigate().GoToUrl("https://www.zapread.com");
            Console.WriteLine(driver.Title);
            driver.FindElement(By.XPath("//a[contains(@href,'/Account/Login/')]")).Click();
            //IWebElement query = driver.FindElement(By.Name("q"));
            //query.SendKeys("Browserstack");
            //query.Submit();
            Console.WriteLine(driver.Title);
            Assert.AreEqual("Log in", driver.Title);
            driver.FindElement(By.Id("UserName")).SendKeys("BSTest");
            driver.FindElement(By.Id("Password")).SendKeys("Testing123");
            driver.FindElement(By.XPath("//button[contains(text(),'Login')]")).Click();
            Console.WriteLine(driver.Title);
            driver.Quit();
        }
    }
}
