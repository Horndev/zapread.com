using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Comments
{
    /// <summary>
    /// Contains the view data for the _PartialCommentReplyInput.cshtml
    /// </summary>
    public class CommentReplyInputViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Int64 CommentId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserAppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ProfileImageVersion { get; set; }
    }
}