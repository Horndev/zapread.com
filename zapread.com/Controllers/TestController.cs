using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace zapread.com.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class TestController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
         // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadImage()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ViewResult UserHover()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "file"></param>
        /// <returns></returns>
        public JsonResult UpdateProfileImage(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                string _FileName = Path.GetFileName(file.FileName);
                MemoryStream ms = new MemoryStream();
                Image img = Image.FromStream(file.InputStream);
            }

            return Json(new
            {
            }

            );
        }
    }
}