﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// Model for a comment reply to a post (root)
    /// 
    /// Also should send if a post is being followed by a user
    /// 
    /// </summary>
    public class PostCommentEmail: Postal.Email
    {
        /// <summary>
        /// 
        /// </summary>
        public Int64 CommentId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserAppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ProfileImageVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PostTitle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }
    }
}