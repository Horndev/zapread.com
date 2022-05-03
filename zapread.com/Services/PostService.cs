using Hangfire;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using zapread.com.Database;

namespace zapread.com.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class PostService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> OnComment()
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Batch service method to update post scores
        /// </summary>
        /// <returns></returns>
        public bool UpdateScores()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool PostImpressionEnqueue(int id)
        {
            BackgroundJob.Enqueue<PostService>(methodCall: x => x.PostImpressionIncrement(id));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<bool> PostImpressionEnqueue(IEnumerable<int> ids)
        {
            // Do this in another thread
            Task<bool> task = Task.Run(() =>
            {
                foreach (var id in ids)
                {
                    BackgroundJob.Enqueue<PostService>(methodCall: x => x.PostImpressionIncrement(id));
                }
                return true;
            });

            return await task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool PostImpressionIncrement(int id)
        {
            using (var db = new ZapContext())
            {
                var post = db.Posts.FirstOrDefault(p => p.PostId == id);
                if (post != null)
                {
                    post.Impressions += 1;
                    db.SaveChanges();
                }
                return true;
            }
        }
    }
}