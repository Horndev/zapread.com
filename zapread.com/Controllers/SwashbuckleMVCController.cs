using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;

namespace ExporterWeb.Controllers
{
    /// <summary>
    /// Swagger UI
    /// </summary>
    public class SwashbuckleMVCController : Controller
    {
        /// <summary>
        /// Swagger UI main page
        /// </summary>
        /// <returns></returns>
        // GET: SwashbucckleMVC
        [HttpGet]
        public ActionResult Index()
        {                        
            return View();
        }
    }
}