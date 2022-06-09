using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class Email
    {
        /// <summary>
        /// 
        /// </summary>
        public Email()
        {
            this.Attachments = new List<Attachment>();
        }

        /// <summary>
        /// 
        /// </summary>
        public uint MessageNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FromEmailAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime DateSent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Attachment> Attachments { get; set; }
    }
}