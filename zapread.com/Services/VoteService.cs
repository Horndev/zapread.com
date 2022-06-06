using Microsoft.Ajax.Utilities;
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
                website.EarningEvents = new List<EarningEvent> {
                    new EarningEvent() {
                        Type = 3, //website
                        Amount = amountZapread,
                        TimeStamp = DateTime.UtcNow,
                        OriginType = originType,
                        OriginId = originId,
                    }
                };

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
                var postInfo = db.Posts
                    .Where(p => p.PostId == postId)
                    .Select(p => new
                    {
                        Post = p,
                        p.Group,
                        User = p.UserId,
                        UserId = p.UserId.Id,
                        UserAppId = p.UserId.AppId,
                        p.UserId.Reputation,
                        p.UserId.ReferralInfo,
                        p.IsNonIncome,
                        p.PostId,
                        ToFunds = isUpvote ? p.UserId.Funds : null // Don't fetch this if not upvoting
                    })
                    .FirstOrDefault();

                bool isAnonymous = userAppId == null;

                UserFunds fromFunds = null;

                if (!isAnonymous)
                {
                    fromFunds = db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => u.Funds)
                        .FirstOrDefault();
                }

                var toFunds = postInfo.ToFunds;

                UserFunds referalFunds = null;
                User referalUser = null;
                User postAuthor = postInfo.User;
                    //db.Users
                    //.Where(u => u.Id == postInfo.UserId)
                    //.FirstOrDefault();

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
                        targetRep: isUpvote ? 0 : postInfo.Reputation,
                        actorRep: 0);
                }

                // Check if author has referral program active
                var referralInfo = postInfo.ReferralInfo;
                    //db.Users
                    //   .Where(u => u.Id == postInfo.UserId)
                    //   .Select(u => u.ReferralInfo)
                    //   .AsNoTracking()
                    //   .FirstOrDefault();

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
                        //.Include(u => u.EarningEvents)
                        .FirstOrDefault();
                }

                // FINANCIAL
                double amountTo = isUpvote ? 0.6 * amount : 0;
                double amountCommunity = 0.1 * amount;
                double amountGroup = isUpvote ? 0.2 * amount : 0.8 * amount;
                if (postInfo.IsNonIncome)
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
                    group: postInfo.Group,
                    originType: 0,
                    originId: postInfo.PostId,
                    amountFrom: amount,
                    amountTo: amountTo,
                    amountGroup: amountGroup,
                    amountCommunity: amountCommunity,
                    amountZapread:   0.1 * amount);
                // END FINANCIAL

                bool saveFailed;
                int attempts = 0;

                do
                {
                    attempts++;
                    saveFailed = false;

                    if (attempts > 50)
                    {
                        //?
                    }

                    // Adjust post owner reputation
                    bool isOwnPost = postInfo.UserAppId == userAppId;
                    postAuthor.Reputation += (isAnonymous ? 0 : 1) * (isOwnPost ? 0 : 1) * (isUpvote ? 1 : -1) * amount;

                    // Keep track of how much this post has made for the owner
                    if (!postInfo.IsNonIncome)
                    {
                        postInfo.Post.TotalEarned += (isUpvote ? 1 : 0) * 0.6 * amount;
                    }

                    // Earning event for post owner
                    if (isUpvote)
                    {
                        if (!postInfo.IsNonIncome)
                        {
                            var ea = new EarningEvent()
                            {
                                Amount = 0.6 * amount,
                                OriginType = 0,
                                TimeStamp = DateTime.UtcNow,
                                Type = 0,
                                OriginId = postInfo.PostId,
                            };

                            postAuthor.EarningEvents = new List<EarningEvent> { ea };
                            postAuthor.TotalEarned += 0.6 * amount;
                        }

                        if (referalFunds != null)
                        {
                            var ea = new EarningEvent()
                            {
                                Amount = 0.03 * amount,
                                OriginType = 0,
                                TimeStamp = DateTime.UtcNow,
                                Type = 4,
                                OriginId = postInfo.PostId,
                            };

                            // Need to do this to not overwrite if EarningEvent created prior
                            if (postAuthor.EarningEvents != null)
                            {
                                postAuthor.EarningEvents.Add(ea);
                            }
                            else
                            {
                                postAuthor.EarningEvents = new List<EarningEvent> { ea };
                            }

                            if (referalUser != null)
                            {
                                referalUser.EarningEvents = new List<EarningEvent> {
                                    new EarningEvent()
                                    {
                                        Amount = 0.03 * amount,
                                        OriginType = 0,
                                        TimeStamp = DateTime.UtcNow,
                                        Type = 4,
                                        OriginId = postInfo.PostId,
                                    }
                                };

                                // Notify the referrer they just made income
                                _ = NotificationService.SendIncomeNotification(
                                    amount: 0.03 * amount,
                                    userId: referalUser.AppId,
                                    reason: "Referral Bonus",
                                    clickUrl: "/Post/Detail/" + postInfo.PostId);
                            }
                        }
                    }
                
                    // Spending event for voter
                    if (!isAnonymous)
                    {
                        var userInfo = db.Users
                            .Where(u => u.AppId == userAppId)
                            .Select(u => new
                            {
                                User = u,
                                u.Reputation,
                                Upvoted = u.PostVotesUp.Select(p => p.PostId).Contains(postInfo.PostId),
                                Downvoted = u.PostVotesDown.Select(p => p.PostId).Contains(postInfo.PostId),
                            })
                            .FirstOrDefault();

                        scoreAdj = ReputationService.GetReputationAdjustedAmount(
                            amount: amount * (isUpvote ? 1 : -1),
                            targetRep: isUpvote ? 0 : postInfo.Reputation,
                            actorRep: userInfo.Reputation);

                        userInfo.User.SpendingEvents = new List<SpendingEvent> { 
                            new SpendingEvent()
                            {
                                Amount = amount,
                                Post = postInfo.Post,
                                TimeStamp = DateTime.UtcNow,
                            } 
                        };

                        if (isUpvote)
                        {
                            if (!userInfo.Upvoted)
                            {
                                postInfo.Post.VotesUp = new List<User> { userInfo.User }; // Adds without DB round-trip
                                //userInfo.User.PostVotesUp = new List<Post> { post }; //.Add(post);
                            }
                        }
                        else
                        {
                            if (!userInfo.Downvoted)
                            {
                                postInfo.Post.VotesDown = new List<User> { userInfo.User }; // Adds without DB round-trip
                                //userInfo.User.PostVotesDown = new List<Post> { post }; //.Add(post);
                            }
                        }
                    }

                    // Adjust post score
                    postInfo.Post.Score += Convert.ToInt32(scoreAdj);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
                    {
                        saveFailed = true;
                        foreach (var entry in ex.Entries)//.Single();
                        {
                            entry.Reload();
                        }
                    }
                }
                while (saveFailed);

                if (isUpvote && !postInfo.IsNonIncome)
                {
                    _ = NotificationService.SendIncomeNotification(
                        0.6 * amount,
                        postInfo.UserAppId,
                        "Post upvote",
                        "/Post/Detail/" + postInfo.PostId);
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
                var commentInfo = db.Comments
                    .Where(c => c.CommentId == commentId)
                    .Select(c => new
                    {
                        Comment = c,
                        c.Post.Group,
                        User = c.UserId,
                        UserId = c.UserId.Id,
                        UserAppId = c.UserId.AppId,
                        c.UserId.Reputation,
                        c.UserId.ReferralInfo,
                        c.CommentId,
                        c.Post.PostId,
                        ToFunds = isUpvote ? c.UserId.Funds : null // Don't fetch if not upvoting
                    })
                    //.Include(c => c.VotesUp)
                    //.Include(c => c.VotesDown)
                    //.Include(c => c.UserId)
                    //.Include(c => c.UserId.Funds)
                    //.Include(c => c.Post)
                    //.Include(c => c.Post.Group)
                    .FirstOrDefault();

                bool isAnonymous = userAppId == null;

                UserFunds fromFunds = null;

                if (!isAnonymous)
                {
                    fromFunds = db.Users
                            .Where(u => u.AppId == userAppId)
                            .Select(u => u.Funds)
                            .FirstOrDefault();
                }

                var toFunds = commentInfo.ToFunds;
                    //db.Users
                    //    .Where(u => u.Id == comment.UserId.Id)
                    //    .Select(u => u.Funds)
                    //    .FirstOrDefault();

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
                        targetRep: isUpvote ? 0 : commentInfo.Reputation,
                        actorRep: 0);
                }

                // Check if author has referral program active
                var referralInfo = commentInfo.ReferralInfo;
                    //db.Users
                    //   .Where(u => u.Id == comment.UserId.Id)
                    //   .Select(u => u.ReferralInfo)
                    //   .AsNoTracking()
                    //   .FirstOrDefault();

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
                        //.Include(u => u.EarningEvents)
                        .FirstOrDefault();
                }

                // FINANCIAL
                RecordFundTransfers(
                    from: isAnonymous ? null : fromFunds,
                    to: isUpvote ? toFunds : null,
                    referredBy: isUpvote ? referalFunds : null,
                    group: commentInfo.Group,
                    originType: 1,
                    originId: Convert.ToInt32(commentInfo.CommentId),
                    amountFrom: amount,
                    amountTo: isUpvote ? 0.6 * amount : 0,
                    amountGroup: isUpvote ? 0.2 * amount : 0.8 * amount,
                    amountCommunity: 0.1 * amount,
                    amountZapread: 0.1 * amount);
                // END FINANCIAL - Things are saved at this point

                bool saveFailed;
                int attempts = 0;

                do
                {
                    attempts++;
                    saveFailed = false;

                    if (attempts > 50)
                    {
                        //?
                    }

                    // Adjust comment owner reputation
                    bool isOwn = commentInfo.UserAppId == userAppId;
                    commentInfo.User.Reputation += (isAnonymous ? 0 : 1) * (isOwn ? 0 : 1) * (isUpvote ? 1 : -1) * amount;

                    // Earning event for post owner
                    if (isUpvote)
                    {
                        commentInfo.User.EarningEvents = new List<EarningEvent> {
                            new EarningEvent()
                            {
                                Amount = 0.6 * amount,
                                OriginType = 1,
                                TimeStamp = DateTime.UtcNow,
                                Type = 0,
                                OriginId = Convert.ToInt32(commentInfo.CommentId),
                            }
                        };

                        commentInfo.User.TotalEarned += 0.6 * amount;

                        if (referalFunds != null)
                        {
                            var ea = new EarningEvent()
                            {
                                Amount = 0.03 * amount,
                                OriginType = 1,
                                TimeStamp = DateTime.UtcNow,
                                Type = 4,
                                OriginId = Convert.ToInt32(commentInfo.CommentId),
                            };
                            if (commentInfo.User.EarningEvents != null)
                            {
                                commentInfo.User.EarningEvents.Add(ea);
                            }
                            else
                            {
                                commentInfo.User.EarningEvents = new List<EarningEvent> { ea };
                            }

                            if (referalUser != null)
                            {
                                referalUser.EarningEvents = new List<EarningEvent> {
                                    new EarningEvent()
                                    {
                                        Amount = 0.03 * amount,
                                        OriginType = 1,
                                        TimeStamp = DateTime.UtcNow,
                                        Type = 4,
                                        OriginId = Convert.ToInt32(commentInfo.CommentId),
                                    }
                                };

                                _ = NotificationService.SendIncomeNotification(
                                    amount: 0.03 * amount,
                                    userId: referalUser.AppId,
                                    reason: "Referral Bonus",
                                    clickUrl: "/Post/Detail/" + commentInfo.PostId);
                            }
                        }
                    }

                    // Spending event for voter
                    if (!isAnonymous)
                    {
                        var userInfo = db.Users
                            .Where(u => u.AppId == userAppId)
                            .Select(u => new
                            {
                                User = u,
                                u.Reputation,
                                Upvoted = u.CommentVotesUp.Select(c => c.CommentId).Contains(commentInfo.CommentId),
                                Downvoted = u.CommentVotesDown.Select(c => c.CommentId).Contains(commentInfo.CommentId),
                            })
                            .FirstOrDefault();

                        scoreAdj = ReputationService.GetReputationAdjustedAmount(
                            amount: amount * (isUpvote ? 1 : -1),
                            targetRep: isUpvote ? 0 : commentInfo.Reputation,
                            actorRep: userInfo.Reputation);

                        userInfo.User.SpendingEvents = new List<SpendingEvent> {
                            new SpendingEvent()
                            {
                                Amount = amount,
                                Comment = commentInfo.Comment,
                                TimeStamp = DateTime.UtcNow,
                            }
                        };

                        if (isUpvote)
                        {
                            if (!userInfo.Upvoted)
                            {
                                commentInfo.Comment.VotesUp = new List<User> { userInfo.User }; // Adds without DB round-trip
                            }
                            //comment.VotesUp.Add(user);
                            //user.CommentVotesUp.Add(comment);
                        }
                        else
                        {
                            if (!userInfo.Downvoted)
                            {
                                commentInfo.Comment.VotesDown = new List<User> { userInfo.User }; // Adds without DB round-trip
                            }
                            //comment.VotesDown.Add(user);
                            //user.CommentVotesDown.Add(comment);
                        }
                    }

                    // Adjust post score
                    commentInfo.Comment.Score += Convert.ToInt32(scoreAdj);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
                    {
                        saveFailed = true;
                        foreach (var entry in ex.Entries)//.Single();
                        {
                            entry.Reload();
                        }
                    }
                }
                while (saveFailed);

                if (isUpvote)
                {
                    _ = NotificationService.SendIncomeNotification(
                        0.6 * amount, 
                        commentInfo.UserAppId, 
                        "Comment upvote",
                        "/Post/Detail/" + commentInfo.PostId );
                }

                return true;
            }
        }
    }
}