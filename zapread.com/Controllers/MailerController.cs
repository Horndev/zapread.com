using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Manage;
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
        public ActionResult MailerNewPost(int id)
        {
            using (var db = new ZapContext())
            {

                Post pst = db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
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

                ViewBag.Message = "New post from a user you are following: " + pst.UserId.Name;
                return View(vm);
            }
        }

        /// <summary>
        /// Mailer renders HTML for comment on post
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> MailerNewComment(int? id)
        {
            using (var db = new ZapContext())
            {
                var c = await db.Comments
                    .Include(cmt => cmt.UserId)
                    .Include(cmt => cmt.Post)
                    //.Take(1)
                    //.AsNoTracking()
                    //.FirstOrDefault();
                    .FirstOrDefaultAsync(cmt => cmt.CommentId == id);

                var vm = new PostCommentsViewModel()
                {
                    Comment = c,
                };

                return View(vm);
            }
        }

        /// <summary>
        /// Render HTML for comment reply
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MailerCommentReply(int id)
        {
            return View();
        }

        public async Task<bool> SendPostComment(long id, string email, string subject)
        {
            using (var db = new ZapContext())
            {
                var c = await db.Comments
                    .Include(cmt => cmt.UserId)
                    .Include(cmt => cmt.Post)
                    .FirstOrDefaultAsync(cmt => cmt.CommentId == id);

                var vm = new PostCommentsViewModel()
                {
                    Comment = c,
                };

                if (c == null)
                {
                    return false;
                }

                ViewBag.Message = subject;
                string HTMLString = RenderViewToString("MailerNewComment", vm);

                //debug
                //email = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];

                await SendMailAsync(HTMLString, email, subject);
            }
            return true;
        }

        /// <summary>
        /// Generates the HTML to be mailed out
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="oldUserName"></param>
        /// <returns></returns>
        public async Task<string> GenerateUpdatedUserAliasEmailBod(int id, string userName, string oldUserName)
        {
            using (var db = new ZapContext())
            {
                var u = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(usr => usr.Id == id);

                UpdatedUserAliasView vm = new UpdatedUserAliasView()
                {
                    OldUserName = oldUserName,
                    NewUserName = userName,
                    User = u
                };

                ViewBag.Message = "User Alias Updated";
                string HTMLString = RenderViewToString("MailerUpdatedUserAlias", vm);

                string msgHTML = CleanMail(HTMLString, subject: "");

                return msgHTML;
            }
        }

        /// <summary>
        /// Generates the HTML to be mailed out
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task<string> GenerateNewPostEmailBod(int id, string subject)
        {
            // TODO: convert youtube embeds to images to mail out: https://img.youtube.com/vi/ifesHElrfuo/hqdefault.jpg
            using (var db = new ZapContext())
            {
                Post pst = await db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PostId == id);

                var groups = db.Groups
                        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .AsNoTracking()
                        .ToListAsync();

                if (pst == null)
                {
                    return null;
                }

                PostViewModel vm = new PostViewModel()
                {
                    Post = pst,
                };

                ViewBag.Message = subject;
                string HTMLString = RenderViewToString("MailerNewPost", vm);

                string msgHTML = CleanMail(HTMLString, subject);

                return msgHTML;
            }
        }

        public async Task<bool> SendNewPost(int id, string email, string subject)
        {
            using (var db = new ZapContext())
            {
                Post pst = await db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PostId == id);

                var groups = db.Groups
                        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                        .AsNoTracking()
                        .ToListAsync();

                if (pst == null)
                {
                    return false;
                }

                PostViewModel vm = new PostViewModel()
                {
                    Post = pst,
                };

                ViewBag.Message = subject;
                string HTMLString = RenderViewToString("MailerNewPost", vm);

                //debug
                //email = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];

                await SendMailAsync(HTMLString, email, subject);
            }
            return true;
        }

        private string CleanMail(string HTMLString, string subject)
        {
            PreMailer.Net.InlineResult result;
            string msgHTML;
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
            return msgHTML;
        }

        private Task SendMailAsync(string HTMLString, string email, string subject)
        {
            string msgHTML = CleanMail(HTMLString, subject);

            return MailingService.SendAsync(user: "Notify",
                message: new UserEmailModel()
                {
                    Destination = email,
                    Body = msgHTML,
                    Email = "",
                    Name = "zapread.com",
                    Subject = subject,
                });
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
                    ViewEngines.Engines.FindView(ControllerContext, "~/Views/Mailer/" + viewName + ".cshtml", null);
                ViewContext viewContext = new ViewContext
                (ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}//