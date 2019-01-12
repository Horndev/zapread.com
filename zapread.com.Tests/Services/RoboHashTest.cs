using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboHash.Net;
using System.Drawing.Imaging;
using System.Drawing;
using zapread.com.Helpers;

namespace zapread.com.Tests.Services
{
    [TestClass]
    public class RoboHashTest
    {
        [TestMethod]
        public void TestGenerate()
        {
            MemoryStream ms = new MemoryStream();

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tests");
            Directory.CreateDirectory(path);

            string userId = "Zelgada";
            int size = 500;

            var r = RoboHash.Net.RoboHash.Create(userId);
            using (var image = r.Render(
                set: null, 
                backgroundSet: RoboConsts.Any,
                color: null, 
                width: 400, 
                height: 400))
            {
                var name = userId + ".bg.png";
                //image.Save(Path.Combine(path, name), ImageFormat.Png);
                //image.SaveAsPng(ms);
                Bitmap thumb = ImageExtensions.ResizeImage(image, (int)size, (int)size);
                byte[] data = thumb.ToByteArray(ImageFormat.Png);

            }
        }
    }
}
