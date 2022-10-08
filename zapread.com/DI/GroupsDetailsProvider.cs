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
    /// Provides sitemap node for group
    /// </summary>
    public class GroupsDetailsProvider : DynamicNodeProviderBase
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
                var groups = db.Groups
                    .Select(g => new {
                        g.GroupName,
                        g.GroupId,
                        timeUpdated = g.Posts.OrderByDescending(c => c.TimeStamp)
                            .Select(c => c.TimeStamp)
                            .FirstOrDefault(),
                    });
                
                foreach (var group in groups)
                {
                    DynamicNode dynamicNode = new DynamicNode();
                    
                    dynamicNode.Title = group.GroupName;
                    //dynamicNode.ParentKey = "Detail_" + post.Group.GroupName;

                    dynamicNode.RouteValues.Add("id", group.GroupId);
                    //dynamicNode.RouteValues.Add("name", group.GroupName.MakeURLFriendly());

                    dynamicNode.Protocol = "https";
                    // Re-index every month (for searching comments)
                    //dynamicNode.ChangeFrequency = ChangeFrequency.Monthly;

                    dynamicNode.LastModifiedDate = group.timeUpdated;

                    yield return dynamicNode;
                }
            }
        }
    }
}