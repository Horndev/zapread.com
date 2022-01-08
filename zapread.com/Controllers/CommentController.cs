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
    public class CommentController : Controller
    {
        private ApplicationUserManager _userManager;

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

        public class NewComment
        {
            [Required]
            [DataType(DataType.MultilineText)]
            [AllowHtml]
            public string CommentContent { get; set; }

            public int PostId { get; set; }

            public int CommentId { get; set; }

            public bool IsReply { get; set; }

            public bool IsDeleted { get; set; }

            public bool IsTest { get; set; }
        }

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
        /// <param name="id">The comment for which the reply input is intended for.</param>
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

        [HttpGet]
        public ActionResult GetUserMentions()
        {
            using (var db = new ZapContext())
            {
                var usernames = db.Users.Select(u => u.Name).ToList();
                return Json(usernames, JsonRequestBehavior.AllowGet);
            }
        }

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
                comment.Text = SanitizeCommentXSS(c.CommentContent);
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
                return Json(new { success = false, Message = "Invalid parameter." });
            }

            // Check for empty comment
            if (c.CommentContent.Replace(" ", "") == "<p><br></p>")
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, Message = "Empty comment." });
            }

            var userAppId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(usr => usr.Settings)
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (user == null)
                {
                    if (Response != null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    return Json(new { success = false, Message = "User not found in DB." });
                }

                var post = await db.Posts
                    .Include(pst => pst.UserId)
                    .Include(pst => pst.UserId.Settings)
                    .FirstOrDefaultAsync(p => p.PostId == c.PostId).ConfigureAwait(true);

                if (post == null)
                {
                    if (Response != null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    return Json(new { success = false, Message = "Post not found in DB." });
                }

                Comment parent = null;
                if (c.IsReply)
                {
                    parent = db.Comments.Include(cmt => cmt.Post).FirstOrDefault(cmt => cmt.CommentId == c.CommentId);
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
                    commentOwner = db.Comments.FirstOrDefault(cmt => cmt.CommentId == c.CommentId).UserId;
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

                    // Done synchronously since we can get errors when trying to render post HTML later.
                    db.SaveChanges();
                }

                // Find user mentions
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(comment.Text);
                    var spans = doc.DocumentNode.SelectNodes("//span");
                    if (spans != null)
                    {
                        foreach (var s in spans)
                        {
                            if (!c.IsTest)
                                await NotifyUserMentioned(db, user, post, comment, s).ConfigureAwait(true);
                        }
                    }
                }
                catch (Exception e)
                {
                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n comment: " + c.CommentContent + "\r\n user: " + userAppId,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User comment error",
                    });
                }

                // Only send messages if not own post.
                if (!c.IsReply && (postOwner.AppId != user.AppId))
                {
                    if (!c.IsTest)
                        await NotifyPostOwnerOfComment(db, user, post, comment, postOwner).ConfigureAwait(true);
                }

                if (c.IsReply && commentOwner.AppId != user.AppId)
                {
                    if (!c.IsTest)
                        await NotifyCommentOwnerOfReply(db, user, post, comment, commentOwner).ConfigureAwait(true);
                }

                // Render the comment to be inserted to HTML
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
                    c.IsReply,
                    c.CommentId,
                });
            }
        }

        /// <summary>
        /// [ ] TODO: update the view model
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="commentId"></param>
        /// <param name="nestLevel"></param>
        /// <param name="rootshown"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> LoadMoreComments(int postId, int? commentId, int? nestLevel, string rootshown)
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

                // All the comments related to this post
                var postComments = await db.Posts
                    .Include(p => p.Group)
                    .Include(p => p.Comments)
                    .Include(p => p.Comments.Select(c => c.Parent))
                    .Include(p => p.Comments.Select(c => c.VotesUp))
                    .Include(p => p.Comments.Select(c => c.VotesDown))
                    .Include(p => p.Comments.Select(c => c.UserId))
                    .Where(p => p.PostId == postId)
                    .SelectMany(p => p.Comments)
                    .ToListAsync().ConfigureAwait(true);

                string CommentHTMLString = "";

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
                    shown.Add(cmt.CommentId);
                }

                return Json(new
                {
                    success = true,
                    shown = String.Join(";", shown),
                    hasMore = commentIds.Count > 3,
                    HTMLString = CommentHTMLString
                });
            }
        }

        private static Comment CreateComment(NewComment c, User user, Post post, Comment parent)
        {
            // Sanitize for XSS
            string commentText = c.CommentContent;
            string sanitizedComment = SanitizeCommentXSS(commentText);

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
                allowedCssProperties: new[] { "color", "display", "text-align", "font-size", "margin-right", "width" }
                //allowedCssClasses: new[] { "badge", "badge-info", "userhint", "blockquote", "img-fluid" }
                );

            sanitizer.AllowedTags.Remove("button");
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Remove("id");

            var sanitizedComment = sanitizer.Sanitize(commentText);
            return sanitizedComment;
        }

        private async Task NotifyUserMentioned(ZapContext db, User user, Post post, Comment comment, HtmlNode s)
        {
            if (s.Attributes.Count(a => a.Name == "class") > 0)
            {
                var cls = s.Attributes.FirstOrDefault(a => a.Name == "class");
                if (cls.Value.Contains("userhint"))
                {
                    var username = s.InnerHtml.Replace("@", "");
                    var mentioneduser = db.Users
                        .Include(usr => usr.Settings)
                        .FirstOrDefault(u => u.Name == username);

                    if (mentioneduser != null)
                    {
                        // Add Message
                        UserMessage message = CreateMentionedMessage(user, post, comment, mentioneduser);
                        mentioneduser.Messages.Add(message);
                        await db.SaveChangesAsync();

                        // Send Email
                        SendMentionedEmail(user, post, comment, mentioneduser);
                    }
                }
            }
        }

        private async Task NotifyPostOwnerOfComment(ZapContext db, User user, Post post, Comment comment, User postOwner)
        {
            // Add Alert
            if (postOwner.Settings == null)
            {
                postOwner.Settings = new UserSettings();
            }

            if (postOwner.Settings.AlertOnOwnPostCommented)
            {
                UserAlert alert = CreateCommentedAlert(user, post, comment, postOwner);
                postOwner.Alerts.Add(alert);
                await db.SaveChangesAsync().ConfigureAwait(true);
            }

            // Send Email
            if (postOwner.Settings.NotifyOnOwnPostCommented)
            {
                string subject = "New comment on your post: " + post.PostTitle;
                string ownerEmail = UserManager.FindById(postOwner.AppId).Email;

                var mailer = DependencyResolver.Current.GetService<MailerController>();
                mailer.ControllerContext = new ControllerContext(this.Request.RequestContext, mailer);

                await mailer.SendPostComment(comment.CommentId, ownerEmail, subject).ConfigureAwait(true);
            }
        }

        private async Task NotifyCommentOwnerOfReply(ZapContext db, User user, Post post, Comment comment, User commentOwner)
        {
            UserMessage message = CreateCommentRepliedMessage(user, post, comment, commentOwner);

            commentOwner.Messages.Add(message);
            await db.SaveChangesAsync().ConfigureAwait(true);

            if (commentOwner.Settings == null)
            {
                commentOwner.Settings = new UserSettings();
            }

            // Send Email
            if (commentOwner.Settings.NotifyOnOwnCommentReplied)
            {
                string subject = "New reply to your comment in post: " + post.PostTitle;
                string ownerEmail = UserManager.FindById(commentOwner.AppId).Email;

                var mailer = DependencyResolver.Current.GetService<MailerController>();
                mailer.ControllerContext = new ControllerContext(this.Request.RequestContext, mailer);

                await mailer.SendPostCommentReply(comment.CommentId, ownerEmail, subject).ConfigureAwait(true);
            }
        }

        private UserMessage CreateCommentRepliedMessage(User user, Post post, Comment comment, User commentOwner)
        {
            return new UserMessage()
            {
                TimeStamp = DateTime.Now,
                Title = "New reply to your comment in post: <a href='" + Url.Action(actionName: "Detail", controllerName: "Post", routeValues: new { id = post.PostId }) + "'>" + (post.PostTitle != null ? post.PostTitle : "Post") + "</a>",
                Content = comment.Text,
                CommentLink = comment,
                IsDeleted = false,
                IsRead = false,
                To = commentOwner,
                PostLink = post,
                From = user,
            };
        }

        private UserAlert CreateCommentedAlert(User user, Post post, Comment comment, User postOwner)
        {
            var alert = new UserAlert()
            {
                TimeStamp = DateTime.Now,
                Title = "New comment on your post: <a href=" + @Url.Action("Detail", "Post", new { post.PostId }) + ">" + post.PostTitle + "</a>",
                Content = "From: <a href='" + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = user.Name }) + "'>" + user.Name + "</a>",//< br/> " + c.CommentContent,
                CommentLink = comment,
                IsDeleted = false,
                IsRead = false,
                To = postOwner,
                PostLink = post,
            };
            return alert;
        }

        private void SendMentionedEmail(User user, Post post, Comment comment, User mentioneduser)
        {
            if (mentioneduser.Settings == null)
            {
                mentioneduser.Settings = new UserSettings();
            }

            if (mentioneduser.Settings.NotifyOnMentioned)
            {
                var cdoc = new HtmlDocument();
                cdoc.LoadHtml(comment.Text);
                var baseUri = new Uri("https://www.zapread.com/");
                var imgs = cdoc.DocumentNode.SelectNodes("//img/@src");
                if (imgs != null)
                {
                    foreach (var item in imgs)
                    {
                        item.SetAttributeValue("src", new Uri(baseUri, item.GetAttributeValue("src", "")).AbsoluteUri);
                    }
                }
                string commentContent = cdoc.DocumentNode.OuterHtml;

                string mentionedEmail = UserManager.FindById(mentioneduser.AppId).Email;
                MailingService.Send(user: "Notify",
                    message: new UserEmailModel()
                    {
                        Subject = "New mention in comment",
                        Body = "From: " + user.Name + "<br/> " + commentContent + "<br/><br/>Go to <a href='http://www.zapread.com/Post/Detail/" + post.PostId.ToString() + "'>post</a> at <a href='http://www.zapread.com'>zapread.com</a>",
                        Destination = mentionedEmail,
                        Email = "",
                        Name = "ZapRead.com Notify"
                    });
            }
        }

        private UserMessage CreateMentionedMessage(User user, Post post, Comment comment, User mentioneduser)
        {
            return new UserMessage()
            {
                TimeStamp = DateTime.Now,
                Title = "You were mentioned in a comment by <a href='" + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = user.Name }) + "'>" + user.Name + "</a>",
                Content = comment.Text,
                CommentLink = comment,
                IsDeleted = false,
                IsRead = false,
                To = mentioneduser,
                PostLink = post,
                From = user,
            };
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

        private async Task EnsureUserExists(string userId, ZapContext db)
        {
            if (db.Users.Where(u => u.AppId == userId).Count() == 0)
            {
                // no user entry
                User u = new User()
                {
                    AboutMe = "Nothing to tell.",
                    AppId = userId,
                    Name = User.Identity.Name,
                    ProfileImage = new UserImage(),
                    ThumbImage = new UserImage(),
                    Funds = new UserFunds(),
                    Settings = new UserSettings(),
                    DateJoined = DateTime.UtcNow,
                };
                db.Users.Add(u);
                await db.SaveChangesAsync();
            }
        }
    }
}