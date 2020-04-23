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
        public Int64 CommentId { get; set; }

        public string UserAppId { get; set; }

        public int ProfileImageVersion { get; set; }
    }
}