using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Manage
{
    /// <summary>
    /// Used for the partial view of posts in manage/index
    /// </summary>
    public class ManageActivityPostsPartialViewModel
    {
        /// <summary>
        /// Posts by user
        /// </summary>
        public List<PostViewModel> ActivityPosts { get; set; }
    }
}