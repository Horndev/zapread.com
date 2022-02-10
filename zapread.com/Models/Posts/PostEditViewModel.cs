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

        /// <summary>
        /// GUID identifier for user
        /// </summary>
        public string UserAppId { get; set; }

        /// <summary>
        /// Version of user image to use
        /// </summary>
        public int ProfileImageVersion { get; set; }
    }
}