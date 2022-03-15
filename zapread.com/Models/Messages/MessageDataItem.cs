using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Messages
{
    /// <summary>
    /// // used for The message table in /Messages/GetMessagesTable/
    /// </summary>
    public class MessageDataItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FromID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Anchor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
    }
}