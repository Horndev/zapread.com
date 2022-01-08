using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using zapread.com.Models;

namespace zapread.com.Services
{
    public class MailingService
    {
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