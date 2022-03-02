using Microsoft.AspNet.Identity;
using QRCoder;
using System;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Controller for images
    /// </summary>
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

        /// <summary>
        /// Returns the image for the achievement specified by id
        /// 
        /// Cached: 1 day
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutputCache(Duration = 86400, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [HttpGet]
        public async Task<ActionResult> AchievementImage(string id)
        {
            // Check for image in DB
            using (var db = new ZapContext())
            {
                int imgid = Convert.ToInt32(id);
                int size = 20;
                var i = await db.Achievements
                    .FirstOrDefaultAsync(a => a.Id == imgid).ConfigureAwait(true);

                if (i != null && i.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream(i.Image))
                    {
                        Image png = Image.FromStream(ms);
                        using (Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size))
                        {
                            byte[] data = thumb.ToByteArray(ImageFormat.Png);
                            return File(data, "image/png");
                        }
                    }
                }
                else
                {
                    i = await db.Achievements
                        .FirstOrDefaultAsync(a => a.Id == 1).ConfigureAwait(true);
                    if (i != null)
                    {
                        Image png = Image.FromStream(new MemoryStream(i.Image));
                        Bitmap thumb = ImageExtensions.ResizeImage(png, (int)size, (int)size);
                        byte[] data = thumb.ToByteArray(ImageFormat.Png);
                        return File(data, "image/png");
                    }

                    Bitmap img = new Bitmap((int)size, (int)size);
                    return File(img.ToByteArray(ImageFormat.Png), "image/png");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Img/Group/DefaultIcon")]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> SetDefaultGroupIcon(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return Json(new { success = false, message = "no file" });
            }

            using (var db = new ZapContext())
            {
                if (file.ContentLength > 0)
                {
                    Image img = Image.FromStream(file.InputStream);

                    byte[] data;
                    string contentType = "image/jpeg";
                    if (img.RawFormat.Equals(ImageFormat.Gif))
                    {
                        contentType = "image/gif";
                    }
                    else
                    {

                    }
                    int maxwidth = 50;

                    var scale = Convert.ToDouble(maxwidth) / Convert.ToDouble(img.Width);

                    using (Bitmap thumb = ImageExtensions.ResizeImage(img, maxwidth, Convert.ToInt32(img.Height * scale)))
                    {
                        if (img.RawFormat.Equals(ImageFormat.Gif))
                        {
                            data = thumb.ToByteArray(ImageFormat.Gif);
                        }
                        else
                        {
                            data = thumb.ToByteArray(ImageFormat.Jpeg);
                        }

                        UserImage i = await db.Images
                            .FirstOrDefaultAsync(im => im.ImageId == 1).ConfigureAwait(false);

                        if (i == null)
                        {
                            i = new UserImage()
                            {
                                ImageId = 1
                            };
                            db.Images.Add(i);
                        }

                        i.ContentType = contentType;
                        i.Image = data;

                        await db.SaveChangesAsync().ConfigureAwait(false);
                        return Json(new { result = "success", imgId = i.ImageId });

                    }
                }
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [Route("Img/Group/Icon/{groupId}")]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> SetGroupIcon([System.Web.Http.FromBody]HttpPostedFileBase file, int groupId)
        {
            if (file == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "no file" });
            }

            using (var db = new ZapContext())
            {
                if (!(file.ContentLength > 0))
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "empty file" });
                }
                else
                {
                    // Authorization
                    var userAppId = User.Identity.GetUserId();
                    if (userAppId == null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Json(new { success = false, message = "Unknown user" });
                    }

                    var giq = db.Groups
                        .Where(g => g.GroupId == groupId);

                    if (!User.IsInRole("Administrator"))
                    {
                        giq = giq.Where(g => g.Administrators.Select(a => a.AppId).Contains(userAppId));
                    }

                    var groupInfo = await giq
                        //.Select(g => new { 
                        //    g.GroupName,
                        //    g.GroupImage })
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    if (groupInfo == null && groupId != -1)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Json(new { success = false, message = "You must be an administrator to change group image." });
                    }

                    Image img = Image.FromStream(file.InputStream);

                    byte[] data;
                    string contentType = "image/jpeg";
                    if (img.RawFormat.Equals(ImageFormat.Gif))
                    {
                        contentType = "image/gif";
                    }
                    else
                    {

                    }
                    int maxwidth = 50;

                    var scale = Convert.ToDouble(maxwidth) / Convert.ToDouble(img.Width);

                    using (Bitmap thumb = ImageExtensions.ResizeImage(img, maxwidth, Convert.ToInt32(img.Height * scale)))
                    {
                        if (img.RawFormat.Equals(ImageFormat.Gif))
                        {
                            data = thumb.ToByteArray(ImageFormat.Gif);
                        }
                        else
                        {
                            data = thumb.ToByteArray(ImageFormat.Jpeg);
                        }

                        UserImage i = groupId == -1 ? null : groupInfo.GroupImage;
                        //await db.Groups
                        //    .Where(g => g.GroupId == groupId)
                        //    .Select(g => g.GroupImage)
                        //    .FirstOrDefaultAsync().ConfigureAwait(false);

                        // No group or no image for group
                        if (i == null)
                        {
                            i = new UserImage()
                            {
                            };

                            if (groupId > 0)
                            {
                                var group = await db.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId)
                                    .ConfigureAwait(false);
                                group.GroupImage = i;
                            }
                        }

                        i.ContentType = contentType;
                        i.Image = data;

                        // New image
                        if (groupId < 1)
                        {
                            db.Images.Add(i);
                        }

                        await db.SaveChangesAsync().ConfigureAwait(false);
                        return Json(new { result = "success", imgId = i.ImageId });
                    }
                }
            }
        }

        /// <summary>
        /// Icon for a group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Img/Group/Icon/{groupId}")]
        //[OutputCache(Duration = 600, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        public async Task<ActionResult> GroupIcon(int groupId)
        {
            using (var db = new ZapContext())
            {
                int size = 30;
                var i = await db.Groups
                    .Where(g => g.GroupId == groupId)
                    .Select(g => g.GroupImage)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
                if (i != null)
                {
                    using (var ims = new MemoryStream(i.Image))
                    {
                        Image png = Image.FromStream(ims);
                        using (Bitmap thumb = ImageExtensions.ResizeImage(png, size, size))
                        {
                            byte[] data = thumb.ToByteArray(ImageFormat.Png);
                            return File(data, "image/png");
                        } 
                    }
                }
                else
                {
                    // do we have a default image?
                    i = await db.Images
                        .Where(im => im.ImageId == 1)
                        .FirstOrDefaultAsync().ConfigureAwait(false);

                    if (i == null || i.Image == null)
                    {
                        i = await db.Images
                        .Where(im => im.Image != null)
                        .FirstOrDefaultAsync().ConfigureAwait(false);
                    }

                    using (var ims = new MemoryStream(i.Image))
                    {
                        Image png = Image.FromStream(ims);
                        using (Bitmap thumb = ImageExtensions.ResizeImage(png, size, size))
                        {
                            byte[] data = thumb.ToByteArray(ImageFormat.Png);
                            return File(data, "image/png");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets an icon from the database by it's image id
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="s">image size in pixels</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Img/Group/IconById/{imageId}")]
        public async Task<ActionResult> GroupIconById(int imageId, int? s)
        {
            using (var db = new ZapContext())
            {
                int size = s ?? 30;
                var i = await db.Images
                    .Where(g => g.ImageId == imageId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
                if (i != null)
                {
                    using (var ims = new MemoryStream(i.Image))
                    {
                        Image png = Image.FromStream(ims);
                        using (Bitmap thumb = ImageExtensions.ResizeImage(png, size, size))
                        {
                            byte[] data = thumb.ToByteArray(ImageFormat.Png);
                            return File(data, "image/png");
                        }
                    }
                }
                else
                {
                    // do we have a default image?
                    i = await db.Images
                        .Where(im => im.ImageId == 1)
                        .FirstOrDefaultAsync().ConfigureAwait(false);

                    if (i == null || i.Image == null)
                    {
                        i = await db.Images
                        .Where(im => im.Image != null)
                        .FirstOrDefaultAsync().ConfigureAwait(false);
                    }

                    using (var ims = new MemoryStream(i.Image))
                    {
                        Image png = Image.FromStream(ims);
                        using (Bitmap thumb = ImageExtensions.ResizeImage(png, size, size))
                        {
                            byte[] data = thumb.ToByteArray(ImageFormat.Png);
                            return File(data, "image/png");
                        }
                    }
                }
            }
        }

        private byte[] NotFoundImage(int width=140, int height=20)
        {
            var bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            Rectangle rect = new Rectangle(0, 0, width, height);
            
            GraphicsPath graphicPath = new GraphicsPath();
            graphicPath.AddString("Image Not Found", FontFamily.GenericSerif, (int)FontStyle.Bold, 16, rect, null);
            //var hatchBrush = new HatchBrush(HatchStyle., Color.White, Color.Teal);
            var brush = new SolidBrush(Color.Teal);
            g.FillPath(brush, graphicPath);
            brush.Dispose();
            g.Dispose();

            var bytes = bitmap.ToByteArray(ImageFormat.Jpeg);

            return bytes;
        }

        /// <summary>
        /// Get an image from the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="imge">Optional: encoded image identifier</param>
        /// <param name="f">Optional: format code</param>
        /// <returns></returns>
        [OutputCache(Duration = int.MaxValue, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
        [HttpGet]
        [Route("Img/Content/{id?}/{f?}")]
        [Route("i/{imge?}/{f?}")]
        public async Task<ActionResult> Content(int? id = null, string imge = null, string f = null)
        {
            if (imge != null)
            {
                id = CryptoService.StringToIntId(imge);
            }

            if (!id.HasValue)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return File(NotFoundImage(), "image/jpeg");
            }

            using (var db = new ZapContext())
            {
                var img = await db.Images.FirstOrDefaultAsync(i => i.ImageId == id)
                    .ConfigureAwait(false); // Don't capture context
                if (img == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return File(NotFoundImage(), "image/jpeg");
                }

                if (img.Image != null)
                {
                    var contentType = img.ContentType;
                    if (contentType == null)
                    {
                        contentType = "image/jpeg";
                    }
                    using (var ms = new MemoryStream(img.Image))
                    {
                        Image png = Image.FromStream(ms);

                        byte[] data;
                        if (contentType == "image/gif")
                        {
                            data = png.ToByteArray(ImageFormat.Gif);
                        }
                        else
                        {
                            data = png.ToByteArray(ImageFormat.Jpeg);
                        }

                        return File(data, contentType);
                    }
                }
            }
            return File(NotFoundImage(), "image/jpeg");
        }

        /// <summary>
        /// Gets the submitted string as QR image
        /// </summary>
        /// <param name="qr"></param>
        /// <returns>image/png</returns>
        [OutputCache(Duration = 60 * 60 * 24, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.Downstream)]
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

        /// <summary>
        /// Upload an image
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadImage(HttpPostedFileBase file)
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
                    string contentType = "image/jpeg";
                    if (img.RawFormat.Equals(ImageFormat.Gif))
                    {
                        contentType = "image/gif";
                    }
                    else
                    {

                    }

                    int maxwidth = 720;

                    if (img.Width > maxwidth)
                    {
                        // rescale if too large for post
                        var scale = Convert.ToDouble(maxwidth) / Convert.ToDouble(img.Width);
                        Bitmap thumb = ImageExtensions.ResizeImage(img, maxwidth, Convert.ToInt32(img.Height * scale));
                       
                        if (img.RawFormat.Equals(ImageFormat.Gif))
                        {
                            data = thumb.ToByteArray(ImageFormat.Gif);
                        }
                        else
                        {
                            data = thumb.ToByteArray(ImageFormat.Jpeg);
                        }
                    }
                    else
                    {
                        
                        if (img.RawFormat.Equals(ImageFormat.Gif))
                        {
                            data = img.ToByteArray(ImageFormat.Gif);
                        }
                        else
                        {
                            data = img.ToByteArray(ImageFormat.Jpeg);
                        }
                    }

                    UserImage i = new UserImage() { 
                        Image = data,
                        ContentType = contentType,
                    };

                    db.Images.Add(i);
                    db.SaveChanges();
                    return Json(new { 
                        success = true,
                        result = "success", 
                        imgId = i.ImageId, 
                        imgIdEnc = CryptoService.IntIdToString(i.ImageId) 
                    });
                }

                return Json(new { result = "failure" });
            }
        }
    }
}