using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    public class PostViewModel
    {
        public Post Post;
        public bool ViewerIsMod;        // User has moderation privilage on this post
        public bool ViewerUpvoted;      // If the user has upvoted this post
        public bool ViewerDownvoted;    // If the user has downvoted this post
        public bool ViewerIgnoredUser;  // If the user has ignored the user

        public int NumComments;

        // Not ideal!
        public List<int> ViewerIgnoredUsers;  // If the user has ignored the user
    }

    public class NewPostViewModel
    {
        public Post Post { get; set; }
        public List<string> Languages { get; set; }
    }

    public class PostsViewModel
    {
        public List<PostViewModel> Posts;
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
        public string Language { get; set; }
    }

    public class PostCommentsViewModel
    {
        public Comment Comment { get; set; }
        public List<Comment> Comments { get; set; }
        public List<int> ViewerIgnoredUsers;  // If the user has ignored the user
    }
}