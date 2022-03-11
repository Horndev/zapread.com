using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.Search;

namespace zapread.com.API
{
    /// <summary>
    /// API for searching ZapRead
    /// </summary>
    public class SearchController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="q">The query string</param>
        /// <param name="p">The page</param>
        /// <param name="ps">The page size (default 20)</param>
        /// <param name="type">Search type (all, post, user)</param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/search/{type}")]
        public async Task<IHttpActionResult> Search(string q = null, int p=1, int ps=20, string type = "all")
        {
            if (q == null)
            {
                return BadRequest("No query");
            }

            using (var db = new ZapContext())
            {
                // This is a custom search

                var comments = await db.Comments
                    .SqlQuery("SELECT TOP 100 i.rank as rank, Text, a.CommentId, a.TimeStamp, a.Score, a.TotalEarned, a.IsDeleted, a.IsReply, a.TimeStampEdited  " +
                    "FROM freetexttable(Comment, Text, @q) as i " +
                    "inner join Comment a " +
                    "on i.[key] = a.[CommentId] " +
                    "WHERE a.IsDeleted=0 " +
                    "order by i.rank desc", new SqlParameter("@q", q))
                    .ToListAsync();

                var posts = await db.Posts
                    .SqlQuery("SELECT TOP 100 i.rank as rank, Content, a.PostId, a.PostTitle, a.TimeStamp, a.IsDeleted, a.IsNSFW, a.IsSticky, a.IsDraft, a.IsPublished, a.Language, a.Impressions, a.Score, a.TotalEarned, a.TimeStampEdited " +
                    "FROM freetexttable(Post, Content, @q) as i " +
                    "inner join Post a " +
                    "on i.[key] = a.[PostId] " +
                    "WHERE a.IsDeleted=0 AND a.IsDraft=0 " +
                    "order by i.rank desc", new SqlParameter("@q", q))
                    .ToListAsync();

                // ideally should do an indexed view but this will have to do for now.
                var CommentIds = comments.Select(c => c.CommentId);
                var PostIds = posts.Select(i => i.PostId);

                var resfullcq = await db.Comments
                    .Where(c => CommentIds.Contains(c.CommentId))
                    .OrderByDescending(c => c.TimeStamp) // more recent first
                    .Skip((p-1)*ps)
                    .Take(ps)
                    .Select(c => new SearchResult()
                    {
                        Id = (int)c.CommentId,
                        Type = "comment",
                        PostId = c.Post.PostId,
                        Title = c.Post.PostTitle,
                        Content = c.Text,
                        UserAppId = c.UserId.AppId,
                        PostScore = c.Post.Score,
                        CommentScore = c.Score,
                        TimeStamp = c.TimeStamp,
                        AuthorName = c.UserId.Name,
                        GroupName = c.Post.Group != null ? c.Post.Group.GroupName : "Community"
                    })
                    .ToListAsync();

                var resfullpq = await db.Posts
                    .Where(c => PostIds.Contains(c.PostId))
                    .OrderByDescending(c => c.TimeStamp) // more recent first
                    .Skip((p - 1) * ps)
                    .Take(ps)
                    .Select(c => new SearchResult()
                    {
                        Id = c.PostId,
                        Type = "post",
                        PostId = c.PostId,
                        Title = c.PostTitle,
                        Content = c.Content,
                        UserAppId = c.UserId.AppId,
                        PostScore = c.Score,
                        CommentScore = 0,
                        TimeStamp = c.TimeStamp,
                        AuthorName = c.UserId.Name,
                        GroupName = c.Group != null ? c.Group.GroupName : "Community"
                    })
                    .ToListAsync();

                var resfullq = resfullcq.Union(resfullpq)
                    .OrderByDescending(c => c.TimeStamp) // more recent first
                    .Skip((p - 1) * ps)
                    .Take(ps).ToList();

                resfullq.ForEach(r => 
                {
                    r.EncPostId = zapread.com.Services.CryptoService.IntIdToString(r.PostId);
                });

                //var res = new List<SearchResult>();
                //foreach (var post in posts)
                //{
                //    res.Add(new SearchResult()
                //    {
                //        Id = post.PostId,
                //        Type = "post",
                //        Content = post.Content,
                //        Title = post.PostTitle,
                //        EncPostId = zapread.com.Services.CryptoService.IntIdToString(post.PostId)
                //    });
                //}

                //foreach(var comment in comments)
                //{
                //    res.Add(new SearchResult()
                //    {
                //        Id = Convert.ToInt32(comment.CommentId),
                //        Type = "post",
                //        Content = comment.Text,
                //        Title = "Comment",
                //        EncPostId = Services.CryptoService.IntIdToString(Convert.ToInt32(comment.CommentId))
                //    });
                //}

                return Ok(new { success = true, result = resfullq });
            }
        }
    }
}
