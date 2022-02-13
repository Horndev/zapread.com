using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Helpers;

namespace zapread.com.DI
{
    /// <summary>
    /// Used for generating the sitemap
    /// </summary>
    public class PostsDetailsProvider : DynamicNodeProviderBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            using (var db = new ZapContext())
            {
                // Create a node for each post
                var posts = db.Posts
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft)
                    .Select(p => new {
                        p,
                        timeUpdated = p.Comments.OrderByDescending(c => c.TimeStamp)
                            .Select(c => c.TimeStamp)
                            .FirstOrDefault(),
                    });
                
                foreach (var post in posts)
                {
                    DynamicNode dynamicNode = new DynamicNode();
                    dynamicNode.Title = post.p.PostTitle;
                    //dynamicNode.ParentKey = "Detail_" + post.Group.GroupName;
                    dynamicNode.RouteValues.Add("PostId", post.p.PostId);
                    if (!string.IsNullOrEmpty(post.p.PostTitle))
                    {
                        dynamicNode.RouteValues.Add("postTitle", post.p.PostTitle.MakeURLFriendly());
                    }
                    dynamicNode.Protocol = "https";
                    // Re-index every month (for searching comments)
                    //dynamicNode.ChangeFrequency = ChangeFrequency.Monthly;

                    dynamicNode.LastModifiedDate = post.timeUpdated;

                    yield return dynamicNode;
                }
            }
        }
    }
}