using Hangfire;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Database.Financial;
using zapread.com.Models.Posts;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Controller for the /Vote route
    /// </summary>
    public class VoteController : Controller
    {
        /// <summary>
        /// User voting on a post
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> Post(Vote v)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid" });
            }

            // Bad parameters check
            if (v == null || v.a < 1)
            {
                return Json(new { success = false, message = "Invalid" });
            }

            // Do checks
            using (var db = new ZapContext())
            {
                var postInfo = await db.Posts
                    .Where(p => p.PostId == v.Id)
                    .Select(p => new
                    {
                        p.Score,
                        p.UserId.Reputation
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var userAppId = User.Identity.GetUserId();

                double scoreAdj = 0.0;

                if (userAppId == null) // Anonymous vote
                {
                    // Check if vote tx has been claimed
                    var txIsValid = await db.LightningTransactions
                        .Where(tx => tx.Id == v.tx)
                        .Select(tx => tx.IsSettled && !tx.IsSpent && tx.Amount >= v.a)
                        .FirstOrDefaultAsync().ConfigureAwait(true); // bool default is false

                    if (!txIsValid)
                    {
                        return Json(new { success = false, message = "No transaction to vote with" });
                    }

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: v.a * (v.d == 1 ? 1 : -1),
                        targetRep: v.d == 1 ? 0 : postInfo.Reputation,
                        actorRep: 0);
                }
                else
                {
                    var userInfo = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => new { 
                            hasFunds = u.Funds.Balance > v.a,
                            u.Reputation
                        })
                        .FirstOrDefaultAsync().ConfigureAwait(true); // bool default is false

                    if (!userInfo.hasFunds)
                    {
                        return Json(new { success = false, message = "Error with requesting user." });
                    }

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: v.a * (v.d == 1 ? 1 : -1),
                        targetRep: v.d == 1 ? 0 : postInfo.Reputation,
                        actorRep: userInfo.Reputation);
                }

                if (postInfo == null)
                {
                    return Json(new { success = false, message = "Invalid Post" });
                }

                // All good - queue processing - this part is slower, so it will be done in a background job
                // Return the optimistic result to the user to improve UI response
                BackgroundJob.Enqueue<VoteService>(x => x.PostVote(
                    userAppId,
                    v.Id,       // postId
                    v.a,        // amount
                    v.d == 1,   // isUpvote
                    v.tx        // txid
                ));

                // Note that this is actually going to be saved in the background job
                // we are only sending the optimistic estimate here
                var newScore = postInfo.Score + scoreAdj;

                // Return quick results
                return Json(new
                {
                    success = true,
                    delta = v.d == 1 ? 1 : -1,
                    scoreStr = newScore.ToAbbrString(),
                    deltaCommunity = Convert.ToInt32(v.a*0.1*(v.d == 1 ? 1.0 : -1.0)),
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> Comment(Vote v)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid" });
            }

            // Bad parameters check
            if (v == null || v.a < 1)
            {
                return Json(new { success = false, message = "Invalid" });
            }
            
            // Here we will do some quick checks, answer the user, and queue the formal settlement
            // in a batch queue.
            using (var db = new ZapContext())
            {
                var commentInfo = await db.Comments
                    .Where(c => c.CommentId == v.Id)
                    .Select(c => new {
                        c.Score,
                        c.UserId.Reputation
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var userAppId = User.Identity.GetUserId();

                double scoreAdj = 0.0;

                if (userAppId == null) // Anonymous vote
                {
                    // Check if vote tx has been claimed
                    var txIsValid = await db.LightningTransactions
                        .Where(tx => tx.Id == v.tx)
                        .Select(tx => tx.IsSettled && !tx.IsSpent && tx.Amount >= v.a)
                        .FirstOrDefaultAsync().ConfigureAwait(true); // bool default is false

                    if (!txIsValid)
                    {
                        return Json(new { success = false, message = "No transaction to vote with" });
                    }

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: v.a * (v.d == 1 ? 1 : -1),
                        targetRep: v.d == 1 ? 0 : commentInfo.Reputation,
                        actorRep: 0);
                }
                else
                {
                    var userInfo = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => new {
                            hasFunds = u.Funds.Balance > v.a,
                            u.Reputation
                        })
                        .FirstOrDefaultAsync().ConfigureAwait(true); // bool default is false

                    if (!userInfo.hasFunds)
                    {
                        return Json(new { success = false, message = "Error with requesting user." });
                    }

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: v.a * (v.d == 1 ? 1 : -1),
                        targetRep: v.d == 1 ? 0 : commentInfo.Reputation,
                        actorRep: userInfo.Reputation);
                }

                if (commentInfo == null)
                {
                    return Json(new { success = false, message = "Invalid Comment" });
                }

                // All good - queue processing - this part is slower, so it will be done in a background job
                // Return the optimistic result to the user to improve UI response
                BackgroundJob.Enqueue<VoteService>(x => x.CommentVote(
                    userAppId,
                    v.Id,       // commentId
                    v.a,        // amount
                    v.d == 1,   // isUpvote
                    v.tx        // txid
                ));

                var newScore = commentInfo.Score + scoreAdj;

                return Json(new {
                    success = true,
                    delta = v.d == 1 ? 1 : -1, 
                    scoreStr = newScore.ToAbbrString(),
                    deltaCommunity = Convert.ToInt32(v.a * 0.1 * (v.d == 1 ? 1.0 : -1.0)),
                });
            }
        }
    }
}