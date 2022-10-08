using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using zapread.com.Models.Tag;

namespace zapread.com.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class TagController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tags")]
        public ActionResult Index()
        {
            var vm = new TagsViewModel();
            return View(vm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "tagName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("tag/{tagName?}")]
        public ActionResult Detail(string tagName)
        {
            var vm = new TagsViewModel()
            {TagName = tagName};
            return View(vm);
        }
    }
}