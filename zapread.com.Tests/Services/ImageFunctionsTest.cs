using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using zapread.com.Helpers;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class ImageFunctionsTest
    {
        [TestMethod]
        [DeploymentItem(@"./Data/test.gif", "testdata")]
        public void TestResize()
        {
            // Arrange
            string filename = "test.gif";
            int maxwidth = 200;
            byte[] data = null;

            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

            Image img = Image.FromFile(@"testdata\test.gif");

            var code = System.Configuration.ConfigurationManager.AppSettings["FunctionsImagesKey"];
            var functionUrl = System.Configuration.ConfigurationManager.AppSettings["FunctionsImagesURL"];

            //Act
            using (var client = new System.Net.Http.HttpClient())
            using (var content = new System.Net.Http.MultipartFormDataContent())
            {
                byte[] originImage = img.ToByteArray(ImageFormat.Gif);

                // attach file to request
                content.Add(new System.Net.Http.StreamContent(new MemoryStream(originImage)), "file", filename);

                // create the rest call to resize function
                using (var response = client.PostAsync($"{functionUrl}?code={code}&size={maxwidth}", content).Result)
                {
                    var result = response.Content.ReadAsStreamAsync().Result;
                    Image resizedGif = Image.FromStream(result);
                    data = resizedGif.ToByteArray(ImageFormat.Gif);
                }
            }

            //Assert
            Assert.IsNotNull(data);
        }
    }
}
