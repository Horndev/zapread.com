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
using HtmlAgilityPack;
using zapread.com.Models.Subscription;
using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using MailKit.Search;
using System.Text.RegularExpressions;
using zapread.com.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using MimeKit;

namespace zapread.com.Services
{
    /// <summary>
    /// Background jobs for mailer (to be used with Hangfire)
    /// 
    /// List of email types / events
    /// 
    /// [ ] New Post
    ///     [✓] Can Unsubscribe directly
    ///     [ ] Followers of User
    /// [ ] New Comment
    ///     [✓] Can Unsubscribe directly
    /// [ ] New Comment Reply
    ///     [✓] Can Unsubscribe directly
    /// [ ] New Chat
    ///     [✓] Can Unsubscribe directly
    /// [ ] User Alias changed
    /// [ ] User Mentioned in Comment
    ///     [✓] Can Unsubscribe directly
    /// [ ] User Mentioned in Post
    ///     [✓] Can Unsubscribe directly
    /// [ ] User Following
    ///     [ ] Can Unsubscribe directly
    /// 
    /// Issues
    /// [ ] User mentioned and comment / comment reply (receive only once?)  - move mention check into oncomment
    /// 
    /// </summary>
    public class MailingService
    {
        /// Email quote code from https://stackoverflow.com/questions/2385347/how-to-remove-the-quoted-text-from-an-email-and-only-show-the-new-text
        /// 
         
        /// general spacers for time and date 
        private static String spacers = "[\\s,/\\.\\-]";
        /// matches times 
        private static String timePattern = "(?:[0-2])?[0-9]:[0-5][0-9](?::[0-5][0-9])?(?:(?:\\s)?[AP]M)?";
        /// matches day of the week 
        private static String dayPattern = "(?:(?:Mon(?:day)?)|(?:Tue(?:sday)?)|(?:Wed(?:nesday)?)|(?:Thu(?:rsday)?)|(?:Fri(?:day)?)|(?:Sat(?:urday)?)|(?:Sun(?:day)?))";
        /// matches day of the month (number and st, nd, rd, th) 
        private static String dayOfMonthPattern = "[0-3]?[0-9]" + spacers + "*(?:(?:th)|(?:st)|(?:nd)|(?:rd))?";
        /// matches months (numeric and text) 
        private static String monthPattern = "(?:(?:Jan(?:uary)?)|(?:Feb(?:uary)?)|(?:Mar(?:ch)?)|(?:Apr(?:il)?)|(?:May)|(?:Jun(?:e)?)|(?:Jul(?:y)?)" + "|(?:Aug(?:ust)?)|(?:Sep(?:tember)?)|(?:Oct(?:ober)?)|(?:Nov(?:ember)?)|(?:Dec(?:ember)?)|(?:[0-1]?[0-9]))";
        /// matches years (only 1000's and 2000's, because we are matching emails) 
        private static String yearPattern = "(?:[1-2]?[0-9])[0-9][0-9]";
        /// matches a full date 
        private static String datePattern = "(?:" + dayPattern + spacers + "+)?(?:(?:" + dayOfMonthPattern + spacers + "+" + monthPattern + ")|" + "(?:" + monthPattern + spacers + "+" + dayOfMonthPattern + "))" + spacers + "+" + yearPattern;
        /// matches a date and time combo (in either order) 
        private static String dateTimePattern = "(?:" + datePattern + "[\\s,]*(?:(?:at)|(?:@))?\\s*" + timePattern + ")|" + "(?:" + timePattern + "[\\s,]*(?:on)?\\s*" + datePattern + ")";
        /// matches a leading line such as
        /// ----Original Message----
        /// or simply
        /// ------------------------
        private static String leadInLine = "-+\\s*(?:Original(?:\\sMessage)?)?\\s*-+\n";
        /// matches a header line indicating the date 
        private static String dateLine = "(?:(?:date)|(?:sent)|(?:time)):\\s*" + dateTimePattern + ".*\n";
        /// matches a subject or address line 
        private static String subjectOrAddressLine = "((?:from)|(?:subject)|(?:b?cc)|(?:to))|:.*\n";
        /// matches gmail style quoted text beginning, i.e.
        /// On Mon Jun 7, 2010 at 8:50 PM, Simon wrote:
        private static String gmailQuotedTextBeginning = "(On\\s+" + dateTimePattern + ".*wrote:\n)";
        private static Regex emailQuotedTextBeginningRegex = new Regex(pattern: "(?i)(?:(?:" + leadInLine + ")?" + "(?:(?:" + subjectOrAddressLine + ")|(?:" + dateLine + ")){2,6})|(?:" + gmailQuotedTextBeginning + ")", options: RegexOptions.Compiled);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CheckChatReplies()
        {
            var user = "Notify";
            var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
            var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"]);
            var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
            var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];
            var Emails = new List<Email>();
            using (var db = new ZapContext())
            {
                using (var client = new ImapClient())
                {
                    client.Connect(emailhost, 993, SecureSocketOptions.SslOnConnect);
                    client.Authenticate(emailuser, emailpass);
                    client.Inbox.Open(FolderAccess.ReadWrite);
                    var namespaces = client.PersonalNamespaces;
                    var folders = client.GetFolders(client.PersonalNamespaces[0]);
                    var chatDoneFolder = folders.Where(f => f.FullName == "ChatRepliesDone").FirstOrDefault();
                    var chatErrorFolder = folders.Where(f => f.FullName == "ChatRepliesError").FirstOrDefault();
                    var queryFrom = SearchQuery.ToContains("+");
                    var query = SearchQuery.BodyContains("chatReplyId:");
                    var chatReplyUids = client.Inbox.Search(query.Or(queryFrom));
                    foreach (var uid in chatReplyUids)
                    {
                        var message = client.Inbox.GetMessage(uid);
                        string userMessage = "";
                        if (message.HtmlBody != null)
                        {
                            // HTML message -> Extract text
                            var emailAsText = HtmlDocumentHelpers.ConvertToPlainText(message.HtmlBody);
                            userMessage = emailAsText;
                        }
                        else if (message.TextBody != null)
                        {
                            // Text message
                            userMessage = message.TextBody;
                        }
                        else
                        {
                            client.Inbox.MoveTo(uid: uid, destination: chatErrorFolder);
                        }

                        if (!String.IsNullOrEmpty(userMessage))
                        {
                            var m = emailQuotedTextBeginningRegex.Match(userMessage);
                            if (m.Success)
                            {
                                userMessage = userMessage.Substring(0, m.Index);
                            }

                            // Remove nuisance strings (should be in a DB or other configuration)
                            userMessage = userMessage.Replace("Sent from Mail for Windows", "");
                            // Do this twice, but should be a smarter way...
                            userMessage = userMessage.SanitizeXSS();
                            userMessage = userMessage.Replace("&nbsp;", " ");
                            userMessage = userMessage.Trim('\r', '\n'); // remove extra newlines
                            userMessage = userMessage.Trim(); // remove spaces
                            userMessage = userMessage.Trim('\r', '\n'); // remove extra newlines
                            userMessage = userMessage.Trim(); // remove spaces
                            // Check for images, extract - and make links
                            try
                            {
                                var images = Regex.Matches(userMessage, "\\[image: (.*)\\]+", RegexOptions.IgnoreCase);
                                if (images.Count > 0)
                                {
                                    foreach (Match imatch in images)
                                    {
                                        //[image: image.png] -> image.png
                                        var imageinfo = imatch.Groups[1].Value;
                                        var imagesrc = imageinfo.Split(';')[0].Replace("cid:", "");
                                        var image = (MimePart)message.BodyParts.Where(p => p.ContentId == imagesrc).First();
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            int maxwidth = 720;
                                            image.Content.DecodeTo(ms);
                                            Image img = Image.FromStream(ms);
                                            byte[] data;
                                            string contentType = "image/jpeg";
                                            if (img.Width > maxwidth)
                                            {
                                                // rescale if too large
                                                var scale = Convert.ToDouble(maxwidth) / Convert.ToDouble(img.Width);
                                                Bitmap thumb = ImageExtensions.ResizeImage(img, maxwidth, Convert.ToInt32(img.Height * scale));
                                                data = thumb.ToByteArray(ImageFormat.Jpeg);
                                            }
                                            else
                                            {
                                                data = img.ToByteArray(ImageFormat.Jpeg);
                                            }

                                            UserImage i = new UserImage()
                                            {Image = data, ContentType = contentType, };
                                            db.Images.Add(i);
                                            db.SaveChanges();
                                            userMessage = userMessage.Replace(imatch.Value, "<br/><img src=\"/i/" + CryptoService.IntIdToString(i.ImageId) + "\"><br/>");
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            // no biggie - but image won't go in.
                            }

                            var toAddress = message.To.Mailboxes.First().Address;
                            var replyId = toAddress.Replace("@zapread.com", "").Replace("notify+", "");
                            if (!String.IsNullOrEmpty(replyId))
                            {
                                var info = CryptoService.DecryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], replyId);
                                // msgInfo.SenderId + ":" + msgInfo.ToId + ":" + msgInfo.Id + ":" + SubscriptionTypes.NewChat
                                var parts = info.Split(':');
                                if (parts.Length >= 4)
                                {
                                    var fromId = Convert.ToInt32(parts[1]);
                                    var toId = Convert.ToInt32(parts[0]);
                                    var msgId = Convert.ToInt32(parts[2]);
                                    var msgType = parts[3];
                                    // Check to ensure reciever isn't blocking sender
                                    var userToInfo = db.Users.Where(u => u.Id == toId).Select(u => new
                                    {
                                    isBlocked = u.BlockingUsers.Select(bu => bu.Id).Contains(fromId), User = u
                                    }).FirstOrDefault();
                                    if (!userToInfo.isBlocked)
                                    {
                                        var userFrom = db.Users.FirstOrDefault(u => u.Id == fromId);
                                        var userTo = userToInfo.User; //db.Users.FirstOrDefault(u => u.Id == toId);
                                        var msg = new UserMessage()
                                        {IsPrivateMessage = true, Content = userMessage, From = userFrom, To = userTo, IsDeleted = false, IsRead = false, TimeStamp = DateTime.UtcNow, Title = "Private message from " + userFrom.Name + "", //" + sender.Name,
 };
                                        // This will add
                                        userTo.Messages = new List<UserMessage>()
                                        {msg};
                                        db.SaveChanges();
                                    //TODO: send update notification to live client
                                    }
                                    else
                                    {
                                        // Send a message to sender they are blocked
                                        BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                                        {Destination = message.From.Mailboxes.First().Address, Body = userToInfo.User.Name + " has blocked you from sending them chat messages.", Email = "notify+" + replyId + "@zapread.com", // This is the reply-to
 Name = "zapread.com", Subject = "Your message to " + userToInfo.User.Name + " was blocked", }, "Notify", true));
                                    }
                                }
                                else
                                {
                                    // Malformed
                                    client.Inbox.MoveTo(uid: uid, destination: chatErrorFolder);
                                }

                                // Move message into IMAP completed folder
                                client.Inbox.MoveTo(uid: uid, destination: chatDoneFolder);
                            }
                        }
                    // If failed to parse email - send notification back to sender that it failed and suggestions to retry.
                    // TODO
                    }

                    client.Disconnect(true);
                }
            }

            return true;
        }

        //public void TestMailReply()
        //{
        //    var user = "Notify";
        //    var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
        //    var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"]);
        //    var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
        //    var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];
        //}
        private string makeImagesFQDN(string emailContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(emailContent);
            var baseUri = new Uri("https://www.zapread.com/");
            var imgs = doc.DocumentNode.SelectNodes("//img/@src");
            if (imgs != null)
            {
                foreach (var item in imgs)
                {
                    item.SetAttributeValue("src", new Uri(baseUri, item.GetAttributeValue("src", "")).AbsoluteUri);
                }
            }

            string newContent = doc.DocumentNode.OuterHtml;
            return newContent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userAppId"></param>
        /// <returns></returns>
        public string GenerateMailWeeklySummary(string userAppId)
        {
            using (var db = new ZapContext())
            {
                var startTime = DateTime.Now - TimeSpan.FromDays(7);
                var startTimePrev = DateTime.Now - TimeSpan.FromDays(14);
                var refCode = db.Users.Where(u => u.AppId == userAppId).Select(u => u.ReferralCode).FirstOrDefault();
                if (refCode == null)
                {
                    var user = db.Users.Where(u => u.AppId == userAppId).FirstOrDefault();
                    if (user != null)
                    {
                        user.ReferralCode = CryptoService.GetNewRefCode();
                        refCode = user.ReferralCode;
                        db.SaveChanges();
                    }
                }

                var viewInfo = db.Users.Where(u => u.AppId == userAppId).Select(u => new WeeklySummaryEmail()
                {RefCode = refCode, TotalEarnedLastWeek = u.EarningEvents.Where(e => e.TimeStamp > startTimePrev && e.TimeStamp < startTime).Sum(e => ((double? )e.Amount)) ?? 0, TotalEarnedWeek = u.EarningEvents.Where(e => e.TimeStamp > startTime).Sum(e => ((double? )e.Amount)) ?? 0, TotalEarnedReferral = u.EarningEvents.Where(e => e.TimeStamp > startTime).Where(e => e.Type == 4).Sum(e => ((double? )e.Amount)) ?? 0, TotalEarnedPosts = u.EarningEvents.Where(e => e.TimeStamp > startTime).Where(e => e.OriginType == 0).Sum(e => ((double? )e.Amount)) ?? 0, TotalEarnedComments = u.EarningEvents.Where(e => e.TimeStamp > startTime).Where(e => e.OriginType == 1).Sum(e => ((double? )e.Amount)) ?? 0, TotalGroupPayments = u.EarningEvents.Where(e => e.TimeStamp > startTime).Where(e => e.Type == 1).Sum(e => ((double? )e.Amount)) ?? 0, TotalCommunityPayments = u.EarningEvents.Where(e => e.TimeStamp > startTime).Where(e => e.Type == 2).Sum(e => ((double? )e.Amount)) ?? 0, TotalPostsWeek = u.Posts.Where(p => p.TimeStamp > startTime).Count(), TotalCommentsWeek = u.Comments.Where(c => c.TimeStamp > startTime).Count(), TopGroups = u.EarningEvents.Where(e => e.TimeStamp > startTime).Where(e => e.Type == 1).GroupBy(e => e.OriginId).Select(e => new
                {
                GroupId = e.Key, Count = e.Count(), Amount = e.Sum(v => ((double? )v.Amount)) ?? 0, }).Join(db.Groups, e => e.GroupId, g => g.GroupId, (i, o) => new TopGroup()
                {GroupName = o.GroupName, GroupId = i.GroupId, AmountEarned = i.Amount}).OrderByDescending(c => c.AmountEarned).Take(3).ToList(), }).FirstOrDefault();
                var emailContent = renderEmail(viewInfo);
                // Make images resolve to zapread
                emailContent = makeImagesFQDN(emailContent);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userId"></param>
        /// <returns></returns>
        public string GenerateMailNewUserFollowing(int userId)
        {
            using (var db = new ZapContext())
            {
                var userInfo = db.Users.Where(u => u.Id == userId).Select(u => new NewUserFollowerEmail()
                {UserName = u.Name, UserAppId = u.AppId, ProfileImageVersion = u.ProfileImage.Version}).FirstOrDefault();
                var emailContent = renderEmail(userInfo);
                // Make images resolve to zapread
                emailContent = makeImagesFQDN(emailContent);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <returns></returns>
        public string GenerateMailPostCommentHTML(long commentId)
        {
            using (var db = new ZapContext())
            {
                var commentInfo = db.Comments.Where(cmt => cmt.CommentId == commentId).Select(c => new PostCommentEmail()
                {CommentId = c.CommentId, Score = c.Score, Text = c.Text, UserId = c.UserId.Id, UserName = c.UserId.Name, UserAppId = c.UserId.AppId, ProfileImageVersion = c.UserId.ProfileImage.Version, PostTitle = c.Post == null ? "" : c.Post.PostTitle, PostId = c.Post == null ? 0 : c.Post.PostId, }).FirstOrDefault();
                var emailContent = renderEmail(commentInfo);
                // Make images resolve to zapread
                emailContent = makeImagesFQDN(emailContent);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <returns></returns>
        public string GenerateMailPostCommentReplyHTML(long commentId)
        {
            using (var db = new ZapContext())
            {
                var commentInfo = db.Comments.Where(cmt => cmt.CommentId == commentId).Select(c => new PostCommentReplyEmail()
                {CommentId = c.CommentId, Score = c.Score, Text = c.Text, UserId = c.UserId.Id, UserName = c.UserId.Name, UserAppId = c.UserId.AppId, ProfileImageVersion = c.UserId.ProfileImage.Version, PostTitle = c.Post == null ? "" : c.Post.PostTitle, PostId = c.Post == null ? 0 : c.Post.PostId, ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId, ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id, ParentUserAppId = c.Parent == null ? "" : c.Parent.UserId.AppId, ParentUserProfileImageVersion = c.Parent == null ? 0 : c.Parent.UserId.ProfileImage.Version, ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name, ParentCommentText = c.Parent == null ? "" : c.Parent.Text, ParentScore = c.Parent == null ? 0 : c.Parent.Score, }).FirstOrDefault();
                var emailContent = renderEmail(commentInfo);
                // Make images resolve to zapread
                emailContent = makeImagesFQDN(emailContent);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "postId"></param>
        /// <returns></returns>
        public string GenerateMailNewPostHTML(int postId)
        {
            using (var db = new ZapContext())
            {
                var postInfo = db.Posts.Where(p => p.PostId == postId).Select(p => new NewPostEmail()
                {PostId = p.PostId, PostTitle = p.PostTitle, Score = p.Score, UserName = p.UserId.Name, UserAppId = p.UserId.AppId, ProfileImageVersion = p.UserId.ProfileImage.Version, GroupName = p.Group.GroupName, GroupId = p.Group.GroupId, Content = p.Content, }).FirstOrDefault();
                var emailContent = renderEmail(postInfo);
                // Make images resolve to zapread
                emailContent = makeImagesFQDN(emailContent);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "chatId"></param>
        /// <returns></returns>
        public string GenerateNewChatHTML(int chatId)
        {
            using (var db = new ZapContext())
            {
                var chatInfo = db.Messages.Where(m => m.Id == chatId).Select(m => new NewChatEmail()
                {FromName = m.From.Name, FromAppId = m.From.AppId, FromProfileImgVersion = m.From.ProfileImage.Version, IsReceived = true, Content = m.Content, }).FirstOrDefault();
                var emailContent = renderEmail(chatInfo);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userId"></param>
        /// <param name = "oldName"></param>
        /// <param name = "newName"></param>
        /// <returns></returns>
        public string GenerateUpdatedUserAliasHTML(int userId, string oldName, string newName)
        {
            var updateInfo = new UpdatedUserAliasEmail()
            {OldUserName = oldName, NewUserName = newName, };
            var emailContent = renderEmail(updateInfo);
            return emailContent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <returns></returns>
        public string GenerateUserMentionedInCommentHTML(long commentId)
        {
            using (var db = new ZapContext())
            {
                var mentionInfo = db.Comments.Where(c => c.CommentId == commentId).Select(c => new UserMentionedInCommentEmail()
                {CommentId = c.CommentId, Score = c.Score, Text = c.Text, UserId = c.UserId.Id, UserName = c.UserId.Name, UserAppId = c.UserId.AppId, ProfileImageVersion = c.UserId.ProfileImage.Version, PostTitle = c.Post == null ? "" : c.Post.PostTitle, PostId = c.Post == null ? 0 : c.Post.PostId, }).FirstOrDefault();
                var emailContent = renderEmail(mentionInfo);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "postId"></param>
        /// <returns></returns>
        public string GenerateUserMentionedInPostHTML(long postId)
        {
            using (var db = new ZapContext())
            {
                var postInfo = db.Posts.Where(p => p.PostId == postId).Select(p => new UserMentionedInPostEmail()
                {PostId = p.PostId, PostTitle = p.PostTitle, Score = p.Score, UserName = p.UserId.Name, UserAppId = p.UserId.AppId, ProfileImageVersion = p.UserId.ProfileImage.Version, GroupName = p.Group.GroupName, GroupId = p.Group.GroupId, Content = p.Content, }).FirstOrDefault();
                var emailContent = renderEmail(postInfo);
                // Make images resolve to zapread
                emailContent = makeImagesFQDN(emailContent);
                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userAppId"></param>
        /// <returns></returns>
        public bool MailWeeklySummary(string userAppId)
        {
            using (var db = new ZapContext())
            {
                var emailContent = GenerateMailWeeklySummary(userAppId);
                using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                {
                    var applicationUser = userManager.FindById(userAppId);
                    // Only send emails if they have confirmed their email address
                    if (applicationUser != null && applicationUser.EmailConfirmed)
                    {
                        string receiverEmail = applicationUser.Email;
                        // Debug
                        if (true)
                        {
                            BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                            {Destination = receiverEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "Your Zapread Weekly Summary", }, "Notify", true));
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// This should only be fired once a week
        /// </summary>
        /// <returns></returns>
        public bool MailWeeklySummaries()
        {
            using (var db = new ZapContext())
            {
                var cutoffTime = DateTime.Now - TimeSpan.FromDays(15);
                var usersToMailInfo = db.Users.Where(u => u.DateLastActivity > cutoffTime).Select(u => new
                {
                u.AppId
                }).ToList();
                var delay = 0;
                foreach (var ui in usersToMailInfo)
                {
                    // rate limit emails
                    delay += 3;
                    BackgroundJob.Schedule<MailingService>(x => x.MailWeeklySummary(ui.AppId), TimeSpan.FromSeconds(delay));
                }

                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userId"></param>
        /// <param name = "oldName"></param>
        /// <param name = "newName"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public bool MailUpdatedUserAlias(int userId, string oldName, string newName, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var userInfo = db.Users.Where(u => u.Id == userId).Select(u => new
                {
                u.AppId
                }).FirstOrDefault();
                var emailContent = GenerateUpdatedUserAliasHTML(userId, oldName, newName);
                if (isTest)
                {
                    var emailDestination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];
                    SendI(new UserEmailModel()
                    {Destination = emailDestination, Body = emailContent, Email = "", Name = "zapread.com", Subject = "User alias updated", }, "Notify", true);
                    return true;
                }

                using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                {
                    string receiverEmail = userManager.FindById(userInfo.AppId).Email;
                    BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                    {Destination = receiverEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "Your Zapread Username has been updated", }, "Notify", true));
                    return true;
                }
            }
        }

        /// <summary>
        /// Handle the mailing of notifications that there was a new comment on a post
        /// </summary>
        /// <param name = "commentId"></param>
        /// <param name = "isTest"></param>
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
                    {Destination = emailDestination, Body = emailContent, Email = "", Name = "zapread.com", Subject = "New Comment on Post", }, "Notify", true);
                    return emailContent;
                }

                // Send to post author and followers ...
                var postInfo = db.Comments.Where(c => c.CommentId == commentId).Select(c => new
                {
                UserAppId = c.UserId.AppId, c.Post.UserId.Settings.NotifyOnOwnPostCommented, c.Post.UserId.AppId, FollowerAppIds = c.Post.FollowedByUsers.Select(u => u.AppId)}).FirstOrDefault();
                // Post Author Notify
                if (postInfo != null && postInfo.AppId != postInfo.UserAppId // Don't send to author if author commented on own post
 && postInfo.NotifyOnOwnPostCommented) // Only send if author wants to be notified
                {
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], postInfo.AppId + ":" + SubscriptionTypes.OwnPostComment);
                        emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                        var applicationUser = userManager.FindById(postInfo.AppId);
                        // Only send if email confirmed
                        if (applicationUser != null && applicationUser.EmailConfirmed)
                        {
                            string postAuthorEmail = applicationUser.Email;
                            SendI(new UserEmailModel()
                            {Destination = postAuthorEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "New comment on your post", }, "Notify", true);
                        }
                    }
                }

                if (postInfo != null && postInfo.FollowerAppIds.Any())
                {
                    // Tie into user database in order to get user emails
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        foreach (var followerAppId in postInfo.FollowerAppIds)
                        {
                            var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], followerAppId + ":" + SubscriptionTypes.FollowedPostComment);
                            emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                            string postFollowerEmail = userManager.FindById(followerAppId).Email;
                            SendI(new UserEmailModel()
                            {Destination = postFollowerEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "New comment on a post you are following", }, "Notify", true);
                        }
                    }
                }

                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "commentId"></param>
        /// <param name = "isTest"></param>
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
                    {Destination = emailDestination, Body = emailContent, Email = "", Name = "zapread.com", Subject = "New reply to your comment", }, "Notify", true);
                    return emailContent;
                }

                var commentInfo = db.Comments.Where(c => c.CommentId == commentId).Select(c => new
                {
                CommenterAppId = c.UserId.AppId, ParentAppId = c.Parent.UserId.AppId, c.Parent.UserId.Settings.NotifyOnOwnCommentReplied, FollowerAppIds = c.Post.FollowedByUsers.Select(u => u.AppId)}).FirstOrDefault();
                // Don't email replies to own comment
                if (commentInfo.ParentAppId == commentInfo.CommenterAppId)
                {
                    return "Not mailed to own comment.";
                }

                if (commentInfo != null && commentInfo.NotifyOnOwnCommentReplied)
                {
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        var applicationUser = userManager.FindById(commentInfo.ParentAppId);
                        // Only send if email confirmed
                        if (applicationUser != null && applicationUser.EmailConfirmed)
                        {
                            string receiverEmail = applicationUser.Email;
                            var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], commentInfo.ParentAppId + ":" + SubscriptionTypes.OwnCommentReply);
                            emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                            BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                            {Destination = receiverEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "New reply to your comment", }, "Notify", true));
                        }
                    }
                }

                return emailContent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userIdFollowed"></param>
        /// <param name = "userIdFollowing"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public bool MailUserNewFollower(int userIdFollowed, int userIdFollowing, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var emailContent = GenerateMailNewUserFollowing(userIdFollowing);
                var userInfo = db.Users.Where(u => u.Id == userIdFollowed).Select(u => new
                {
                u.AppId
                }).FirstOrDefault();
                var otherUserInfo = db.Users.Where(u => u.Id == userIdFollowing).Select(u => new
                {
                u.AppId, u.Name
                }).FirstOrDefault();
                if (userInfo != null)
                {
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        var applicationUser = userManager.FindById(userInfo.AppId);
                        // Only send if email confirmed
                        if (applicationUser != null && applicationUser.EmailConfirmed)
                        {
                            string receiverEmail = applicationUser.Email;
                            var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], userInfo.AppId + ":" + SubscriptionTypes.NewUserFollowing);
                            emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                            BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                            {Destination = receiverEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = otherUserInfo.Name + " is now following you", }, "Notify", true));
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "postId"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public bool MailUserMentionedInPost(long postId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var mentionInfo = db.Posts.Where(c => c.PostId == postId).Select(c => new
                {
                Text = c.Content, FromUserName = c.UserId.Name
                }).FirstOrDefault();
                var emailContent = GenerateUserMentionedInPostHTML(postId);
                var doc = new HtmlDocument();
                doc.LoadHtml(mentionInfo.Text);
                var spans = doc.DocumentNode.SelectNodes("//span");
                if (spans != null)
                {
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        foreach (var s in spans)
                        {
                            if (s.Attributes.Count(a => a.Name == "class") > 0)
                            {
                                var cls = s.Attributes.FirstOrDefault(a => a.Name == "class");
                                if (cls.Value.Contains("userhint"))
                                {
                                    var username = s.InnerHtml.Replace("@", "");
                                    var mentionedUserInfo = db.Users.Where(u => u.Name == username).Select(u => new
                                    {
                                    u.Settings.NotifyOnMentioned, u.AppId
                                    }).FirstOrDefault();
                                    if (mentionedUserInfo.NotifyOnMentioned)
                                    {
                                        var applicationUser = userManager.FindById(mentionedUserInfo.AppId);
                                        // Only send if email confirmed
                                        if (applicationUser != null && applicationUser.EmailConfirmed)
                                        {
                                            string receiverEmail = applicationUser.Email;
                                            var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], mentionedUserInfo.AppId + ":" + SubscriptionTypes.UserMentionedInComment);
                                            emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                                            BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                                            {Destination = receiverEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "You were mentioned in a post by " + mentionInfo.FromUserName, }, "Notify", true));
                                        }
                                    }
                                }
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
        /// <param name = "commentId"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public bool MailUserMentionedInComment(long commentId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var mentionInfo = db.Comments.Where(c => c.CommentId == commentId).Select(c => new
                {
                c.Text, FromUserName = c.UserId.Name
                }).FirstOrDefault();
                var emailContent = GenerateUserMentionedInCommentHTML(commentId);
                var doc = new HtmlDocument();
                doc.LoadHtml(mentionInfo.Text);
                var spans = doc.DocumentNode.SelectNodes("//span");
                if (spans != null)
                {
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        foreach (var s in spans)
                        {
                            if (s.Attributes.Count(a => a.Name == "class") > 0)
                            {
                                var cls = s.Attributes.FirstOrDefault(a => a.Name == "class");
                                if (cls.Value.Contains("userhint"))
                                {
                                    var username = s.InnerHtml.Replace("@", "");
                                    var mentionedUserInfo = db.Users.Where(u => u.Name == username).Select(u => new
                                    {
                                    u.Settings.NotifyOnMentioned, u.AppId
                                    }).FirstOrDefault();
                                    if (mentionedUserInfo.NotifyOnMentioned)
                                    {
                                        var applicationUser = userManager.FindById(mentionedUserInfo.AppId);
                                        // Only send if email confirmed
                                        if (applicationUser != null && applicationUser.EmailConfirmed)
                                        {
                                            string receiverEmail = applicationUser.Email;
                                            var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], mentionedUserInfo.AppId + ":" + SubscriptionTypes.UserMentionedInComment);
                                            emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                                            BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                                            {Destination = receiverEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "You were mentioned in a comment by " + mentionInfo.FromUserName, }, "Notify", true));
                                        }
                                    }
                                }
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
        /// <param name = "postId"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public bool MailNewPostToFollowers(int postId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var emailContent = GenerateMailNewPostHTML(postId);
                if (isTest)
                {
                    var key = System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"];
                    var userUnsubscribeId = CryptoService.EncryptString(key, "test");
                    emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                    return SendTestEmail(emailContent, "New post by user you are following");
                }

                var followUsers = db.Posts.Where(p => p.PostId == postId).SelectMany(p => p.UserId.Followers).Select(u => new
                {
                u.Id, u.AppId, u.Settings.NotifyOnNewPostSubscribedUser, u.Settings.AlertOnNewPostSubscribedUser, user = u, }).ToList();
                if (followUsers.Any())
                {
                    // Tie into user database in order to get user emails
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        foreach (var follower in followUsers)
                        {
                            //Email
                            if (follower.NotifyOnNewPostSubscribedUser)
                            {
                                var applicationUser = userManager.FindById(follower.AppId);
                                // Only send if email confirmed
                                if (applicationUser != null && applicationUser.EmailConfirmed)
                                {
                                    string followerEmail = applicationUser.Email;
                                    var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], follower.AppId + ":" + SubscriptionTypes.FollowedUserNewPost);
                                    emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                                    BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                                    {Destination = followerEmail, Body = emailContent, Email = "", Name = "zapread.com", Subject = "New post by user you are following", }, "Notify", true));
                                }
                            }
                        }

                        db.SaveChanges();
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "chatId"></param>
        /// <param name = "isTest"></param>
        /// <returns></returns>
        public bool MailNewChatToUser(int chatId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                // Check if receiver is online
                var msgInfo = db.Messages.Where(m => m.Id == chatId).Select(m => new
                {
                m.Id, SenderName = m.From.Name, SenderId = m.From.Id, ToId = m.To.Id, m.To.AppId, m.To.IsOnline, m.To.Settings.NotifyOnPrivateMessage, }).FirstOrDefault();
                // Only send email if offline
                if (msgInfo != null && !msgInfo.IsOnline)
                {
                    var emailContent = GenerateNewChatHTML(chatId);
                    using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                    {
                        if (msgInfo.NotifyOnPrivateMessage)
                        {
                            var applicationUser = userManager.FindById(msgInfo.AppId);
                            // Only send if email confirmed
                            if (applicationUser != null && applicationUser.EmailConfirmed)
                            {
                                string receiverEmail = applicationUser.Email;
                                var userUnsubscribeId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], msgInfo.AppId + ":" + SubscriptionTypes.NewChat);
                                emailContent = emailContent.Replace("[userUnsubscribeId]", userUnsubscribeId);
                                var replyId = CryptoService.EncryptString(System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"], msgInfo.SenderId + ":" + msgInfo.ToId + ":" + msgInfo.Id + ":" + SubscriptionTypes.NewChat);
                                emailContent = emailContent.Replace("[replyId]", replyId);
                                BackgroundJob.Enqueue<MailingService>(x => x.SendI(new UserEmailModel()
                                {Destination = receiverEmail, Body = emailContent, Email = "notify+" + replyId + "@zapread.com", // This is the reply-to
 Name = "zapread.com", Subject = "New private ZapRead message from " + msgInfo.SenderName, }, "Notify", true));
                            }
                        }
                    }
                }

                return true;
            }
        }

        private bool SendTestEmail(string emailContent, string subject)
        {
            var emailDestination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"];
            SendI(new UserEmailModel()
            {Destination = emailDestination, Body = emailContent, Email = "", Name = "zapread.com", Subject = subject, }, "Notify", true);
            return true;
        }

        /// <summary>
        /// This mailer should be able to run on HangFire without an HttpContext
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> TestMailer()
        {
            var emailViewRenderer = getRenderer();
            var testEmail = new TestEmail()
            {Comment = "Test"};
            string HTMLString = emailViewRenderer.Render(testEmail);
            // premailer cleanup
            PreMailer.Net.InlineResult result;
            var baseUri = new Uri("https://www.zapread.com/");
            result = PreMailer.Net.PreMailer.MoveCssInline(baseUri: baseUri, html: HTMLString, removeComments: true, removeStyleElements: true, stripIdAndClassAttributes: true);
            var cleanHTMLString = result.Html;
            return await Task.FromResult(true);
        }

        private static Postal.EmailViewRenderer getRenderer()
        {
            var engines = new ViewEngineCollection();
            var assembiles = AppDomain.CurrentDomain.GetAssemblies();
            var zr = assembiles.SingleOrDefault(assembly => assembly.GetName().Name == "zapread.com");
            engines.Add(new Postal.ResourceRazorViewEngine(viewSourceAssembly: zr, viewPathRoot: "zapread.com.Views.Mailer"));
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
            result = PreMailer.Net.PreMailer.MoveCssInline(baseUri: baseUri, html: HTMLString, removeComments: true, removeStyleElements: true, stripIdAndClassAttributes: true);
            var cleanHTMLString = result.Html;
            return cleanHTMLString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "message"></param>
        /// <param name = "user"></param>
        /// <param name = "useSSL"></param>
        /// <returns></returns>
        public bool SendI(UserEmailModel message, string user = "Accounts", bool useSSL = true)
        {
            // Plug in your email service here to send an email.
            var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
            var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"], CultureInfo.InvariantCulture);
            var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
            var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];
            var mmessage = new MailMessage();
            mmessage.To.Add(new MailAddress(message.Destination));
            mmessage.From = new MailAddress(emailuser); // replace with valid value
            mmessage.Subject = message.Subject;
            mmessage.Body = message.Body;
            mmessage.IsBodyHtml = true;
            if (message.Email != null && message.Email != "")
            {
                mmessage.ReplyToList.Add(message.Email);
            }

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential{UserName = emailuser, Password = emailpass};
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
        /// <param name = "message"></param>
        /// <param name = "user"></param>
        /// <param name = "useSSL"></param>
        /// <returns></returns>
        public static bool Send(UserEmailModel message, string user = "Accounts", bool useSSL = true)
        {
            // Plug in your email service here to send an email.
            var emailhost = System.Configuration.ConfigurationManager.AppSettings["EmailSMTPHost"];
            var emailport = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailSMTPPort"], CultureInfo.InvariantCulture);
            var emailuser = System.Configuration.ConfigurationManager.AppSettings[user + "EmailUser"];
            var emailpass = System.Configuration.ConfigurationManager.AppSettings[user + "EmailPass"];
            var mmessage = new MailMessage();
            mmessage.To.Add(new MailAddress(message.Destination));
            mmessage.From = new MailAddress(emailuser); // replace with valid value
            mmessage.Subject = message.Subject;
            mmessage.Body = message.Body;
            mmessage.IsBodyHtml = true;
            if (message.Email != null && message.Email != "")
            {
                mmessage.ReplyToList.Add(message.Email);
            }

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential{UserName = emailuser, Password = emailpass};
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
        /// <param name = "title"></param>
        /// <param name = "message"></param>
        /// <returns></returns>
        public static bool SendErrorNotification(string title, string message)
        {
            // Send error
            return Send(new UserEmailModel()
            {Body = message, Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"], Email = "", Name = "zapread.com Exception", Subject = title, });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "message"></param>
        /// <param name = "user"></param>
        /// <param name = "useSSL"></param>
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
            mmessage.From = new MailAddress(emailuser); // replace with valid value
            mmessage.Subject = message.Subject;
            mmessage.Body = message.Body;
            mmessage.IsBodyHtml = true;
            if (message.Email != null && message.Email != "")
            {
                mmessage.ReplyToList.Add(new MailAddress(message.Email));
            }

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential{UserName = emailuser, Password = emailpass};
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