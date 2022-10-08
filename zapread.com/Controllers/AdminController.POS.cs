using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace zapread.com.Controllers
{
    [Authorize(Roles = "Administrator")]
    public partial class AdminController : Controller
    {
        /// <summary>
        /// View for Proof of State
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Admin/POS"), Authorize(Roles = "Administrator")]
        public ActionResult POS()
        {
            // Redirect to login screen if not authenticated.
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new
                {
                    returnUrl = "/Admin/POS/"
                });
            }

            return View();
        }
    }
}