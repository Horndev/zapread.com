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
    /// <summary>
    /// Controller for the mailer (generate email content)
    /// </summary>
    public class MailerController : Controller
    {
        /// <summary>
        /// // GET: Mailer
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Mailer renders HTML for new Post.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
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

                PostViewModel vm = new PostViewModel()
                {
                    Post = pst,
                    PostTitle = pst.PostTitle,
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
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> MailerNewComment(int? id)
        {
            using (var db = new ZapContext())
            {
                var vm = await db.Comments
                    .Where(cmt => cmt.CommentId == id)
                    .Select(c => new PostCommentsViewModel()
                    {
                        CommentId = c.CommentId,
                        Score = c.Score,
                        Text = c.Text,
                        UserId = c.UserId.Id,
                        UserName = c.UserId.Name,
                        UserAppId = c.UserId.AppId,
                        ProfileImageVersion = c.UserId.ProfileImage.Version,
                        PostTitle = c.Post == null ? "" : c.Post.PostTitle,
                        PostId = c.Post == null ? 0 : c.Post.PostId,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                return View(vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task<bool> SendPostComment(long id, string email, string subject)
        {
            using (var db = new ZapContext())
            {
                var vm = await db.Comments
                    .Where(cmt => cmt.CommentId == id)
                    .Select(c => new PostCommentsViewModel()
                    {
                        CommentId = c.CommentId,
                        Score = c.Score,
                        Text = c.Text,
                        UserId = c.UserId.Id,
                        UserName = c.UserId.Name,
                        UserAppId = c.UserId.AppId,
                        ProfileImageVersion = c.UserId.ProfileImage.Version,
                        PostTitle = c.Post == null ? "" : c.Post.PostTitle,
                        PostId = c.Post == null ? 0 : c.Post.PostId,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                ViewBag.Message = subject;
                string HTMLString = RenderViewToString("MailerNewComment", vm);

                await SendMailAsync(HTMLString, email, subject).ConfigureAwait(true);
            }
            return true;
        }

        /// <summary>
        /// Render HTML for comment reply
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Mailer/Template/CommentReply/{id}")]
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> MailerCommentReply(int id)
        {
            using (var db = new ZapContext())
            {
                var vm = await db.Comments
                    .Where(cmt => cmt.CommentId == id)
                    .Select(c => new PostCommentsViewModel()
                    {
                        CommentId = c.CommentId,
                        Score = c.Score,
                        Text = c.Text,
                        UserId = c.UserId.Id,
                        UserName = c.UserId.Name,
                        UserAppId = c.UserId.AppId,
                        ProfileImageVersion = c.UserId.ProfileImage.Version,
                        PostTitle = c.Post == null ? "" : c.Post.PostTitle,
                        PostId = c.Post == null ? 0 : c.Post.PostId,
                        IsReply = c.IsReply,
                        ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                        ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                        ParentUserAppId = c.Parent == null ? "" : c.Parent.UserId.AppId,
                        ParentUserProfileImageVersion = c.Parent == null ? 0 : c.Parent.UserId.ProfileImage.Version,
                        ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                        ParentCommentText = c.Parent == null ? "" : c.Parent.Text,
                        ParentScore = c.Parent == null ? 0 : c.Parent.Score,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                return View(viewName: "NewCommentReply", model: vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task<bool> SendPostCommentReply(long id, string email, string subject)
        {
            using (var db = new ZapContext())
            {
                var vm = await db.Comments
                    .Where(cmt => cmt.CommentId == id)
                    .Select(c => new PostCommentsViewModel()
                    {
                        CommentId = c.CommentId,
                        Score = c.Score,
                        Text = c.Text,
                        UserId = c.UserId.Id,
                        UserName = c.UserId.Name,
                        UserAppId = c.UserId.AppId,
                        ProfileImageVersion = c.UserId.ProfileImage.Version,
                        PostTitle = c.Post == null ? "" : c.Post.PostTitle,
                        PostId = c.Post == null ? 0 : c.Post.PostId,
                        IsReply = c.IsReply,
                        ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                        ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                        ParentUserAppId = c.Parent == null ? "" : c.Parent.UserId.AppId,
                        ParentUserProfileImageVersion = c.Parent == null ? 0 : c.Parent.UserId.ProfileImage.Version,
                        ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                        ParentCommentText = c.Parent == null ? "" : c.Parent.Text,
                        ParentScore = c.Parent == null ? 0 : c.Parent.Score,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                ViewBag.Message = subject;
                string HTMLString = RenderViewToString("NewCommentReply", vm);

                await SendMailAsync(HTMLString, email, subject).ConfigureAwait(true);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Mailer/Template/NewChat/{id}")]
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> MailerNewChat(int id)
        {
            using (var db = new ZapContext())
            {
                var vm = await db.Messages
                    .Where(m => m.Id == id)
                    .Select(m => new ChatMessageViewModel()
                    {
                        Content = m.Content,
                        TimeStamp = m.TimeStamp.Value,
                        FromName = m.From.Name,
                        FromAppId = m.From.AppId,
                        IsReceived = true,
                        FromProfileImgVersion = m.From.ProfileImage.Version,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                return View("NewChat", vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task<bool> SendNewChat(long id, string email, string subject)
        {
            using (var db = new ZapContext())
            {
                var vm = await db.Messages
                    .Where(m => m.Id == id)
                    .Select(m => new ChatMessageViewModel()
                    {
                        Content = m.Content,
                        TimeStamp = m.TimeStamp.Value,
                        FromName = m.From.Name,
                        FromAppId = m.From.AppId,
                        IsReceived = true,
                        FromProfileImgVersion = m.From.ProfileImage.Version,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                ViewBag.Message = subject;
                string HTMLString = RenderViewToString("NewChat", vm);

                //await SendMailAsync(HTMLString, email, subject).ConfigureAwait(true);
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

                string msgHTML = CleanMail(HTMLString);

                return msgHTML;
            }
        }

        /// <summary>
        /// Generates the HTML to be mailed out
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task<string> GenerateNewPostEmailBody(int id)
        {
            // TODO: convert youtube embeds to images to mail out: https://img.youtube.com/vi/ifesHElrfuo/hqdefault.jpg
            using (var db = new ZapContext())
            {
                Post pst = await db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.ProfileImage)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PostId == id).ConfigureAwait(true);

                //var groups = db.Groups
                //        .Select(gr => new { gr.GroupId, pc = gr.Posts.Count, mc = gr.Members.Count, l = gr.Tier })
                //        .AsNoTracking()
                //        .ToListAsync();

                if (pst == null)
                {
                    return null;
                }

                PostViewModel vm = new PostViewModel()
                {
                    Post = pst,
                };

                ViewBag.Message = "New post by user you are following";
                string HTMLString = RenderViewToString("MailerNewPost", vm);

                string msgHTML = CleanMail(HTMLString);

                return msgHTML;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
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

        private string CleanMail(string HTMLString)
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
            string msgHTML = CleanMail(HTMLString);

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