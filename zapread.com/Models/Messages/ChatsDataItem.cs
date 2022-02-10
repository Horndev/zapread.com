using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Messages
{
    /// <summary>
    /// Used for Chats table
    /// 
    /// [Todo] Fix types in these - even though they are consumed by js.
    /// </summary>
    public class ChatsDataItem
    {
        /// <summary>
        /// The status of the chat [TODO]
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// True if user has read the chat
        /// </summary>
        public string IsRead { get; set; }

        /// <summary>
        /// The type of chat [TODO]
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// User chat is from
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Identifier for who chat is from
        /// </summary>
        public string FromID { get; set; }

        /// <summary>
        /// Date of last message
        /// </summary>
        public string LastMessage { get; set; }
    }
}