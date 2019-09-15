using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Models.Database;

namespace zapread.com.Services
{
    public class PayoutsService
    {
        public void CommunityPayout()
        {
            int maxDistributions = 1000;    // Per run
            int minDistributionSize = 1;    // Go as low as 1 Satoshi

            using (var db = new ZapContext())
            {
                var website = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    .FirstOrDefault();

                if (website == null)
                {
                    throw new Exception("Unable to load website settings.");
                }

                Dictionary<int, double> payoutUserAmount = new Dictionary<int, double>();

                // This is how much is in the community pool for distribution
                var toDistribute = Math.Floor(website.CommunityEarnedToDistribute);

                if (toDistribute < 0)
                {
                    toDistribute = 0;

                    Services.MailingService.SendErrorNotification(
                        title: "Community payout error",
                        message: "Error during community distribution.  Total to distribute is negative.");
                }

                var numDistributions = Convert.ToInt32(Math.Min(toDistribute / minDistributionSize, maxDistributions));
                if (numDistributions > 0)
                {
                    var sitePostsRecent = db.Posts
                        .Where(p => p.Score > 0)
                        .Where(p => DbFunctions.DiffDays(p.TimeStamp, DateTime.UtcNow) <= 30)
                        .Where(p => !p.IsDeleted)
                        .Where(p => !p.IsDraft)
                        .ToList();
                    var sitePostsOld = db.Posts
                        .Where(p => p.Score > 0)
                        .Where(p => DbFunctions.DiffDays(p.TimeStamp, DateTime.UtcNow) > 30)
                        .Where(p => !p.IsDeleted)
                        .Where(p => !p.IsDraft)
                        .ToList();

                    var numPostsOld = sitePostsOld.Count();
                    var numPostsNew = sitePostsRecent.Count();

                    var newFrac = numPostsOld == 0 ? 1.0 : 0.5;
                    var oldFrac = numPostsOld == 0 ? 0.0 : 0.5;

                    List<Post> postsToDistribute;

                    if (numPostsNew < numDistributions* newFrac)
                    {
                        // Too few posts, so each post is selected
                        postsToDistribute = sitePostsRecent;
                    }
                    else
                    {
                        // Need to Randomly choose posts to distribute to
                        Random rnd = new Random();
                        postsToDistribute = sitePostsRecent.OrderBy(ps => rnd.Next()).Take((int)Math.Floor(numDistributions*newFrac)).ToList();
                    }

                    double totalScores = 1.0 * postsToDistribute.Select(p => p.Score).Sum();
                    foreach (var p in postsToDistribute)
                    {
                        var score = Math.Max(1.0 * p.Score, 0.0);
                        var frac = score / totalScores;
                        var earnedAmount = newFrac * frac * toDistribute;
                        if (earnedAmount > 0)
                        {
                            var owner = p.UserId;
                            if (owner != null)
                            {
                                // Record and increment user payment to be saved to DB later.
                                if (payoutUserAmount.ContainsKey(owner.Id))
                                {
                                    payoutUserAmount[owner.Id] += earnedAmount;
                                }
                                else
                                {
                                    payoutUserAmount[owner.Id] = earnedAmount;
                                }
                            }
                        }
                    }

                    if (numPostsOld < numDistributions* oldFrac)
                    {
                        // Too few posts, so each post is selected
                        postsToDistribute = sitePostsOld;
                    }
                    else
                    {
                        // Need to Randomly choose posts to distribute to
                        Random rnd = new Random();
                        postsToDistribute = sitePostsOld.OrderBy(ps => rnd.Next()).Take((int)(numDistributions*oldFrac)).ToList();
                    }

                    totalScores = 1.0 * postsToDistribute.Select(p => p.Score).Sum();
                    foreach (var p in postsToDistribute)
                    {
                        var score = 1.0 * p.Score;
                        var frac = score / totalScores;
                        var earnedAmount = oldFrac * frac * toDistribute;
                        var owner = p.UserId;
                        if (owner != null)
                        {
                            // Record and increment user payment to be saved to DB later.
                            if (payoutUserAmount.ContainsKey(owner.Id))
                            {
                                payoutUserAmount[owner.Id] += earnedAmount;
                            }
                            else
                            {
                                payoutUserAmount[owner.Id] = earnedAmount;
                            }
                        }
                    }

                    // apply distribution to DB
                    var distributed = 0.0;
                    foreach (var uid in payoutUserAmount.Keys)
                    {
                        // This is where payouts should be made for each user link to group
                        var owner = db.Users.FirstOrDefault(u => u.Id == uid);
                        double earnedAmount = payoutUserAmount[uid];
                        var ea = new EarningEvent()
                        {
                            Amount = earnedAmount,
                            OriginType = 0,                 // 0 = post
                            TimeStamp = DateTime.UtcNow,
                            Type = 2,                       // 2 = community
                            OriginId = 0,                   // Indicates the group which generated the payout
                        };
                        owner.EarningEvents.Add(ea);
                        owner.TotalEarned += earnedAmount;
                        owner.Funds.Balance += earnedAmount;
                        distributed += earnedAmount;
                    }

                    //record distribution
                    website.CommunityEarnedToDistribute -= distributed;
                    website.TotalEarnedCommunity += distributed;

                    db.SaveChanges();
                }
            }
        }

        public void GroupsPayout()
        {

        }
    }
}