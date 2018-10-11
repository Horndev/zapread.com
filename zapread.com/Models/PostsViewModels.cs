using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    public class PostsViewModel
    {
        public List<Post> Posts;
        public List<int> Upvoted;
        public List<int> Downvoted;
        public double UserBalance;
        public List<GroupInfo> SubscribedGroups;
        public string Sort;
    }

    public class NewPostMsg
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public int GroupId { get; set; }
        public string Title { get; set; }
        public bool IsDraft { get; set; }
    }

    public class PostCommentsViewModel
    {
        public Comment Comment { get; set; }
        public List<Comment> Comments { get; set; }
    }
}