using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace zapread.com.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadImage()
        {
            return View();
        }

        public async Task<ViewResult> UserHover()
        {

            return View();
        }

        public async Task<JsonResult> UpdateProfileImage(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                string _FileName = Path.GetFileName(file.FileName);
                MemoryStream ms = new MemoryStream();

                Image img = Image.FromStream(file.InputStream);

                
            }
            return Json(new { });
        }
    }
}