using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using zapread.com.Models;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    public class MailerController : Controller
    {
        // GET: Mailer
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Mailer renders HTML for new Post.
        /// </summary>
        /// <returns></returns>
        public ActionResult NewPost()
        {

            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult TestMailer()
        {
            string HTMLString = RenderViewToString("Index", null);

            var baseUri = new Uri("https://www.zapread.com");
            var pm = new PreMailer.Net.PreMailer(HTMLString, baseUri);

            var result = pm.MoveCssInline();
                //PreMailer.Net.PreMailer.MoveCssInline(
                //    baseUri: new Uri("https://www.zapread.com"),
                //    html: HTMLString,
                //    removeComments: true
                //    );

            var msgHTML = result.Html;      // Resultant HTML, with CSS in-lined.
            var msgWarn = String.Join(",", result.Warnings);    // string[] of any warnings that occurred during processing.

            Encoding utf8 = Encoding.UTF8;
            Encoding ascii = Encoding.ASCII;

            MailingService.Send(new UserEmailModel()
            {
                Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                Body = msgHTML,
                Email = "",
                Name = "zapread.com Mailer",
                Subject = "zapread.com Mailer Test",
            });

            return Json(new
            {
                HTMLString,
                msg = ascii.GetString(Encoding.Convert(utf8, ascii, utf8.GetBytes(msgHTML))),
                msgWarn
            }, JsonRequestBehavior.AllowGet);
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult =
                ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext
                (ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        // https://www.codemag.com/article/1312081/Rendering-ASP.NET-MVC-Razor-Views-to-String
        protected string RenderViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult =
                ViewEngines.Engines.FindView(ControllerContext, viewName, null);
                ViewContext viewContext = new ViewContext
                (ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}