using System.Collections.Generic;
using System.Linq;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.GroupView;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class HTMLRenderHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="user"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static PostViewModel CreatePostViewModel(Post p, User user, List<GroupStats> groups)
        {
            return new PostViewModel()
            {
                Post = p,
                ViewerIsMod = user != null ? user.GroupModeration.Select(g => g.GroupId).Contains(p.Group.GroupId) : false,
                ViewerUpvoted = user != null ? user.PostVotesUp.Select(pv => pv.PostId).Contains(p.PostId) : false,
                ViewerDownvoted = user != null ? user.PostVotesDown.Select(pv => pv.PostId).Contains(p.PostId) : false,
                NumComments = 0,
            };
        }
    }
}