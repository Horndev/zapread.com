using LightningLib.lndrpc;
using System;
using System.Data.Entity;
using System.Linq;
using zapread.com.Database;
using zapread.com.Models;

namespace zapread.com.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class LNTransactionMonitor
    {
        private static bool running = false;
        /// <summary>
        /// Synchronize the database with the Lightning Node
        /// </summary>
        public void SyncNode()
        {
            using (var db = new ZapContext())
            {
                if (running)
                {
                    return;
                }
                running = true;

                var website = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    //.AsNoTracking()
                    .FirstOrDefault();

                if (website == null)
                {
                    throw new Exception("Unable to load website settings.");
                }

                LndRpcClient lndClient = GetLNDClient(website);

                int step = 300;
                int start = 63000;
                int max = 30;

                var paymentsResult = lndClient.GetPayments(
                    include_incomplete: true, // Important for checking
                    //reversed: true, // Start with most recent and work backwards
                    max_payments: step);

                var paymentsResultEnd = lndClient.GetPayments(
                    include_incomplete: true, // Important for checking
                    reversed: true, // Start with most recent and work backwards
                    max_payments: 1);

                start = Convert.ToInt32(paymentsResult.first_index_offset);
                max += Convert.ToInt32(paymentsResultEnd.last_index_offset);
                bool updated = false;
                bool flagged = false;
                while (start < max)
                {
                    foreach (var payment in paymentsResult.payments)
                    {
                        var payment_hash = payment.payment_hash;
                        var invoice = payment.payment_request;

                        var dbMatches = db.LightningTransactions
                            .Where(tx => tx.PaymentRequest == invoice)
                            .ToList();

                        if (dbMatches.Count > 0)
                        {
                            if (dbMatches.Count == 1)
                            {
                                var tx = dbMatches[0];
                                if (payment.payment_hash!= null && tx.PaymentHash == null)
                                {
                                    tx.PaymentHash = payment.payment_hash;
                                    updated = true;
                                }
                                if (payment.failure_reason != null && tx.FailureReason == null)
                                {
                                    tx.FailureReason = payment.failure_reason;
                                    updated = true;
                                }
                                if (payment.payment_index != null && tx.PaymentIndex == null)
                                {
                                    tx.PaymentIndex = Convert.ToInt32(payment.payment_index);
                                    updated = true;
                                }
                                if (payment.payment_preimage != null && tx.PaymentPreimage == null)
                                {
                                    tx.PaymentPreimage = payment.payment_preimage;
                                    updated = true;
                                }
                                if (payment.status != null && tx.PaymentStatus == null)
                                {
                                    tx.PaymentStatus = payment.status;
                                    updated = true;
                                    if (payment.status == "UNKNOWN")
                                    {
                                        // not sure what happened.
                                        flagged = true;
                                    }
                                    if (!tx.IsSettled && payment.status == "SUCCEEDED")
                                    {
                                        // LND marked as settled but not in db!
                                        updated = false; // Don't save for now
                                        flagged = true;
                                    }
                                    if (tx.IsSettled && (payment.status == "FAILED" || payment.status == "IN_FLIGHT"))
                                    {
                                        // We settled an invoice we should not have
                                        updated = false; // Don't save for now
                                        flagged = true;
                                    }
                                }
                                if (updated)
                                {
                                    tx.TimestampUpdated = DateTime.UtcNow;
                                    db.SaveChanges();
                                }
                                else if (flagged)
                                {
                                    ;
                                    if (tx.IsLimbo)
                                    {
                                        ;
                                    }
                                }
                            }
                            else
                            {
                                ; // shouldn't be
                            }
                            updated = false; // for next round
                            flagged = false;
                        }
                    }

                    start = Convert.ToInt32(paymentsResult.last_index_offset);

                    //get next batch
                    paymentsResult = lndClient.GetPayments(
                       include_incomplete: true, // Important for checking
                       index_offset: start,
                       max_payments: step);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void CheckLNTransactions()
        {
            using (var db = new ZapContext())
            {
                var website = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    //.AsNoTracking() // need to track since it will get updated
                    .FirstOrDefault();

                if (website == null)
                {
                    throw new Exception("Unable to load website settings.");
                }

                LndRpcClient lndClient = GetLNDClient(website);

                //var invv = lndClient.GetInvoice("8Td4xGBvz4nI2qRLIVC93S9mcTDodd/sylhd9IG7FEA=", out string responseStr, useQuery: false);
                //var allpayments = lndClient.GetPayments(out string responseStr, include_incomplete: true);

                // ** DANGER ZONE **
                //lndClient.DeletePayments(out string responseStr);
                // *****************

                // These are the unpaid invoices in database (incoming payments)
                var unpaidInvoices = db.LightningTransactions
                    .Where(t => t.IsSettled == false)
                    .Where(t => t.IsDeposit == true)
                    .Where(t => t.IsIgnored == false)
                    .Include(t => t.User)
                    .Include(t => t.User.Funds);

                var invoiceDebug = unpaidInvoices.ToList();
                foreach (var i in unpaidInvoices)
                {
                    if (i.HashStr != null)
                    {
                        var inv = lndClient.GetInvoice(rhash: i.HashStr);
                        if (inv != null && inv.settled != null && inv.settled == true)
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
                                // Not handled yet
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
                        else if (inv != null)
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
                    .Where(t => t.IsLimbo)              // Only check those in limbo
                    .Include(t => t.User)
                    .Include(t => t.User.Funds)
                    .OrderByDescending(t => t.TimestampCreated);

                var numup = unpaidWithdraws.Count();

                if (numup > 0)
                {
                    // Check the unpaid withdraws
                    var payments = lndClient.GetPayments(include_incomplete: true);

                    foreach (var i in unpaidWithdraws)
                    {
                        var pmt = payments.payments.Where(p => p.payment_hash == i.HashStr).FirstOrDefault();
                        double amount = Convert.ToDouble(i.Amount);

                        if (pmt != null)
                        {
                            // Paid?
                            if (pmt.status == "SUCCEEDED")
                            {
                                // Payment succeeded - remove from Limbo
                                if (i.IsLimbo)
                                {
                                    if (i.User.Funds.LimboBalance - amount < 0)
                                    {
                                        // shouldn't happen!
                                        i.User.Funds.LimboBalance = 0;
                                        Services.MailingService.SendErrorNotification(
                                            title: "Tx caused limbo to be negative. - payment verified",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason -1");
                                    }
                                    else
                                    {
                                        i.User.Funds.LimboBalance -= amount;
                                        if (i.User.Funds.LimboBalance < 0) // shouldn't happen!
                                        {
                                            i.User.Funds.LimboBalance = 0;
                                        }
                                    }
                                }

                                i.IsLimbo = false;
                                i.IsIgnored = true;
                                i.IsSettled = true;

                                Services.MailingService.SendErrorNotification(
                                            title: "Tx marked as ignored - payment verified",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason 0");
                            }
                            else if (pmt.status == "FAILED")
                            {
                                // Payment failed - refund user
                                if (i.User.Funds.LimboBalance - amount < 0)
                                {
                                    if (i.User.Funds.LimboBalance < 0) // shouldn't happen!
                                    {
                                        i.User.Funds.LimboBalance = 0;
                                    }
                                    i.User.Funds.Balance += i.User.Funds.LimboBalance;
                                    i.User.Funds.LimboBalance = 0;
                                }
                                else
                                {
                                    i.User.Funds.LimboBalance -= amount;
                                    i.User.Funds.Balance += amount;
                                    if (i.User.Funds.LimboBalance < 0) // shouldn't happen!
                                    {
                                        i.User.Funds.LimboBalance = 0;
                                    }
                                }
                                i.IsLimbo = false;
                                i.IsIgnored = true;
                                i.IsSettled = false;
                                Services.MailingService.SendErrorNotification(
                                            title: "Tx marked as ignored - failure verified",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason 0");
                            }

                            // I really don't like these next options!!  Should verify validity
                            else if (i.ErrorMessage == "Error: invoice is already paid")
                            {
                                // Invoice is already paid - 

                                // This was a duplicate payment - funds were not sent and this payment hash should only have one paid version.
                                if (i.User.Funds.LimboBalance - amount < 0)
                                {
                                    if (i.User.Funds.LimboBalance < 0) // shouldn't happen!
                                    {
                                        i.User.Funds.LimboBalance = 0;
                                    }
                                    i.User.Funds.Balance += i.User.Funds.LimboBalance;
                                    i.User.Funds.LimboBalance = 0;
                                }
                                else
                                {
                                    i.User.Funds.LimboBalance -= amount;
                                    i.User.Funds.Balance += amount;
                                    if (i.User.Funds.LimboBalance < 0) // shouldn't happen!
                                    {
                                        i.User.Funds.LimboBalance = 0;
                                    }
                                }
                                i.IsLimbo = false;
                                i.IsIgnored = true;
                                i.IsSettled = false;
                                Services.MailingService.SendErrorNotification(
                                            title: "Tx marked as ignored",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason 1");
                            }
                            else if (i.ErrorMessage == "Error: payment is in transition")
                            {
                                // Double spend attempt stopped.  No loss of funds
                                i.IsIgnored = true;
                                Services.MailingService.SendErrorNotification(
                                            title: "Tx marked as ignored",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason 2");
                            }
                            else if (i.ErrorMessage == "Error: FinalExpiryTooSoon")
                            {
                                i.IsIgnored = true;
                                Services.MailingService.SendErrorNotification(
                                            title: "Tx marked as ignored",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason 3");
                            }
                            else if (i.ErrorMessage == "Error validating payment." || i.ErrorMessage == "Error executing payment.")
                            {
                                // Payment has come through

                                // No longer in limbo
                                i.User.Funds.LimboBalance -= amount;
                                if (i.User.Funds.LimboBalance < 0)
                                {
                                    // Should not happen!
                                    i.User.Funds.LimboBalance = 0;
                                }
                                i.IsIgnored = true;
                                i.IsSettled = true;
                                i.IsLimbo = false;
                                Services.MailingService.SendErrorNotification(
                                            title: "User withdraw limbo complete (settled)",
                                            message: "Withdraw Invoice completed limbo (payment was found)."
                                                + "\r\n invoice: " + i.PaymentRequest
                                                + "\r\n user: " + i.User.Name + "(" + i.User.AppId + ")"
                                                + "\r\n amount: " + Convert.ToString(i.Amount)
                                                + "\r\n error: " + (i.ErrorMessage ?? "null"));
                            }
                            else
                            {
                                // Payment may have gone through without recording in DB.
                                ;
                                if (i.ErrorMessage != null)
                                {
                                    i.IsError = true;
                                }
                                i.IsSettled = true;
                                Services.MailingService.SendErrorNotification(
                                            title: "Tx marked as ignored",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason 4");
                            }
                        }
                        else
                        {
                            // Consider as not paid (for now) if not in DB - probably an error
                            if (i.ErrorMessage == "Error: invoice is already paid")
                            {
                                // This was a duplicate payment - funds were not sent and this payment hash should only have one paid version.
                                i.IsIgnored = true;
                                Services.MailingService.SendErrorNotification(
                                            title: "Tx marked as ignored",
                                            message: "tx.id: " + Convert.ToString(i.Id)
                                            + " Reason 5");
                            }
                            else if (i.ErrorMessage == "Error: amount must be specified when paying a zero amount invoice" ||
                                     i.ErrorMessage == "Error: payment attempt not completed before timeout")
                            {
                                i.IsIgnored = true;

                                if (i.User.Funds.LimboBalance - amount < 0)
                                {
                                    if (i.User.Funds.LimboBalance < 0) // shouldn't happen!
                                    {
                                        i.User.Funds.LimboBalance = 0;
                                    }
                                    i.User.Funds.Balance += i.User.Funds.LimboBalance;
                                    i.User.Funds.LimboBalance = 0;
                                }
                                else
                                {
                                    i.User.Funds.LimboBalance -= amount;
                                    i.User.Funds.Balance += amount;
                                    if (i.User.Funds.LimboBalance < 0) // shouldn't happen!
                                    {
                                        i.User.Funds.LimboBalance = 0;
                                    }
                                }
                                i.IsLimbo = false;
                                // TODO: send user email notification update of result.
                                Services.MailingService.SendErrorNotification(
                                        title: "User withdraw limbo expired (not settled - limbo returned)",
                                        message: "Withdraw Invoice expired (payment not found). Funds released to user."
                                            + "\r\n invoice: " + i.PaymentRequest
                                            + "\r\n user: " + i.User.Name + "(" + i.User.AppId + ")"
                                            + "\r\n amount: " + Convert.ToString(i.Amount)
                                            + "\r\n error: " + (i.ErrorMessage ?? "null"));
                            }
                            else
                            {
                                var inv = lndClient.DecodePayment(i.PaymentRequest);
                                var t1 = i.TimestampCreated.Value;
                                var tNow = DateTime.UtcNow;// DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                var tExpire = t1.AddSeconds(Convert.ToInt64(inv.expiry) + 10000); //Add a buffer time
                                if (tNow > tExpire)
                                {
                                    // Expired - let's stop checking this invoice
                                    i.IsIgnored = true;
                                    // The payment can't go through any longer.
                                    if (i.User.Funds.LimboBalance - amount < 0)
                                    {
                                        //shouldn't happen!
                                        if (i.User.Funds.LimboBalance < 0)
                                        {
                                            i.User.Funds.LimboBalance = 0;
                                        }
                                        i.User.Funds.Balance += i.User.Funds.LimboBalance;
                                        i.User.Funds.LimboBalance = 0;
                                    }
                                    else
                                    {
                                        i.User.Funds.LimboBalance -= amount;
                                        i.User.Funds.Balance += amount;
                                    }
                                    i.IsLimbo = false;
                                    // TODO: send user email notification update of result.
                                    Services.MailingService.SendErrorNotification(
                                            title: "User withdraw limbo expired (not settled - limbo returned)",
                                            message: "Withdraw Invoice expired (payment not found). Funds released to user."
                                                + "\r\n invoice: " + i.PaymentRequest
                                                + "\r\n user: " + i.User.Name + "(" + i.User.AppId + ")"
                                                + "\r\n amount: " + Convert.ToString(i.Amount)
                                                + "\r\n error: " + (i.ErrorMessage ?? "null"));
                                }
                                else
                                {
                                    ; // keep waiting
                                }
                            }
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        private static LndRpcClient GetLNDClient(Models.Database.ZapReadGlobals website)
        {
            return new LndRpcClient(
                                host: website.LnMainnetHost,
                                macaroonAdmin: website.LnMainnetMacaroonAdmin,
                                macaroonRead: website.LnMainnetMacaroonRead,
                                macaroonInvoice: website.LnMainnetMacaroonInvoice);
        }
    }
}