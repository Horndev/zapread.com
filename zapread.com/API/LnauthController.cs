using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using zapread.com.Helpers;
using zapread.com.Models.Account;
using zapread.com.Services;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.AspNet.Identity;

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
        [Route("lnauth/auth")]
        public ActionResult Login(string cb, string client_id, string redirect_uri, string scope, string state)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // Generate a random 32 byte challenge k1
                byte[] k1 = new byte[32];
                rng.GetBytes(k1);
                var k1str = BitConverter.ToString(k1).Replace("-", string.Empty);

                //var serviceHost = System.Configuration.ConfigurationManager.AppSettings["LnAuth_Host"];
                //var url = serviceHost + "/lnauth/signin?tag=login&k1=" + k1str;
                var url = Request.Url.GetLeftPart(UriPartial.Authority) + "/lnauth/signin?tag=login&k1=" + k1str;
                //var url = "http://192.168.0.172:27543" + "/lnauth/signin?tag=login&k1=" + k1str;

                var dataStr = CryptoService.Bech32.EncodeString("lnurl", url);

                // Make it a qr
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(dataStr, QRCodeGenerator.ECCLevel.L); //, forceUtf8: true);
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        Bitmap qrCodeImage = qrCode.GetGraphic(20);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            qrCodeImage.Save(ms, ImageFormat.Png);
                            Image png = Image.FromStream(ms);
                            byte[] imgdata = png.ToByteArray(ImageFormat.Png);
                            var base64String = Convert.ToBase64String(imgdata);

                            var vm = new LNAuthLoginView()
                            {
                                QrImageBase64 = base64String,
                                client_id = client_id,
                                redirect_uri = redirect_uri,
                                state = state,
                                k1 = k1str,
                                dataStr = dataStr,
                            };

                            //System.IO.File.AppendAllText(@"D:\Lnauthdebug.txt",
                            //    "Send to wallet:" + Environment.NewLine +
                            //    "req:" + dataStr + Environment.NewLine +
                            //    "url:" + url + Environment.NewLine);

                            return View(vm);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parameters received from wallet
        /// </summary>
        public class Authparams
        {
            public string sig { get; set; }
            public string k1 { get; set; }
            public string key { get; set; }
            public string tag { get; set; }
            public string action { get; set; }
            public string cb { get; set; }
        }

        /// <summary>
        /// https://github.com/fiatjaf/lnurl-rfc/blob/luds/04.md
        /// 
        /// Get the LN auth challenge
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("lnauth/signin")]
        public async Task<JsonResult> Signin(Authparams getparams)
        {
            // Examples
            // https://github.com/chill117/lnurl-node/blob/master/lib/verifyAuthorizationSignature.js
            // https://github.com/ko-redtruck/ln-auth-python/blob/master/app.py

            if (getparams == null)
            {
                return Json(new { status= "ERROR", reason= "Parameter error" }, JsonRequestBehavior.AllowGet);
            }

            //System.IO.File.AppendAllText(@"D:\Lnauthdebug.txt",
            //        "Response from wallet:" + Environment.NewLine +
            //        "key:" + getparams.key + Environment.NewLine +
            //        "k1:" + getparams.k1 + Environment.NewLine +
            //        "sig:" + getparams.sig + Environment.NewLine);

            var isValid = CryptoService.VerifyHashSignatureSecp256k1(
                pubKey: getparams.key,
                hash: getparams.k1,
                signature: getparams.sig);

            //System.IO.File.AppendAllText(@"D:\Lnauthdebug.txt",
            //        "isValid:" + (isValid ? "True" : "False") + Environment.NewLine + Environment.NewLine);

            if (isValid)
            {
                // need to return user to callback endpoint
                await NotificationService.SendLnAuthLoginNotification(
                    userId: getparams.k1,
                    callback: "/lnauth/callback",
                    token: getparams.key).ConfigureAwait(true);
            }
            else
            {
                return Json(new { status = "ERROR", reason = "Unable to validate signature." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "OK" }, JsonRequestBehavior.AllowGet);
        }
    }
}