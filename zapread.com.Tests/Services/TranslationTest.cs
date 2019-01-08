using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Google.Cloud.Translation.V2;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Translate.v2;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class TranslationTest
    {
        [TestMethod]
        public void TestDetectLanguage()
        {
            //Assert.Inconclusive();
            //var c = new BaseClientService.Initializer ()
            //{
                
            //    ApplicationName = "ZapRead",
            //    ApiKey = "",  // IP Restricted, API restricted
            //};

            //var service = new TranslateService(c);

            //TranslationClient client = new TranslationClientImpl(service, TranslationModel.ServiceDefault);
            //var detection = client.DetectLanguage(text: "Hello world.");
            //Console.WriteLine(
            //    $"{detection.Language}\tConfidence: {detection.Confidence}");
        }
    }
}
