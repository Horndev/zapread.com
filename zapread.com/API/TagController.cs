using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.API.Tag;
using zapread.com.Helpers;
using Microsoft.AspNet.Identity;
using zapread.com.Models.Database;
using HtmlAgilityPack;

namespace zapread.com.API
{
    /// <summary>
    /// 
    /// </summary>
    public class TagController : ApiController
    {
        /// <summary>
        /// Get posts from the tag
        /// </summary>
        /// <param name="req">query parameters</param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/tag/posts")]
        public async Task<IHttpActionResult> GetPosts(GetTagPostsParameters req)
        {
            if (req == null) return BadRequest();

            if (req.TagId.HasValue && req.TagId < 1) return BadRequest();

            if (!req.TagId.HasValue && String.IsNullOrEmpty(req.TagName)) return BadRequest();

            int BlockSize = req.blockSize ?? 10;

            int BlockNumber = req.blockNumber ?? 0;

            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();

                var userInfo = string.IsNullOrEmpty(userAppId) ? null : await db.Users
                    .Select(u => new QueryHelpers.PostQueryUserInfo()
                    {
                        Id = u.Id,
                        AppId = u.AppId,
                        ViewAllLanguages = u.Settings.ViewAllLanguages,
                        IgnoredGroups = u.IgnoredGroups.Select(g => g.GroupId).ToList(),
                        IgnoredPosts = u.IgnoringPosts.Select(p => p.PostId).ToList(),
                    })
                    .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);

                IQueryable<Post> validposts = QueryHelpers.QueryValidPosts(
                    userLanguages: null,
                    db: db,
                    userInfo: userInfo);

                var tagId = 0;

                if (!req.TagId.HasValue)
                {
                    //
                    var nameQuery = req.TagName;

                    var tagIdRes = await db.Tags
                        .Where(g => g.TagName == nameQuery)
                        .Select(g => new { g.TagId })
                        .FirstOrDefaultAsync();

                    if (tagIdRes == null)
                    {
                        return NotFound();
                    }

                    tagId = tagIdRes.TagId;
                }
                else
                {
                    tagId = req.TagId.Value;
                }

                var tagPosts = QueryHelpers.OrderPostsByNew(
                    validposts: validposts,
                    tagId: tagId, 
                    stickyPostOnTop: false);

                var postsVm = await QueryHelpers.QueryPostsVm(
                        start: BlockNumber * BlockSize,
                        count: BlockSize,
                        postquery: tagPosts,
                        userInfo: userInfo,
                        limitComments: true).ConfigureAwait(true);

                // Make images lazy TODO: apply this when submitting new posts
                postsVm.ForEach(post =>
                {
                    HtmlDocument postDocument = new HtmlDocument();
                    postDocument.LoadHtml(post.Content);

                    var postImages = postDocument.DocumentNode.SelectNodes("//img/@src");
                    if (postImages != null)
                    {
                        foreach (var postImage in postImages)
                        {
                            postImage.SetAttributeValue("loading", "lazy");
                        }
                        post.Content = postDocument.DocumentNode.OuterHtml;
                    }
                });

                var response = new GetTagPostsResponse()
                {
                    HasMorePosts = tagPosts.Count() >= BlockNumber * BlockSize,
                    Posts = postsVm,
                    success = true,
                };

                return Ok(response);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/tag/load")]
        public async Task<IHttpActionResult> GetTagInfo(GetTagInfoRequest req)
        {
            if (req == null) return BadRequest();

            using (var db = new ZapContext())
            {
                var tag = await db.Tags
                    .Where(t => t.TagName == req.TagName)
                    .Select(t => new TagItem()
                    {
                        id = t.TagId,
                        value = t.TagName,
                        count = t.Posts.Count
                    })
                    .FirstAsync();

                return Ok(new GetTagInfoResponse()
                {
                    Tag = tag,
                    IsLoggedIn = User.Identity.GetUserId() != null,
                    success = true
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/tag/list")]
        public async Task<IHttpActionResult> GetTags()
        {
            using (var db = new ZapContext())
            {
                var tags = await db.Tags
                    .OrderByDescending(t => t.Posts.Count + t.Comments.Count)
                    .Where(t => t.Posts.Where(p => !p.IsDeleted && !p.IsDraft).Count() > 0 
                        || t.Comments.Where(c => !c.IsDeleted && !c.Post.IsDeleted && !c.Post.IsDraft).Count() > 0)
                    .Take(100)
                    .Select(t => new TagItem()
                    {
                        value = t.TagName,
                        id = t.TagId,
                        count = t.Posts.Where(p=> !p.IsDeleted && !p.IsDraft).Count(),
                        CommentCount = t.Comments.Where(c => !c.IsDeleted && !c.Post.IsDeleted && !c.Post.IsDraft).Count()
                    })
                    .ToListAsync();

                tags.ForEach(t => t.link = "/tag/" + Uri.EscapeDataString(t.value.CleanUnicode().Trim()).Replace("%EF%BB%BF", "") + "/");

                return Ok(new GetTagsResponse()
                {
                    success = true,
                    Tags = tags
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/tag/mentions/list")]
        public async Task<IHttpActionResult> MentionTags(MentionTagRequest req)
        {
            if (req == null) { return BadRequest(); }

            using (var db = new ZapContext())
            {
                var searchTerm = req.SearchTerm.SanitizeXSS().CleanUnicode().Trim();

                if (searchTerm.Length > 60)
                {
                    searchTerm = searchTerm.Substring(0,60);
                }

                char[] arr = searchTerm.ToCharArray();
                arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c)
                                                  || char.IsWhiteSpace(c)
                                                  || c == '-')));
                searchTerm = new string(arr);

                var tagExists = await db.Tags
                    .AnyAsync(t => t.TagName.ToUpper() == searchTerm.ToUpper());

                var tags = await db.Tags
                    .Where(t => t.TagName.ToUpper() == searchTerm.ToUpper() || t.TagName.ToUpper().StartsWith(searchTerm.ToUpper()))
                    .OrderByDescending(t => t.Posts.Count())
                    .Select(u => new TagItem()
                    {
                        id = u.TagId,
                        value = u.TagName
                    })
                    .Take(10)
                    .ToListAsync().ConfigureAwait(false);

                if (!tagExists)
                {
                    tags = tags.Prepend(new TagItem() 
                    { 
                        id = -1, 
                        value = searchTerm, 
                        newtag = true 
                    }).ToList();
                }

                tags.ForEach(t => t.link = "/tag/" + Uri.EscapeDataString(t.value.CleanUnicode().Trim()).Replace("%EF%BB%BF", "") + "/");

                return Ok(new MentionTagResponse()
                {
                    success = true,
                    Tags = tags
                });
            }
        }
    }
}
