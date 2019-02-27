using LightningLib.lndrpc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models;

namespace zapread.com.Services
{
    public class LNTransactionMonitor
    {
        public void CheckLNTransactions()
        {
            using (var db = new ZapContext())
            {
                var website = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    .AsNoTracking()
                    .FirstOrDefault();

                LndRpcClient lndClient = new LndRpcClient(
                    host: website.LnMainnetHost,
                    macaroonAdmin: website.LnMainnetMacaroonAdmin,
                    macaroonRead: website.LnMainnetMacaroonRead,
                    macaroonInvoice: website.LnMainnetMacaroonInvoice);

                // These are the unpaid invoices in database
                var unpaidInvoices = db.LightningTransactions
                    .Where(t => t.IsSettled == false)
                    .Where(t => t.IsDeposit == true)
                    .Where(t => t.IsIgnored == false)
                    .Include(t => t.User)
                    .Include(t => t.User.Funds);

                //var invoiceDebug = unpaidInvoices.ToList();
                foreach (var i in unpaidInvoices)
                {
                    if (i.HashStr != null)
                    {
                        var inv = lndClient.GetInvoice(rhash: i.HashStr);
                        if (inv.settled != null && inv.settled == true)
                        {
                            // Paid but not applied in DB
                            var use = i.UsedFor;
                            if (use == TransactionUse.VotePost)
                            {
                                //if (false) // Disable for now
                                //{
                                //    var vc = new VoteController();
                                //    var v = new VoteController.Vote()
                                //    {
                                //        a = Convert.ToInt32(i.Amount),
                                //        d = i.UsedForAction == TransactionUseAction.VoteDown ? 0 : 1,
                                //        Id = i.UsedForId,
                                //        tx = i.Id
                                //    };
                                //    await vc.Post(v);

                                //    i.IsSpent = true;
                                //    i.IsSettled = true;
                                //    i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                //}
                            }
                            else if (use == TransactionUse.VoteComment)
                            {
                                ;
                            }
                            else if (use == TransactionUse.UserDeposit)
                            {
                                if (i.User == null)
                                {
                                    // Not sure how to deal with this other than add funds to Community
                                    website.CommunityEarnedToDistribute += i.Amount;
                                    i.IsSpent = true;
                                    i.IsSettled = true;
                                    i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                }
                                else
                                {
                                    // Deposit funds in user account
                                    var user = i.User;
                                    user.Funds.Balance += i.Amount;
                                    i.IsSettled = true;
                                    i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;

                                }
                            }
                            else if (use == TransactionUse.Undefined)
                            {
                                if (i.User == null)
                                {
                                    // Not sure how to deal with this other than add funds to Community
                                    website.CommunityEarnedToDistribute += i.Amount;
                                    i.IsSpent = true;
                                    i.IsSettled = true;
                                    i.TimestampSettled = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(inv.settle_date)).UtcDateTime;
                                }
                                else
                                {
                                    // Not sure what the user was doing - deposit into their account.
                                    ;
                                }
                            }
                        }
                        else
                        {
                            // Not settled - check expiry
                            var t1 = Convert.ToInt64(inv.creation_date);
                            var tNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            var tExpire = t1 + Convert.ToInt64(inv.expiry) + 10000; //Add a buffer time
                            if (tNow > tExpire)
                            {
                                // Expired - let's stop checking this invoice
                                i.IsIgnored = true;
                            }
                            else
                            {
                                ; // keep waiting
                            }
                        }
                    }
                    else
                    {
                        // No hash string to look it up.  Must be an error somewhere.
                        i.IsIgnored = true;
                    }
                }

                db.SaveChanges();

                // These are non-settled withdraws in the database
                var unpaidWithdraws = db.LightningTransactions
                    .Where(t => t.IsSettled == false)   // Not settled
                    .Where(t => t.IsDeposit == false)   // Withdraw
                    .Where(t => t.IsIgnored == false)   // Still valid
                    .Include(t => t.User)
                    .Include(t => t.User.Funds);

                foreach (var i in unpaidWithdraws)
                {

                }

                db.SaveChanges();
            }
        }
    }
}