using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Database.Financial;

namespace zapread.com.Services
{
    /// <summary>
    /// Voting service
    /// </summary>
    public class VoteService
    {
        private static void RecordFundTransfers(
            UserFunds from, 
            UserFunds to, 
            Group group, 
            double amountFrom, 
            double amountTo, 
            double amountGroup,
            double amountCommunity,
            double amountZapread)
        {
            using (var db = new ZapContext())
            {
                var website = db.ZapreadGlobals.FirstOrDefault(i => i.Id == 1);

                if (group == null)
                {
                    website.CommunityEarnedToDistribute += amountGroup;
                }
                else
                {
                    group.TotalEarnedToDistribute += amountGroup;
                }

                website.CommunityEarnedToDistribute += amountCommunity;
                website.ZapReadTotalEarned += amountZapread;
                website.ZapReadEarnedBalance += amountZapread;

                int attempts = 0;
                bool saveFailed;
                bool saveAborted = false;
                do
                {
                    attempts++;
                    saveFailed = false;

                    if (attempts < 50)
                    {
                        // This really shouldn't happen!
                        if (from != null) // ignore if we're not debiting a user - funds are from a new deposit
                        {
                            from.Balance -= amountFrom;
                        }

                        if (to != null) // downvotes could go to just community and group
                        {
                            to.Balance += amountTo;
                        }

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
                        {
                            saveFailed = true;
                            var entry = ex.Entries.Single();
                            entry.Reload();
                        }
                    }
                    else
                    {
                        saveAborted = true;
                        throw new Exception("Unable to save financials after 50 attempts.");
                    }
                }
                while (saveFailed);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppId"></param>
        /// <param name="postId"></param>
        /// <param name="amount"></param>
        /// <param name="isUpvote"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
        public bool PostVote(string userAppId, int postId, int amount, bool isUpvote, int txid)
        {
            double scoreAdj = 0.0;

            using (var db = new ZapContext())
            {
                var post = db.Posts
                    .Where(p => p.PostId == postId)
                    .Include(p => p.UserId)
                    .Include(p => p.UserId.EarningEvents)
                    .Include(p => p.VotesUp)
                    .Include(p => p.VotesDown)
                    .Include(p => p.Group)
                    .FirstOrDefault();

                scoreAdj = ReputationService.GetReputationAdjustedAmount(
                    amount: amount * (isUpvote ? 1 : -1),
                    targetRep: isUpvote ? 0 : post.UserId.Reputation,
                    actorRep: 0);

                bool isAnonymous = userAppId == null;

                var fromFunds = db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                var toFunds = db.Users
                        .Where(u => u.Id == post.UserId.Id)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                if (isAnonymous)  // Anonymous vote
                {
                    // Check if vote tx has been claimed
                    var vtx = db.LightningTransactions.FirstOrDefault(tx => tx.Id == txid);

                    if (vtx == null || vtx.IsSpent == true)
                    {
                        return false;
                    }

                    vtx.IsSpent = true;
                    db.SaveChanges();
                }

                // FINANCIAL
                RecordFundTransfers(
                    from: isAnonymous ? null    : fromFunds,
                    to:   isUpvote    ? toFunds : null,
                    group: post.Group,
                    amountFrom: amount,
                    amountTo: isUpvote      ? 0.6 * amount   : 0,
                    amountGroup: isUpvote   ? 0.2 * amount   : 0.8 * amount,
                    amountCommunity: 0.1 * amount,
                    amountZapread:   0.1 * amount);
                
                // END FINANCIAL

                // Adjust post score
                post.Score += Convert.ToInt32(scoreAdj);

                // Adjust post owner reputation
                bool isOwnPost = post.UserId.AppId == userAppId;
                post.UserId.Reputation += (isAnonymous ? 0 : 1) * (isOwnPost ? 0 : 1) * (isUpvote ? 1 : -1) * amount;

                // Keep track of how much this post has made for the owner
                post.TotalEarned += (isUpvote ? 1 : 0) * 0.6 * amount;

                // Earning event for post owner
                if (isUpvote)
                {
                    post.UserId.EarningEvents.Add(new EarningEvent()
                    {
                        Amount = 0.6 * amount,
                        OriginType = 0,
                        TimeStamp = DateTime.UtcNow,
                        Type = 0,
                        OriginId = post.PostId,
                    });
                    post.UserId.TotalEarned += 0.6 * amount;
                }
                
                // Spending event for voter
                if (!isAnonymous)
                {
                    db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => u.SpendingEvents)
                        .FirstOrDefault()
                        .Add(new SpendingEvent()
                    {
                        Amount = amount,
                        Post = post,
                        TimeStamp = DateTime.UtcNow,
                    });
                }

                db.SaveChanges();

                if (isUpvote)
                {
                    _ = NotificationService.SendIncomeNotification(
                        0.6 * amount,
                        post.UserId.AppId,
                        "Post upvote",
                        "/Post/Detail/" + post.PostId);
                }

                return true;
            }
        }
    }
}