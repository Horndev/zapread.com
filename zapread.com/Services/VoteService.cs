﻿using Microsoft.Ajax.Utilities;
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
            UserFunds referredBy,
            Group group, 
            int originType,
            int originId,
            double amountFrom, 
            double amountTo, 
            double amountGroup,
            double amountCommunity,
            double amountZapread)
        {
            double referralPayment = 0.0;

            using (var db = new ZapContext())
            {
                var website = db.ZapreadGlobals.FirstOrDefault(i => i.Id == 1);

                if (website == null)
                {
                    throw new Exception(message: Properties.Resources.ErrorDatabaseNoWebsiteSettings);
                }

                if (group == null)
                {
                    website.CommunityEarnedToDistribute += amountGroup;
                }
                else
                {
                    group.TotalEarnedToDistribute += amountGroup;
                }

                if (referredBy != null)
                {
                    // Make a referral bonus payment - comes out of zapread funds (6%)
                    referralPayment = amountFrom * 0.06;
                    amountZapread -= referralPayment;
                    if (amountZapread < 0)
                    {
                        amountCommunity += amountZapread;
                        amountZapread = 0;
                    }
                }

                website.CommunityEarnedToDistribute += amountCommunity;
                website.ZapReadTotalEarned += amountZapread;
                website.ZapReadEarnedBalance += amountZapread;
                website.EarningEvents.Add(new EarningEvent() { 
                    Type = 3, //website
                    Amount = amountZapread,
                    TimeStamp = DateTime.UtcNow,
                    OriginType = originType,
                    OriginId = originId,
                });

                int attempts = 0;
                bool saveFailed;
                do
                {
                    attempts++;
                    saveFailed = false;

                    if (attempts < 50)
                    {
                        if (from != null) // ignore if we're not debiting a user - funds are from a new deposit
                        {
                            if (from.Balance < amountFrom)
                            {
                                throw new Exception(message: Properties.Resources.ErrorVoteFinanceUpdateBalance);
                            }
                            from.Balance -= amountFrom;
                        }

                        if (to != null) // downvotes could go to just community and group
                        {
                            to.Balance += amountTo;
                        }

                        if (referredBy != null)
                        {
                            // Both referred and referred by get 1/2 of the payment, so 3% each.
                            referredBy.Balance += 0.5 * referralPayment;
                            to.Balance += 0.5 * referralPayment;
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
                        throw new Exception(message: Properties.Resources.ErrorVoteFinanceUpdateBalanceHardFail);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
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

                bool isAnonymous = userAppId == null;

                var fromFunds = db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                var toFunds = db.Users
                        .Where(u => u.Id == post.UserId.Id)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                UserFunds referalFunds = null;
                User referalUser = null;

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

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: amount * (isUpvote ? 1 : -1),
                        targetRep: isUpvote ? 0 : post.UserId.Reputation,
                        actorRep: 0);
                }

                // Check if author has referral program active
                var referralInfo = db.Users
                       .Where(u => u.Id == post.UserId.Id)
                       .Select(u => u.ReferralInfo)
                       .AsNoTracking()
                       .FirstOrDefault();

                // Referral within last 6 months
                if ( referralInfo != null && ((DateTime.UtcNow - referralInfo.TimeStamp) < TimeSpan.FromDays(30*6)))
                {
                    var refbyAppId = referralInfo.ReferredByAppId;
                    referalFunds = db.Users
                        .Where(u => u.AppId == refbyAppId)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                    referalUser = db.Users
                        .Where(u => u.AppId == refbyAppId)
                        .Include(u => u.EarningEvents)
                        .FirstOrDefault();
                }

                // FINANCIAL
                double amountTo = isUpvote ? 0.6 * amount : 0;
                double amountCommunity = 0.1 * amount;
                double amountGroup = isUpvote ? 0.2 * amount : 0.8 * amount;
                if (post.IsNonIncome)
                {
                    // If the author declared the post as non-income, instead of getting
                    // money themselves, the author's payment goes to the community and group.
                    amountCommunity += amountTo * 0.5;
                    amountGroup += amountTo * 0.5;
                    amountTo = 0;
                }

                RecordFundTransfers(
                    from: isAnonymous ? null    : fromFunds,
                    to:   isUpvote    ? toFunds : null,
                    referredBy: isUpvote ? referalFunds : null,
                    group: post.Group,
                    originType: 0,
                    originId: post.PostId,
                    amountFrom: amount,
                    amountTo: amountTo,
                    amountGroup: amountGroup,
                    amountCommunity: amountCommunity,
                    amountZapread:   0.1 * amount);
                // END FINANCIAL

                // Adjust post owner reputation
                bool isOwnPost = post.UserId.AppId == userAppId;
                post.UserId.Reputation += (isAnonymous ? 0 : 1) * (isOwnPost ? 0 : 1) * (isUpvote ? 1 : -1) * amount;

                // Keep track of how much this post has made for the owner
                if (!post.IsNonIncome)
                {
                    post.TotalEarned += (isUpvote ? 1 : 0) * 0.6 * amount;
                }

                // Earning event for post owner
                if (isUpvote)
                {
                    if (!post.IsNonIncome)
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

                    if (referalFunds != null)
                    {
                        post.UserId.EarningEvents.Add(new EarningEvent()
                        {
                            Amount = 0.03 * amount,
                            OriginType = 0,
                            TimeStamp = DateTime.UtcNow,
                            Type = 4,
                            OriginId = post.PostId,
                        });

                        if (referalUser != null)
                        {
                            referalUser.EarningEvents.Add(new EarningEvent()
                            {
                                Amount = 0.03 * amount,
                                OriginType = 0,
                                TimeStamp = DateTime.UtcNow,
                                Type = 4,
                                OriginId = post.PostId,
                            });

                            // Notify the referrer they just made income
                            _ = NotificationService.SendIncomeNotification(
                                amount: 0.03 * amount,
                                userId: referalUser.AppId,
                                reason: "Referral Bonus",
                                clickUrl: "/Post/Detail/" + post.PostId);
                        }
                    }
                }
                
                // Spending event for voter
                if (!isAnonymous)
                {
                    var user = db.Users
                        .Where(u => u.AppId == userAppId)
                        .Include(u => u.SpendingEvents)
                        .Include(u => u.PostVotesUp)
                        .Include(u => u.PostVotesDown)
                        .FirstOrDefault();

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: amount * (isUpvote ? 1 : -1),
                        targetRep: isUpvote ? 0 : post.UserId.Reputation,
                        actorRep: user.Reputation);

                    user.SpendingEvents
                        .Add(new SpendingEvent()
                    {
                        Amount = amount,
                        Post = post,
                        TimeStamp = DateTime.UtcNow,
                    });

                    if (isUpvote)
                    {
                        post.VotesUp.Add(user);
                        user.PostVotesUp.Add(post);
                    }
                    else
                    {
                        post.VotesDown.Add(user);
                        user.PostVotesDown.Add(post);
                    }
                }

                // Adjust post score
                post.Score += Convert.ToInt32(scoreAdj);

                db.SaveChanges();

                if (isUpvote && !post.IsNonIncome)
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppId"></param>
        /// <param name="commentId"></param>
        /// <param name="amount"></param>
        /// <param name="isUpvote"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public bool CommentVote(string userAppId, int commentId, int amount, bool isUpvote, int txid)
        {
            double scoreAdj = 0.0;

            using (var db = new ZapContext())
            {
                var comment = db.Comments
                    .Where(c => c.CommentId == commentId)
                    .Include(c => c.VotesUp)
                    .Include(c => c.VotesDown)
                    .Include(c => c.UserId)
                    .Include(c => c.UserId.Funds)
                    .Include(c => c.Post)
                    .Include(c => c.Post.Group)
                    .FirstOrDefault();

                bool isAnonymous = userAppId == null;

                var fromFunds = db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                var toFunds = db.Users
                        .Where(u => u.Id == comment.UserId.Id)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                UserFunds referalFunds = null;
                User referalUser = null;

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

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: amount * (isUpvote ? 1 : -1),
                        targetRep: isUpvote ? 0 : comment.UserId.Reputation,
                        actorRep: 0);
                }

                // Check if author has referral program active
                var referralInfo = db.Users
                       .Where(u => u.Id == comment.UserId.Id)
                       .Select(u => u.ReferralInfo)
                       .AsNoTracking()
                       .FirstOrDefault();

                // Referral within last 6 months
                if (referralInfo != null && ((DateTime.UtcNow - referralInfo.TimeStamp) < TimeSpan.FromDays(30 * 6)))
                {
                    var refbyAppId = referralInfo.ReferredByAppId;
                    referalFunds = db.Users
                        .Where(u => u.AppId == refbyAppId)
                        .Select(u => u.Funds)
                        .FirstOrDefault();

                    referalUser = db.Users
                        .Where(u => u.AppId == refbyAppId)
                        .Include(u => u.EarningEvents)
                        .FirstOrDefault();
                }

                // FINANCIAL
                RecordFundTransfers(
                    from: isAnonymous ? null : fromFunds,
                    to: isUpvote ? toFunds : null,
                    referredBy: isUpvote ? referalFunds : null,
                    group: comment.Post.Group,
                    originType: 1,
                    originId: Convert.ToInt32(comment.CommentId),
                    amountFrom: amount,
                    amountTo: isUpvote ? 0.6 * amount : 0,
                    amountGroup: isUpvote ? 0.2 * amount : 0.8 * amount,
                    amountCommunity: 0.1 * amount,
                    amountZapread: 0.1 * amount);
                // END FINANCIAL


                // Adjust comment owner reputation
                bool isOwn = comment.UserId.AppId == userAppId;
                comment.UserId.Reputation += (isAnonymous ? 0 : 1) * (isOwn ? 0 : 1) * (isUpvote ? 1 : -1) * amount;

                // Earning event for post owner
                if (isUpvote)
                {
                    comment.UserId.EarningEvents.Add(new EarningEvent()
                    {
                        Amount = 0.6 * amount,
                        OriginType = 1,
                        TimeStamp = DateTime.UtcNow,
                        Type = 0,
                        OriginId = Convert.ToInt32(comment.CommentId),
                    });
                    comment.UserId.TotalEarned += 0.6 * amount;

                    if (referalFunds != null)
                    {
                        comment.UserId.EarningEvents.Add(new EarningEvent()
                        {
                            Amount = 0.03 * amount,
                            OriginType = 1,
                            TimeStamp = DateTime.UtcNow,
                            Type = 4,
                            OriginId = Convert.ToInt32(comment.CommentId),
                        });

                        if (referalUser != null)
                        {
                            referalUser.EarningEvents.Add(new EarningEvent()
                            {
                                Amount = 0.03 * amount,
                                OriginType = 1,
                                TimeStamp = DateTime.UtcNow,
                                Type = 4,
                                OriginId = Convert.ToInt32(comment.CommentId),
                            });

                            _ = NotificationService.SendIncomeNotification(
                                amount: 0.03 * amount,
                                userId: referalUser.AppId,
                                reason: "Referral Bonus",
                                clickUrl: "/Post/Detail/" + comment.Post.PostId);
                        }
                    }
                }

                // Spending event for voter
                if (!isAnonymous)
                {
                    var user = db.Users
                        .Where(u => u.AppId == userAppId)
                        .Include(u => u.SpendingEvents)
                        .Include(u => u.PostVotesUp)
                        .Include(u => u.PostVotesDown)
                        .FirstOrDefault();

                    scoreAdj = ReputationService.GetReputationAdjustedAmount(
                        amount: amount * (isUpvote ? 1 : -1),
                        targetRep: isUpvote ? 0 : comment.UserId.Reputation,
                        actorRep: user.Reputation);

                    user.SpendingEvents
                        .Add(new SpendingEvent()
                        {
                            Amount = amount,
                            Comment = comment,
                            TimeStamp = DateTime.UtcNow,
                        });

                    if (isUpvote)
                    {
                        comment.VotesUp.Add(user);
                        user.CommentVotesUp.Add(comment);
                    }
                    else
                    {
                        comment.VotesDown.Add(user);
                        user.CommentVotesDown.Add(comment);
                    }
                }

                // Adjust post score
                comment.Score += Convert.ToInt32(scoreAdj);

                db.SaveChanges();

                if (isUpvote)
                {
                    _ = NotificationService.SendIncomeNotification(
                        0.6 * amount, 
                        comment.UserId.AppId, 
                        "Comment upvote",
                        "/Post/Detail/" + comment.Post.PostId );
                }

                return true;
            }
        }
    }
}