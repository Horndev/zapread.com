using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Posts
{
    /// <summary>
    /// Model for data set up for the post edit view
    /// </summary>
    public class PostEditViewModel
    {
        /// <summary>
        /// Reputation of the user who is editing
        /// </summary>
        public long UserReputation { get; set; }
    }
}