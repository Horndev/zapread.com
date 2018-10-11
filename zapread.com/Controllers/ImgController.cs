using Microsoft.AspNet.Identity;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;

namespace zapread.com.Controllers
{
    public class ImgController : Controller
    {
        /// <summary>
        /// Gets an image as an encode base64 string
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetB64(int id)
        {
            using (var db = new ZapContext())
            {
                MemoryStream ms = new MemoryStream();

                var img = db.Images.FirstOrDefault(i => i.ImageId == id);
                if (img.Image != null)
                {
                    Image png = Image.FromStream(new MemoryStream(img.Image));
                    byte[] data = png.ToByteArray(ImageFormat.Jpeg);
                    var base64String = Convert.ToBase64String(data);
                    return base64String;
                }
            }
            return "";
        }

        // GET: Img
        [OutputCache(Duration = int.MaxValue, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult Content(int id)
        {
            using (var db = new ZapContext())
            {
                MemoryStream ms = new MemoryStream();

                var img = db.Images.FirstOrDefault(i => i.ImageId == id);
                if (img.Image != null)
                {
                    Image png = Image.FromStream(new MemoryStream(img.Image));
                    byte[] data = png.ToByteArray(ImageFormat.Jpeg);

                    return File(data, "image/jpeg");
                }
            }
            return Json(new { result = "failure" });
        }

        // GET: Img
        [OutputCache(Duration = 60*60*24, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public ActionResult QR(string qr)
        {
            if (qr is null || qr == "")
                qr = "zapread.com";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.L);//, forceUtf8: true);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            MemoryStream ms = new MemoryStream();
            qrCodeImage.Save(ms, ImageFormat.Png);
            return File(ms.ToArray(), "image/png");
        }

        [HttpPost]
        public async Task<JsonResult> UploadImage(HttpPostedFileBase file)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    MemoryStream ms = new MemoryStream();

                    Image img = Image.FromStream(file.InputStream);

                    byte[] data;

                    int maxwidth = 720;

                    if (img.Width > maxwidth)
                    {
                        // rescale if too large for post
                        var scale = Convert.ToDouble(maxwidth) / Convert.ToDouble(img.Width);
                        Bitmap thumb = ImageExtensions.ResizeImage(img, maxwidth, Convert.ToInt32(img.Height * scale));
                        data = thumb.ToByteArray(ImageFormat.Jpeg);
                    }
                    else
                    {
                        data = img.ToByteArray(ImageFormat.Jpeg);
                    }

                    UserImage i = new UserImage() { Image = data };

                    db.Images.Add(i);
                    db.SaveChanges();
                    return Json(new { result = "success", imgId = i.ImageId });
                }

                return Json(new { result = "failure" });
            }
        }
    }
}