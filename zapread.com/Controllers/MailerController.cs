using Hangfire;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
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
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Mailer/Template/WeeklySummary")]
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> MailerWeeklySummary()
        {
            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();

                var startTime = DateTime.Now - TimeSpan.FromDays(7);
                var startTimePrev = DateTime.Now - TimeSpan.FromDays(14);

                var refCode = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.ReferralCode)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (refCode == null)
                {
                    var user = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    if (user != null)
                    {
                        user.ReferralCode = CryptoService.GetNewRefCode();
                        refCode = user.ReferralCode;
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }

                var vm = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new WeeklySummaryEmail()
                    {
                        RefCode = refCode,
                        TotalEarnedLastWeek = u.EarningEvents
                            .Where(e => e.TimeStamp > startTimePrev && e.TimeStamp < startTime)
                            .Sum(e => ((double?)e.Amount)) ?? 0,
                        TotalEarnedWeek = u.EarningEvents
                            .Where(e => e.TimeStamp > startTime)
                            .Sum(e => ((double?)e.Amount)) ?? 0,
                        TotalEarnedReferral = u.EarningEvents
                            .Where(e => e.TimeStamp > startTime)
                            .Where(e => e.Type == 4)
                            .Sum(e => ((double?)e.Amount)) ?? 0,
                        TotalEarnedPosts = u.EarningEvents
                            .Where(e => e.TimeStamp > startTime)
                            .Where(e => e.OriginType == 0)
                            .Sum(e => ((double?)e.Amount)) ?? 0,
                        TotalEarnedComments = u.EarningEvents
                            .Where(e => e.TimeStamp > startTime)
                            .Where(e => e.OriginType == 1)
                            .Sum(e => ((double?)e.Amount)) ?? 0,
                        TotalGroupPayments = u.EarningEvents
                            .Where(e => e.TimeStamp > startTime)
                            .Where(e => e.Type == 1)
                            .Sum(e => ((double?)e.Amount)) ?? 0,
                        TotalCommunityPayments = u.EarningEvents
                            .Where(e => e.TimeStamp > startTime)
                            .Where(e => e.Type == 2)
                            .Sum(e => ((double?)e.Amount)) ?? 0,
                        TotalPostsWeek = u.Posts.Where(p => p.TimeStamp > startTime)
                            .Count(),
                        TotalCommentsWeek = u.Comments.Where(c => c.TimeStamp > startTime)
                            .Count(),
                        TopGroups = u.EarningEvents
                            .Where(e => e.TimeStamp > startTime)
                            .Where(e => e.Type == 1)
                            .GroupBy(e => e.OriginId)
                            .Select(e => new
                            {
                                GroupId = e.Key,
                                Count = e.Count(),
                                Amount = e.Sum(v => ((double?)v.Amount)) ?? 0,
                            })
                            .Join(db.Groups, e => e.GroupId, g => g.GroupId, (i, o) => new TopGroup()
                            {
                                GroupName = o.GroupName,
                                GroupId = i.GroupId,
                                AmountEarned = i.Amount
                            })
                            .OrderByDescending(c => c.AmountEarned)
                            .Take(3)
                            .ToList(),
                    })
                    .FirstOrDefaultAsync();

                return View("WeeklySummary", vm);
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
            }
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
}