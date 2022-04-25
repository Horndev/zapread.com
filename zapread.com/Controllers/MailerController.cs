using Hangfire;
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
using zapread.com.Models.Email;
using zapread.com.Models.Manage;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Controller for the mailer (generate email content)
    /// 
    /// Emails sent.  ✓ indicates it is using the background mailer
    /// 
    /// [✓] MailerNewComment - New comment on a post authored
    /// [✓] MailerNewComment - New comment on a post followed
    /// [✓] MailerNewChat - New chat message when offline
    /// [ ] MailerUpdatedUserAlias - User updated their user alias
    /// [ ] MailerCommentReply - New reply to a comment
    /// [ ]  - User mentioned
    /// [ ]  - New Follower
    /// [ ]  - (weekly) payout summary
    /// [✓]  - New post by user followed
    /// 
    /// </summary>
    public class MailerController : Controller
    {
        private IEventService eventService;

        /// <summary>
        /// Constructor with DI
        /// </summary>
        /// <param name="eventService"></param>
        public MailerController(IEventService eventService)
        {
            this.eventService = eventService;
        }

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
        public async Task<ActionResult> MailerNewPost(int id)
        {
            await eventService.OnNewPostAsync(id, isTest: true);

            using (var db = new ZapContext())
            {
                var vm = db.Posts
                    .Where(p => p.PostId == id)
                    .Select(p => new NewPostEmail()
                    {
                        PostId = p.PostId,
                        PostTitle = p.PostTitle,
                        Score = p.Score,
                        UserName = p.UserId.Name,
                        UserAppId = p.UserId.AppId,
                        ProfileImageVersion = p.UserId.ProfileImage.Version,
                        GroupName = p.Group.GroupName,
                        GroupId = p.Group.GroupId,
                        Content = p.Content,
                    })
                    .FirstOrDefault();

                return View("NewPost", vm);
            }
        }

        /// <summary>
        /// Mailer renders HTML for comment on post.
        /// 
        /// [✓] Works without HttpContext
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> MailerNewComment(int? id)
        {
            await eventService.OnPostCommentAsync(Convert.ToInt64(id));

            // Render for preview
            using (var db = new ZapContext())
            {
                var vm = await db.Comments
                    .Where(cmt => cmt.CommentId == id)
                    .Select(c => new Models.Email.PostCommentEmail()
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

                if (vm == null)
                {
                    return View("PostComment", new Models.Email.PostCommentEmail() { 
                        Text = "Comment Not Found"
                    });
                }

                return View("PostComment", vm);
            }
        }

        /// <summary>
        /// Render HTML for comment reply
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Mailer/Template/CommentReply/{id}")]
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> MailerCommentReply(int? id)
        {
            // This is all that should be needed.  Test will email to the exception address
            BackgroundJob.Enqueue<MailingService>(
                 methodCall: x => x.MailPostCommentReply(
                     id.Value, // commentId
                     true // isTest
                     ));

            using (var db = new ZapContext())
            {
                var vm = await db.Comments
                    .Where(cmt => cmt.CommentId == id)
                    .Select(c => new PostCommentReplyEmail()
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
                        ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                        ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                        ParentUserAppId = c.Parent == null ? "" : c.Parent.UserId.AppId,
                        ParentUserProfileImageVersion = c.Parent == null ? 0 : c.Parent.UserId.ProfileImage.Version,
                        ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                        ParentCommentText = c.Parent == null ? "" : c.Parent.Text,
                        ParentScore = c.Parent == null ? 0 : c.Parent.Score,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                return View(viewName: "PostCommentReply", model: vm);
            }
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
            await eventService.OnNewChatAsync(id);

            using (var db = new ZapContext())
            {
                var vm = await db.Messages
                    .Where(m => m.Id == id)
                    .Select(m => new NewChatEmail()
                    {
                        FromName = m.From.Name,
                        FromAppId = m.From.AppId,
                        FromProfileImgVersion = m.From.ProfileImage.Version,
                        IsReceived = true,
                        Content = m.Content
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                return View("NewChat", vm);

                // OLD:

                var vmx = await db.Messages
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

        /// <summary>
        /// This method should not be needed here anymore.  The pre-mailing happens now in MailingService
        /// </summary>
        /// <param name="HTMLString"></param>
        /// <returns></returns>
        [Obsolete]
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

        /// <summary>
        /// // https://www.codemag.com/article/1312081/Rendering-ASP.NET-MVC-Razor-Views-to-String
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete("Should use MailerService and non-http method to render views")]
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