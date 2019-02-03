using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Services;
using System.Data.Entity;
using HtmlAgilityPack;
using zapread.com.Models.Database;

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

        [HttpGet]
        public async Task<PartialViewResult> GetInputBox(int id)
        {
            using (var db = new ZapContext())
            {
                Comment comment = await db.Comments
                    .Include(cmt => cmt.Post)
                    .FirstOrDefaultAsync(cmt => cmt.CommentId == id);
                return PartialView("_PartialCommentReplyInput", comment);
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

            using(var db = new ZapContext())
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

        [HttpPost]
        public ActionResult UpdateComment(NewComment c)
        {
            if (!ModelState.IsValid)
            {
                return Json(new{ Success = false });
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var comment = db.Comments.FirstOrDefault(cmt => cmt.CommentId == c.CommentId);
                if (comment == null)
                {
                    return Json(new { Success = false });
                }
                if (comment.UserId.AppId != userId)
                {
                    return Json(new { Success = false });
                }
                comment.Text = c.CommentContent;
                db.SaveChanges();
            }

            return this.Json(new
            {
                HTMLString = "",
                c.PostId,
                Success = true,
            });
        }

        [HttpPost]
        public async Task<ActionResult> AddComment(NewComment c)
        {
            // Check for empty comment

            if (c.CommentContent.Replace(" ", "") == "<p><br></p>")
            {
                return Json(new
                {
                    success = false,
                    message = "Error: Empty comment.",
                    c.PostId,
                    c.IsReply,
                    c.CommentId,
                });
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                await EnsureUserExists(userId, db);
                var user = db.Users
                    .Include(usr => usr.Settings)
                    .Where(u => u.AppId == userId).First();

                var post = db.Posts
                    .Include(pst => pst.UserId)
                    .Include(pst => pst.UserId.Settings)
                    .FirstOrDefault(p => p.PostId == c.PostId);

                if (post == null)
                {
                    return this.Json(new
                    {
                        HTMLString = "",
                        c.PostId,
                        Success = false,
                        c.CommentId
                    });
                }
                Comment parent = null;
                if (c.IsReply)
                {
                    parent = db.Comments.Include(cmt => cmt.Post).FirstOrDefault(cmt => cmt.CommentId == c.CommentId);
                }

                Comment comment = new Comment()
                {
                    //CommentId = 1,
                    Parent = parent,
                    IsReply = c.IsReply,
                    UserId = user,
                    Text = c.CommentContent,
                    TimeStamp = DateTime.UtcNow,
                    Post = post,
                    Score = 0,
                    TotalEarned = 0.0,
                    VotesUp = new List<User>(),
                    VotesDown = new List<User>(),
                };

                var postOwner = post.UserId;
                if (postOwner.Settings == null)
                {
                    postOwner.Settings = new UserSettings();
                }

                User commentOwner = null;

                if (c.IsReply)
                {
                    commentOwner = db.Comments.FirstOrDefault(cmt => cmt.CommentId == c.CommentId).UserId;
                    if (commentOwner.Settings == null)
                    {
                        commentOwner.Settings = new UserSettings();
                    }
                }

                db.Comments.Add(comment);
                if (!c.IsReply)
                {
                    post.Comments.Add(comment);
                }
                await db.SaveChangesAsync();

                // Find user mentions

                var doc = new HtmlDocument();
                doc.LoadHtml(c.CommentContent);
                var spans = doc.DocumentNode.SelectNodes("//span");

                try
                {
                    if (spans != null)
                    {
                        foreach (var s in spans)
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

                                        var message = new UserMessage()
                                        {
                                            TimeStamp = DateTime.Now,
                                            Title = "You were mentioned in a comment by <a href='" + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = user.Name }) + "'>" + user.Name + "</a>",
                                            Content = c.CommentContent,
                                            CommentLink = comment,
                                            IsDeleted = false,
                                            IsRead = false,
                                            To = mentioneduser,
                                            PostLink = post,
                                            From = user,
                                        };

                                        mentioneduser.Messages.Add(message);

                                        await db.SaveChangesAsync();

                                        // Send Email
                                        if (mentioneduser.Settings == null)
                                        {
                                            mentioneduser.Settings = new UserSettings();
                                        }

                                        if (mentioneduser.Settings.NotifyOnMentioned)
                                        {
                                            var cdoc = new HtmlDocument();
                                            cdoc.LoadHtml(c.CommentContent);
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
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n comment: " + c.CommentContent + "\r\n user: " + userId,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User comment error",
                    });
                }

                // Only send messages if not own post.
                if (!c.IsReply && (postOwner.AppId != user.AppId))
                {
                    // Add Alert
                    if (postOwner.Settings == null)
                    {
                        postOwner.Settings = new UserSettings();
                    }

                    if (postOwner.Settings.AlertOnOwnPostCommented)
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
                        postOwner.Alerts.Add(alert);
                        await db.SaveChangesAsync();
                    }

                    // Send Email
                    if (postOwner.Settings.NotifyOnOwnPostCommented)
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

                        string ownerEmail = UserManager.FindById(postOwner.AppId).Email;
                        MailingService.Send(user: "Notify",
                            message: new UserEmailModel()
                            {
                                Subject = "New comment on your post: " + post.PostTitle,
                                Body = "From: " + user.Name + "<br/> " + commentContent + "<br/><br/>Go to <a href='http://www.zapread.com/Post/Detail/" + post.PostId.ToString() + "'>post</a> at <a href='http://www.zapread.com'>zapread.com</a>",
                                Destination = ownerEmail,
                                Email = "",
                                Name = "ZapRead.com Notify"
                            });
                    }
                }

                if (c.IsReply && commentOwner.AppId != user.AppId )
                {
                    var message = new UserMessage()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "New reply to your comment in post: <a href='" + Url.Action(actionName:"Detail", controllerName: "Post", routeValues: new { id = post.PostId }) + "'>" + (post.PostTitle != null ? post.PostTitle : "Post") + "</a>",
                        Content = c.CommentContent,
                        CommentLink = comment,
                        IsDeleted = false,
                        IsRead = false,
                        To = commentOwner,
                        PostLink = post,
                        From = user,
                    };

                    commentOwner.Messages.Add(message);
                    await db.SaveChangesAsync();

                    if (commentOwner.Settings == null)
                    {
                        commentOwner.Settings = new UserSettings();
                    }

                    if (commentOwner.Settings.NotifyOnOwnCommentReplied)
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

                        string ownerEmail = UserManager.FindById(commentOwner.AppId).Email;
                        MailingService.Send(user: "Notify",
                            message: new UserEmailModel()
                            {
                                Subject = "New reply to your comment in post: " + post.PostTitle,
                                Body = "From: <a href='http://www.zapread.com/user/" + user.Name.ToString() + "'>" + user.Name + "</a>" 
                                    + "<br/> " + commentContent 
                                    + "<br/><br/>Go to <a href='http://www.zapread.com/Post/Detail/" + post.PostId.ToString() + "'>"+ (post.PostTitle != null ? post.PostTitle : "Post") + "</a> at <a href='http://www.zapread.com'>zapread.com</a>",
                                Destination = ownerEmail,
                                Email = "",
                                Name = "ZapRead.com Notify"
                            });
                    }
                }

                string CommentHTMLString = RenderPartialViewToString("_PartialCommentRender", new PostCommentsViewModel() { Comment = comment, Comments = new List<Comment>() });

                return this.Json(new
                {
                    HTMLString = CommentHTMLString,
                    c.PostId,
                    success = true,
                    IsReply = c.IsReply,
                    c.CommentId,
                });
            }
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