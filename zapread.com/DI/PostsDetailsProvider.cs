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
                    .Where(p => p.Score > -50)
                    .Select(p => new {
                        p,
                        lastComment = p.Comments.OrderByDescending(c => c.TimeStamp)
                            .Select(c => c.TimeStamp)
                            .FirstOrDefault(),
                        Timestamp = p.TimeStampEdited.HasValue ? p.TimeStampEdited.Value : p.TimeStamp,
                    }).Select(p => new
                    {
                        p.p.PostTitle,
                        p.p.PostId,
                        timeUpdated = p.lastComment.HasValue ? (p.lastComment.Value > p.Timestamp) ? p.lastComment.Value : p.Timestamp : p.Timestamp,
                    });
                
                foreach (var post in posts)
                {
                    DynamicNode dynamicNode = new DynamicNode();
                    dynamicNode.Title = post.PostTitle;
                    //dynamicNode.ParentKey = "Detail_" + post.Group.GroupName;
                    
                    //dynamicNode.RouteValues.Add("PostId", post.p.PostId);
                    
                    if (!string.IsNullOrEmpty(post.PostTitle))
                    {
                        dynamicNode.RouteValues.Add("postIdEnc", Services.CryptoService.IntIdToString(post.PostId));
                        dynamicNode.RouteValues.Add("postTitle", post.PostTitle.MakeURLFriendly());
                    }
                    else
                    {
                        dynamicNode.RouteValues.Add("PostId", post.PostId);
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