using Hangfire;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Comments;
using zapread.com.Models.Database;
using zapread.com.Models.Database.Financial;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class CommentController : Controller
    {
        private ApplicationUserManager _userManager;

        private IEventService eventService;

        /// <summary>
        /// Default constructor for DI
        /// </summary>
        /// <param name="eventService"></param>
        public CommentController(IEventService eventService)
        {
            this.eventService = eventService;
        }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class NewComment
        {
            /// <summary>
            /// 
            /// </summary>
            [Required]
            [DataType(DataType.MultilineText)]
            [AllowHtml]
            public string CommentContent { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int PostId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int CommentId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool IsReply { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool IsDeleted { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public bool IsTest { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchstr"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public JsonResult GetMentions(string searchstr)
        {
            using (var db = new ZapContext())
            {
                var users = db.Users
                    .Where(u => u.Name.StartsWith(searchstr))
                    .Select(u => u.Name)
                    .Take(10)
                    .AsNoTracking()
                    .ToList();

                return Json(new { users });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchstr"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Route("Comment/Mentions")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Executed on SQL server")]
        public async Task<JsonResult> Mentions(string searchstr)
        {
            using (var db = new ZapContext())
            {
                var users = await db.Users
                    .Where(u => u.Name.StartsWith(searchstr))
                    .Select(u => new {
                        id = u.Id,
                        value = u.Name 
                    })
                    .Take(10)
                    .ToListAsync().ConfigureAwait(true);

                return Json(new { success=true, users });
            }
        }

        /// <summary>
        /// This method returns the partial HTML view for a comment input box.
        /// </summary>
        /// <param name="commentId">The comment for which the reply input is intended for.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Comment/GetInputBox/{commentId}")]
        public async Task<PartialViewResult> GetInputBox(int commentId)
        {
            Response.AddHeader("X-Frame-Options", "DENY");
            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();
                var userProvileVer = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.ProfileImage.Version)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var commentVm = new CommentReplyInputViewModel()
                {
                    CommentId = commentId,
                    UserAppId = userAppId,
                    ProfileImageVersion = userProvileVer
                };

                return PartialView("_PartialCommentReplyInput", commentVm);
            }
        }

        /// <summary>
        /// Returns the server-rendered HTML for a reply view to comment on a post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Comment/PostReply/{postId}")]
        public async Task<PartialViewResult> PostComment(int postId)
        {
            Response.AddHeader("X-Frame-Options", "DENY");
            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();
                var userProvileVer = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.ProfileImage.Version)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var vm = new CommentReplyInputViewModel()
                {
                    PostId = postId,
                    UserAppId = userAppId,
                    ProfileImageVersion = userProvileVer
                };

                return PartialView("_PartialPostReplyInput", vm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetUserMentions()
        {
            XFrameOptionsDeny();
            using (var db = new ZapContext())
            {
                var usernames = db.Users.Select(u => u.Name).ToList();
                return Json(usernames, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult DeleteComment(int Id)
        {
            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                Comment comment = db.Comments.FirstOrDefault(cmt => cmt.CommentId == Id);
                if (comment == null)
                {
                    return Json(new { Success = false });
                }
                if (comment.UserId.AppId != userId)
                {
                    return Json(new { Success = false });
                }
                comment.IsDeleted = true;
                db.SaveChanges();
                return Json(new { Success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult UpdateComment(NewComment c)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false });
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var comment = db.Comments.FirstOrDefault(cmt => cmt.CommentId == c.CommentId);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Comment not found." });
                }
                if (comment.UserId.AppId != userId)
                {
                    return Json(new { success = false, message = "User does not have rights to edit comment." });
                }
                comment.Text = SanitizeCommentXSS(c.CommentContent.Replace("<p><br></p>", ""));
                comment.TimeStampEdited = DateTime.UtcNow;
                db.SaveChanges();
            }

            return Json(new
            {
                HTMLString = "",
                c.PostId,
                success = true,
            });
        }


        /// <summary>
        /// Add a comment
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> AddComment(NewComment c)
        {
            if (c == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Invalid parameter." });
            }

            // Check for empty comment
            if (c.CommentContent.Replace(" ", "") == "<p><br></p>")
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Empty comment." });
            }

            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, message = "Unable to verify logged in user" });
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(usr => usr.Settings)
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(true);

                if (user == null)
                {
                    if (Response != null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    return Json(new { success = false, message = "User not found in database" });
                }

                var post = await db.Posts
                    .Include(pst => pst.UserId)
                    .Include(pst => pst.UserId.Settings)
                    .FirstOrDefaultAsync(p => p.PostId == c.PostId)
                    .ConfigureAwait(true);

                if (post == null)
                {
                    if (Response != null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    return Json(new { success = false, message = "Post not found in DB." });
                }

                Comment parent = null;
                if (c.IsReply)
                {
                    parent = await db.Comments
                        .Include(cmt => cmt.Post)
                        .FirstOrDefaultAsync(cmt => cmt.CommentId == c.CommentId)
                        .ConfigureAwait(true);
                }

                Comment comment = CreateComment(c, user, post, parent);

                var postOwner = post.UserId;
                if (postOwner.Settings == null)
                {
                    postOwner.Settings = new UserSettings();
                }

                // This is the owner of the comment being replied to
                User commentOwner = null;

                if (c.IsReply)
                {
                    commentOwner = await db.Comments
                        .Where(cmt => cmt.CommentId == c.CommentId)
                        .Include(cmt=> cmt.UserId.ProfileImage)
                        .Select(cmt => cmt.UserId)
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(true);

                    if (commentOwner.Settings == null)
                    {
                        commentOwner.Settings = new UserSettings();
                    }
                }

                if (!c.IsReply)
                {
                    post.Comments.Add(comment);
                }

                if (!c.IsTest)
                {
                    db.Comments.Add(comment);
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }

                // Find user mentions
                try
                {
                    // This could just move into the OnComment event and get processed in background?
                    var doc = new HtmlDocument();
                    doc.LoadHtml(comment.Text);

                    if (doc.HasUserMention())
                    {
                        await eventService.OnUserMentionedInComment(comment.CommentId);
                    }
                }
                catch (Exception e)
                {
                    MailingService.SendErrorNotification(
                        title: "User comment error",
                        message: " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n comment: " + c.CommentContent + "\r\n user: " + userAppId);
                }

                if (!c.IsReply && !c.IsTest)
                {
                    await eventService.OnPostCommentAsync(comment.CommentId);
                }

                if (c.IsReply && !c.IsTest)
                {
                    await eventService.OnCommentReplyAsync(comment.CommentId);
                }

                // Render the comment to HTML
                // TODO: this will be replaced with returning json object instead of server-rendered HTML
                string CommentHTMLString = RenderPartialViewToString(
                    viewName: "_PartialCommentRenderVm", 
                    model: new PostCommentsViewModel() 
                    { 
                        PostId = c.PostId,
                        StartVisible = true, 
                        CommentId = comment.CommentId,
                        IsReply = comment.IsReply,
                        IsDeleted = comment.IsDeleted,
                        CommentVms = new List<PostCommentsViewModel>(),
                        NestLevel = 0,
                        ParentUserId = parent == null ? 0 : parent.UserId.Id,
                        UserId = comment.UserId.Id,
                        Score = comment.Score,
                        ParentUserName = parent == null ? "" : parent.UserId.Name,
                        ProfileImageVersion = comment.UserId.ProfileImage.Version,
                        Text = comment.Text,
                        TimeStamp = comment.TimeStamp,
                        TimeStampEdited = comment.TimeStampEdited,
                        UserAppId = comment.UserId.AppId,
                        UserName = comment.UserId.Name,
                        ViewerDownvoted = false,
                        ViewerIgnoredUser = false,
                        ParentCommentId = parent == null ? 0 : parent.CommentId,
                    });

                return Json(new
                {
                    HTMLString = CommentHTMLString,
                    c.PostId,
                    success = true,
                    comment.IsReply,
                    comment.CommentId,
                });
            }
        }

        /// <summary>
        /// [ ] TODO: update the view model
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="commentId"></param>
        /// <param name="nestLevel"></param>
        /// <param name="rootshown"></param>
        /// <param name="render"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> LoadMoreComments(int postId, int? commentId, int? nestLevel, string rootshown, bool render = true)
        {
            using (var db = new ZapContext())
            {
                var post = await db.Posts
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.PostId == postId).ConfigureAwait(true);

                if (post == null)
                {
                    return HttpNotFound("Post not found");
                }

                var comment = await db.Comments
                    .Include(c => c.Post)
                    .Include(c => c.Post.Comments)
                    .FirstOrDefaultAsync(c => c.CommentId == commentId).ConfigureAwait(true);

                if (comment == null && commentId != null)
                {
                    return HttpNotFound("Comment not found");
                }

                if (rootshown == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "rootshown missing");
                }

                var shown = rootshown.Split(';').Select(s => Convert.ToInt64(s, CultureInfo.InvariantCulture)).ToList();

                // these are the comments we will show
                var commentIds = post.Comments.Where(c => !shown.Contains(c.CommentId))
                    .Where(c => !c.IsReply)
                    .OrderByDescending(c => c.Score)
                    .ThenByDescending(c => c.TimeStamp)
                    .Select(c => c.CommentId)
                    .ToList();

                string CommentHTMLString = "";
                List<PostCommentsViewModel> comments = new List<PostCommentsViewModel>();

                foreach (var cid in commentIds.Take(3)) // Comments in 3's
                {
                    var cmt = await db.Comments
                    .Include(c => c.Post)
                    .Include(c => c.Post.Comments)
                    .Include(c => c.Post.Comments.Select(cm => cm.Parent))
                    .Include(c => c.UserId)
                    .Include(c => c.VotesUp)
                    .Include(c => c.VotesDown)
                    .Include(c => c.Parent)
                    .Include(c => c.Parent.UserId)
                    .FirstOrDefaultAsync(c => c.CommentId == cid).ConfigureAwait(true);

                    if (cmt == null)
                    {
                        return Json(new
                        {
                            success = true,
                            more = false,
                            HTMLString = ""
                        });
                    }

                    comments.Add(new PostCommentsViewModel()
                    {
                        PostId = postId,
                        StartVisible = true,
                        CommentId = cmt.CommentId,
                        IsReply = cmt.IsReply,
                        IsDeleted = cmt.IsDeleted,
                        CommentVms = new List<PostCommentsViewModel>(),
                        NestLevel = 0,
                        ParentUserId = comment == null ? 0 : comment.UserId.Id,
                        UserId = cmt.UserId.Id,
                        Score = cmt.Score,
                        ParentUserName = comment == null ? "" : comment.UserId.Name,
                        ProfileImageVersion = cmt.UserId.ProfileImage.Version,
                        Text = cmt.Text,
                        TimeStamp = cmt.TimeStamp,
                        TimeStampEdited = cmt.TimeStampEdited,
                        UserAppId = cmt.UserId.AppId,
                        UserName = cmt.UserId.Name,
                        ViewerDownvoted = false,
                        ViewerIgnoredUser = false,
                        ParentCommentId = comment == null ? 0 : comment.CommentId,
                    });

                    if (render)
                    {
                        // Render the comment to be inserted to HTML
                        string aCommentHTMLString = RenderPartialViewToString(
                            viewName: "_PartialCommentRenderVm",
                            model: new PostCommentsViewModel()
                            {
                                PostId = postId,
                                StartVisible = true,
                                CommentId = cmt.CommentId,
                                IsReply = cmt.IsReply,
                                IsDeleted = cmt.IsDeleted,
                                CommentVms = new List<PostCommentsViewModel>(),
                                NestLevel = 0,
                                ParentUserId = comment == null ? 0 : comment.UserId.Id,
                                UserId = cmt.UserId.Id,
                                Score = cmt.Score,
                                ParentUserName = comment == null ? "" : comment.UserId.Name,
                                ProfileImageVersion = cmt.UserId.ProfileImage.Version,
                                Text = cmt.Text,
                                TimeStamp = cmt.TimeStamp,
                                TimeStampEdited = cmt.TimeStampEdited,
                                UserAppId = cmt.UserId.AppId,
                                UserName = cmt.UserId.Name,
                                ViewerDownvoted = false,
                                ViewerIgnoredUser = false,
                                ParentCommentId = comment == null ? 0 : comment.CommentId,
                            });
                        CommentHTMLString += aCommentHTMLString;
                    }
                    shown.Add(cmt.CommentId);
                }
                
                return Json(new
                {
                    success = true,
                    shown = String.Join(";", shown),
                    hasMore = commentIds.Count > 3,
                    HTMLString = CommentHTMLString,
                    comments = comments
                });
            }
        }

        private static Comment CreateComment(NewComment c, User user, Post post, Comment parent)
        {
            // Sanitize for XSS
            string commentText = c.CommentContent;
            string sanitizedComment = SanitizeCommentXSS(commentText.Replace("<p><br></p>", ""));

            return new Comment()
            {
                Parent = parent,
                IsReply = c.IsReply,
                UserId = user,
                Text = sanitizedComment,
                TimeStamp = DateTime.UtcNow,
                Post = post,
                Score = 0,
                TotalEarned = 0.0,
                VotesUp = new List<User>(),
                VotesDown = new List<User>(),
                IsDeleted = c.IsDeleted,
            };
        }

        private static string SanitizeCommentXSS(string commentText)
        {
            // Fix for nasty inject with odd brackets
            byte[] bytes = Encoding.Unicode.GetBytes(commentText);
            commentText = Encoding.Unicode.GetString(bytes);

            var sanitizer = new Ganss.XSS.HtmlSanitizer(
                allowedCssProperties: new[] { "color", "display", "text-align", "font-size", "margin-top", "margin-right", "margin-bottom", "margin-left", "margin", "float", "width" }
                //allowedCssClasses: new[] { "badge", "badge-info", "userhint", "blockquote", "img-fluid" }
                );

            sanitizer.AllowedTags.Remove("button");
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("data-index");
            sanitizer.AllowedAttributes.Add("data-denotation-char");
            sanitizer.AllowedAttributes.Add("data-id");
            sanitizer.AllowedAttributes.Add("data-value");
            sanitizer.AllowedAttributes.Add("contenteditable");
            sanitizer.AllowedAttributes.Remove("id");

            var sanitizedComment = sanitizer.Sanitize(commentText);
            return sanitizedComment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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
