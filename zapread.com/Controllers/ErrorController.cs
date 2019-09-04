using System.Web.Mvc;

namespace zapread.com.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserNotFound()
        {
            return View();
        }

    }
}