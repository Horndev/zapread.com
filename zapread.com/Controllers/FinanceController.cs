using System.Web.Mvc;

namespace zapread.com.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class FinanceController : Controller
    {
        // GET: Finance
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            XFrameOptionsDeny();
            return View();
        }

        private void XFrameOptionsDeny()
        {
            try
            {
                Response.AddHeader("X-Frame-Options", "DENY");
            }
            catch
            {
                // TODO: add error handling - temp fix for unit test.
            }
        }
    }
}