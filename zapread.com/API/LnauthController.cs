using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using zapread.com.Services;
using QRCoder;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using zapread.com.Helpers;
using zapread.com.Models.Account;

namespace zapread.com.API
{
    /// <summary>
    /// https://github.com/fiatjaf/lnurl-rfc
    /// </summary>
    public class LnauthController : Controller
    {
        /// <summary>
        /// https://github.com/fiatjaf/lnurl-rfc/blob/luds/04.md
        /// 
        /// Get the LN auth challenge
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("lnauth")]
        public ActionResult Login()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // Generate a random 32 byte challenge k1
                byte[] k1 = new byte[32];
                rng.GetBytes(k1);

                var k1str = BitConverter.ToString(k1).Replace("-", string.Empty);

                var host = Request.Url;

                //var url = host.GetLeftPart(UriPartial.Authority) + "/lnauth/signin?tag=login&k1=" + k1str;
                var url = "http://192.168.0.172:27543" + "/lnauth/signin?tag=login&k1=" + k1str;

                var dataStr = CryptoService.Bech32.EncodeString(url);

                // Make it a qr
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(dataStr, QRCodeGenerator.ECCLevel.L);//, forceUtf8: true);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                MemoryStream ms = new MemoryStream();
                qrCodeImage.Save(ms, ImageFormat.Png);

                Image png = Image.FromStream(ms);
                byte[] imgdata = png.ToByteArray(ImageFormat.Png);
                var base64String = Convert.ToBase64String(imgdata);

                //return File(ms.ToArray(), "image/png");

                var vm = new LNAuthLoginView()
                {
                    QrImageBase64 = base64String,
                };

                return View(vm);
            }
        }

        public class Authparams
        {
            public string sig { get; set; }
            public string k1 { get; set; }
            public string key { get; set; }
            public string tag { get; set; }
            public string action { get; set; }
        }

        /// <summary>
        /// https://github.com/fiatjaf/lnurl-rfc/blob/luds/04.md
        /// 
        /// Get the LN auth challenge
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("lnauth/signin")]
        public JsonResult Signin(Authparams getparams)
        {
            //login code

            return Json(new { status = "OK"}, JsonRequestBehavior.AllowGet);
        }
    }
}