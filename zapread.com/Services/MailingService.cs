using System;
using System.Net;
using System.Linq;
using System.Net.Mail;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using zapread.com.Database;
using zapread.com.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using zapread.com.Controllers;
using System.Web.Mvc;
using Hangfire;
using zapread.com.Models.Database;
using System.Globalization;
using System.Web;
using System.IO;
using System.Web.Routing;
using System.Web.Hosting;
using zapread.com.Models.Email;

namespace zapread.com.Services
{
    /// <summary>
    /// Background jobs for mailer (to be used with Hangfire)
    /// </summary>
    public class MailingService
    {
        private static Postal.EmailViewRenderer getRenderer()
        {
            var engines = new ViewEngineCollection();
            var assembiles = AppDomain.CurrentDomain.GetAssemblies();
            var zr = assembiles.SingleOrDefault(assembly => assembly.GetName().Name == "zapread.com");
            engines.Add(new Postal.ResourceRazorViewEngine(
                viewSourceAssembly: zr,
                viewPathRoot: "zapread.com.Views.Mailer"
                ));
            var emailViewRenderer = new Postal.EmailViewRenderer(engines);
            return emailViewRenderer;
        }

        private static string renderEmail(Postal.Email email, string baseUriString = "https://www.zapread.com/")
        {
            var emailViewRenderer = getRenderer();
            string HTMLString = emailViewRenderer.Render(email);

            // premailer cleanup
            PreMailer.Net.InlineResult result;
            var baseUri = new Uri(baseUriString);
            result = PreMailer.Net.PreMailer.MoveCssInline(
                baseUri: baseUri,
                html: HTMLString,
                removeComments: true,
                removeStyleElements: true,
                stripIdAndClassAttributes: true
                );

            var cleanHTMLString = result.Html;
            return cleanHTMLString;
        }

        /// <summary>
        /// Background mailer for test page
        /// </summary>
        /// <returns></returns>
        public bool BackgroundMailTestPage()
        {
            var email = new Models.Email.TestEmail()
            {
                To = "Nobody",
                Comment = "Test"
            };

            var emailContent = renderEmail(email);

            var emailDestination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];

            SendI(new UserEmailModel()
            {
                Destination = emailDestination,
                Body = emailContent,
                Email = "",
                Name = "zapread.com",
                Subject = "TestMail",
            }, "Notify", true);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public string GenerateMailPostCommentHTML(long commentId)
        {
            using (var db = new ZapContext())
            {
                var commentInfo = db.Comments
                    .Where(cmt => cmt.CommentId == commentId)
                    .Select(c => new PostCommentEmail()
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
                    .FirstOrDefault();

                var emailContent = renderEmail(commentInfo);

                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public string GenerateMailPostCommentReplyHTML(long commentId)
        {
            using (var db = new ZapContext())
            {
                var commentInfo = db.Comments
                    .Where(cmt => cmt.CommentId == commentId)
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
                    .FirstOrDefault();

                var emailContent = renderEmail(commentInfo);

                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public string GenerateMailNewPostHTML(int postId)
        {
            using (var db = new ZapContext())
            {
                var postInfo = db.Posts
                    .Where(p => p.PostId == postId)
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

                var emailContent = renderEmail(postInfo);

                return emailContent;
            }
        }

        /// <summary>
        /// Handle the mailing of notifications that there was a new comment on a post
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public string MailPostComment(long commentId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var emailContent = GenerateMailPostCommentHTML(commentId);

                if (isTest)
                {
                    var emailDestination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];
                    
                    SendI(new UserEmailModel()
                    {
                        Destination = emailDestination,
                        Body = emailContent,
                        Email = "",
                        Name = "zapread.com",
                        Subject = "New Comment on Post",
                    }, "Notify", true);
                    return emailContent;
                }

                // Send to post author and followers ...
                var postInfo = db.Comments
                    .Where(c => c.CommentId == commentId)
                    .Select(c => new
                    {
                        UserAppId = c.UserId.AppId,
                        c.Post.UserId.Settings.NotifyOnOwnPostCommented,
                        c.Post.UserId.AppId,
                        FollowerAppIds = c.Post.FollowedByUsers.Select(u => u.AppId)
                    })
                    .FirstOrDefault();

                // Post Author Notify
                if (postInfo != null
                    && postInfo.AppId != postInfo.UserAppId // Don't send to author if author commented on own post
                    && postInfo.NotifyOnOwnPostCommented) // Only send if author wants to be notified
                {
                    using (var appDB = new ApplicationDbContext()) // Tie into user database in order to get user emails
                    {
                        using (var userStore = new UserStore<ApplicationUser>(appDB))
                        {
                            using (var userManager = new ApplicationUserManager(userStore))
                            {
                                string postAuthorEmail = userManager.FindById(postInfo.AppId).Email;
                                SendI(new UserEmailModel()
                                {
                                    Destination = postAuthorEmail,
                                    Body = emailContent,
                                    Email = "",
                                    Name = "zapread.com",
                                    Subject = "New comment on your post",
                                }, "Notify", true);
                            }
                        }
                    }
                }

                if (postInfo != null 
                    && postInfo.FollowerAppIds.Any())
                {
                    // Tie into user database in order to get user emails
                    using (var appDB = new ApplicationDbContext())
                    {
                        using (var userStore = new UserStore<ApplicationUser>(appDB))
                        {
                            using (var userManager = new ApplicationUserManager(userStore))
                            {
                                foreach (var followerAppId in postInfo.FollowerAppIds)
                                {
                                    string postFollowerEmail = userManager.FindById(followerAppId).Email;
                                    SendI(new UserEmailModel()
                                    {
                                        Destination = postFollowerEmail,
                                        Body = emailContent,
                                        Email = "",
                                        Name = "zapread.com",
                                        Subject = "New comment on a post you are following",
                                    }, "Notify", true);
                                }
                            }
                        }
                    }
                }
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest"></param>
        /// <returns>email content as HTML</returns>
        public string MailPostCommentReply(long commentId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var emailContent = GenerateMailPostCommentReplyHTML(commentId);

                if (isTest)
                {
                    var emailDestination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];

                    SendI(new UserEmailModel()
                    {
                        Destination = emailDestination,
                        Body = emailContent,
                        Email = "",
                        Name = "zapread.com",
                        Subject = "New reply to your comment",
                    }, "Notify", true);
                    return emailContent;
                }

                return emailContent;
            }
        }

        /// <summary>
        /// This mailer should be able to run on HangFire without an HttpContext
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> TestMailer()
        {
            var emailViewRenderer = getRenderer();
            var testEmail = new TestEmail() { 
                Comment = "Test"
            };
            string HTMLString = emailViewRenderer.Render(testEmail);

            // premailer cleanup
            PreMailer.Net.InlineResult result;
            var baseUri = new Uri("https://www.zapread.com/");
            result = PreMailer.Net.PreMailer.MoveCssInline(
                baseUri: baseUri,
                html: HTMLString,
                removeComments: true,
                removeStyleElements: true,
                stripIdAndClassAttributes: true
                );

            var cleanHTMLString = result.Html;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Sends emails out to followers of the supplied postId
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="postBody"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public bool MailNewPostToFollowers(int postId, string postBody)
        {
            using (var db = new ZapContext())
            {
                var postInfo = db.Posts
                    .Where(p => p.PostId == postId)
                    .Select(p => new
                    {
                        authorUserId = p.UserId.Id,
                        p.UserId.Name,
                        post = p
                    })
                    .FirstOrDefault();
                
                if (postInfo == null) return false;

                var followUsers = db.Users
                    .Where(u => u.Id == postInfo.authorUserId)
                    .SelectMany(u => u.Followers)
                    .Select(u => new
                    {
                        u.Id,
                        u.AppId,
                        u.Settings.NotifyOnNewPostSubscribedUser,
                        u.Settings.AlertOnNewPostSubscribedUser,
                        user = u,
                    }).ToList();

                if (followUsers.Any())
                {
                    // Tie into user database in order to get user emails
                    using (var appDB = new ApplicationDbContext())
                    {
                        using (var userStore = new UserStore<ApplicationUser>(appDB))
                        {
                            using (var userManager = new ApplicationUserManager(userStore))
                            {
                                foreach (var follower in followUsers)
                                {
                                    // Add Alert
                                    if (follower.AlertOnNewPostSubscribedUser) 
                                    {
                                        var alert = new UserAlert()
                                        {
                                            TimeStamp = DateTime.Now,
                                            Title = "New post by a user you are following: <a href='/User/" + Uri.EscapeDataString(postInfo.Name) + "'>" + postInfo.Name + "</a>",
                                            Content = "",
                                            IsDeleted = false,
                                            IsRead = false,
                                            To = follower.user,
                                            PostLink = postInfo.post,                                            
                                        };

                                        follower.user.Alerts.Add(alert);

                                        if (follower.user.Settings == null)
                                        {
                                            follower.user.Settings = new UserSettings();
                                        }
                                    }

                                    //Email
                                    if (follower.NotifyOnNewPostSubscribedUser)
                                    {
                                        string followerEmail = userManager.FindById(follower.AppId).Email;
                                        var key = System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"];
                                        var userUnsubscribeId = CryptoService.EncryptString(key, follower.AppId);

                                        postBody = postBody.Replace("[userUnsubscribeId]", userUnsubscribeId);

                                        BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                                            new UserEmailModel()
                                            {
                                                Destination = followerEmail,
                                                Body = postBody,
                                                Email = "",
                                                Name = "zapread.com",
                                                Subject = "New post by user you are following",
                                            }, "Notify", true));
                                    }
                                }

                                db.SaveChanges();
                            }
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="useSSL"></param>
        /// <returns></returns>
        public bool SendI(UserEmailModel message, string user = "Accounts", bool useSSL=true)
        {
            // Plug in your email service here to send an email.
            var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
            var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"]);
            var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
            var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];

            var mmessage = new MailMessage();
            mmessage.To.Add(new MailAddress(message.Destination));
            mmessage.From = new MailAddress(emailuser);  // replace with valid value
            mmessage.Subject = message.Subject;
            mmessage.Body = message.Body;
            mmessage.IsBodyHtml = true;
            if (message.Email != null && message.Email != "")
            {
                mmessage.ReplyToList.Add(new MailAddress(message.Email));
            }

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = emailuser,
                    Password = emailpass
                };
                smtp.Credentials = credential;
                smtp.Host = emailhost;
                smtp.Port = emailport;
                smtp.EnableSsl = useSSL;
                smtp.Send(mmessage);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="useSSL"></param>
        /// <returns></returns>
        public static bool Send(UserEmailModel message, string user = "Accounts", bool useSSL=true)
        {
            // Plug in your email service here to send an email.
            var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
            var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"], CultureInfo.InvariantCulture);
            var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
            var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];

            var mmessage = new MailMessage();
            mmessage.To.Add(new MailAddress(message.Destination));
            mmessage.From = new MailAddress(emailuser);  // replace with valid value
            mmessage.Subject = message.Subject;
            mmessage.Body = message.Body;
            mmessage.IsBodyHtml = true;
            if (message.Email != null && message.Email != "")
            {
                mmessage.ReplyToList.Add(new MailAddress(message.Email));
            }

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = emailuser,
                    Password = emailpass
                };
                smtp.Credentials = credential;
                smtp.Host = emailhost;
                smtp.Port = emailport;
                smtp.EnableSsl = useSSL;
                smtp.Send(mmessage);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool SendErrorNotification(string title, string message)
        {
            // Send error
            return Send(new UserEmailModel()
            {
                Body = message,
                Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                Email = "",
                Name = "zapread.com Exception",
                Subject = title,
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="useSSL"></param>
        /// <returns></returns>
        public static async Task<bool> SendAsync(UserEmailModel message, string user = "Accounts", bool useSSL = true)
        {
            // Plug in your email service here to send an email.
            var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
            var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"]);
            var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
            var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];

            var mmessage = new MailMessage();
            mmessage.To.Add(new MailAddress(message.Destination));
            mmessage.From = new MailAddress(emailuser);  // replace with valid value
            mmessage.Subject = message.Subject;
            mmessage.Body = message.Body;
            mmessage.IsBodyHtml = true;
            if (message.Email != null && message.Email != "")
            {
                mmessage.ReplyTo = new MailAddress(message.Email);
            }

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = emailuser,
                    Password = emailpass
                };
                smtp.Credentials = credential;
                smtp.Host = emailhost;
                smtp.Port = emailport;
                smtp.EnableSsl = useSSL;
                await smtp.SendMailAsync(mmessage).ConfigureAwait(true);
            }
            return true;
        }
    }
}