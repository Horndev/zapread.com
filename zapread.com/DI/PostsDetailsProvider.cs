using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Database;

namespace zapread.com.DI
{
    public class PostsDetailsProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            using (var db = new ZapContext())
            {
                // Create a node for each album 
                var posts = db.Posts
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft);

                foreach (var post in posts)
                {
                    DynamicNode dynamicNode = new DynamicNode();
                    dynamicNode.Title = post.PostTitle;
                    //dynamicNode.ParentKey = "Detail_" + post.Group.GroupName;
                    dynamicNode.RouteValues.Add("id", post.PostId);

                    yield return dynamicNode;
                }
            }
        }
    }
}