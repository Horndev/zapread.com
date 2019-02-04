using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Services;
using System.Data.Entity;
using HtmlAgilityPack;

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
        public ActionResult NewPost(int id)
        {
            using (var db = new ZapContext())
            {

                var pst = db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.Comments)
                    .Include(p => p.Comments.Select(cmt => cmt.Parent))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesUp))
                    .Include(p => p.Comments.Select(cmt => cmt.VotesDown))
                    .Include(p => p.Comments.Select(cmt => cmt.UserId))
                    .Include("UserId")
                    .AsNoTracking()
                    .FirstOrDefault(p => p.PostId == id);

                List<int> viewerIgnoredUsers = new List<int>();
                
                if (pst == null)
                {
                    return RedirectToAction("PostNotFound");
                }

                var groups = db.Groups
                        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .AsNoTracking()
                        .ToList();

                PostViewModel vm = new PostViewModel()
                {
                    Post = pst,
                };

                string HTMLString = RenderViewToString("NewPost", vm);

                PreMailer.Net.InlineResult result;
                string msgHTML;
                SendTestMail(HTMLString, out result, out msgHTML);

                return View(vm);
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult TestMailer()
        {
            string HTMLString = RenderViewToString("Index", null);

            //var baseUri = new Uri("https://www.zapread.com");
            //var pm = new PreMailer.Net.PreMailer(HTMLString, baseUri);

            PreMailer.Net.InlineResult result;
            string msgHTML;
            SendTestMail(HTMLString, out result, out msgHTML);

            var msgWarn = String.Join(",", result.Warnings);    // string[] of any warnings that occurred during processing.
            Encoding utf8 = Encoding.UTF8;
            Encoding ascii = Encoding.ASCII;
            return Json(new
            {
                HTMLString,
                msg = ascii.GetString(Encoding.Convert(utf8, ascii, utf8.GetBytes(msgHTML))),
                msgWarn
            }, JsonRequestBehavior.AllowGet);
        }

        private static void SendTestMail(string HTMLString, out PreMailer.Net.InlineResult result, out string msgHTML)
        {
            var baseUri = new Uri("https://www.zapread.com/");
            result = PreMailer.Net.PreMailer.MoveCssInline(
                                baseUri: baseUri,
                                html: HTMLString,
                                removeComments: true,
                                stripIdAndClassAttributes: true
                                );
            string msgHTMLPre = result.Html;

            var doc = new HtmlDocument();
            doc.LoadHtml(msgHTMLPre);
            var imgs = doc.DocumentNode.SelectNodes("//img/@src");
            if (imgs != null)
            {
                foreach (var item in imgs)
                {
                    // TODO: check if external url
                    item.SetAttributeValue("src", new Uri(baseUri, item.GetAttributeValue("src", "")).AbsoluteUri);
                }
            }

            var links = doc.DocumentNode.SelectNodes("//a/@href");
            if (links != null)
            {
                foreach (var link in links)
                {
                    // TODO: check if external url
                    link.SetAttributeValue("href", new Uri(baseUri, link.GetAttributeValue("href", "")).AbsoluteUri);
                }
            }

            msgHTML = doc.DocumentNode.OuterHtml;

            MailingService.Send(new UserEmailModel()
            {
                Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                Body = msgHTML,
                Email = "",
                Name = "zapread.com Mailer",
                Subject = "zapread.com Mailer Test",
            });
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