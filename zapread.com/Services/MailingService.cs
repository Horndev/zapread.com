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

namespace zapread.com.Services
{
    /// <summary>
    /// Background jobs for mailer (to be used with Hangfire)
    /// </summary>
    public class MailingService
    {
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

        public static string ComposeEmail(string body, string header = "", string footer = "")
        {
            header = "";
            footer = "";
            return header + body + footer;
        }

        public bool SendEmail(string emailTo, string subject, string body, string user = "Accounts", bool useSSL=true)
        {
            // Plug in your email service here to send an email.
            var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
            var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"]);
            var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
            var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];

            var mmessage = new MailMessage();
            mmessage.To.Add(new MailAddress(emailTo));
            mmessage.From = new MailAddress(emailuser);
            mmessage.Subject = subject;
            mmessage.Body = body;
            mmessage.IsBodyHtml = true;
            //if (message.Email != null && message.Email != "")
            //{
            //    mmessage.ReplyTo = new MailAddress(message.Email);
            //}

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

        public static bool Send(UserEmailModel message, string user = "Accounts", bool useSSL=true)
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