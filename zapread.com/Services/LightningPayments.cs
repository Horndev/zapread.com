using Hangfire;
using LightningLib.lndrpc;
using LightningLib.lndrpc.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Database;

namespace zapread.com.Services
{

    public class LightningPayments : ILightningPayments
    {
        /// <summary>
        /// Tracks the time each node has last withdrawn.
        /// </summary>
        private static ConcurrentDictionary<string, DateTime> nodeWithdrawAttemptTimes = new ConcurrentDictionary<string, DateTime>();
        private static ConcurrentDictionary<string, DateTime> userWithdrawAttemptTimes = new ConcurrentDictionary<string, DateTime>();

        // Badness value for node (for banning)
        private static ConcurrentDictionary<string, int> nodeBadness = new ConcurrentDictionary<string, int>();

        private static DateTime timeLastAnonWithdraw = DateTime.Now - TimeSpan.FromHours(1);

        //Used for rate limiting double withdraws
        static ConcurrentDictionary<string, DateTime> WithdrawRequests = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// Ensure only one withdraw at a time
        /// </summary>
        private Object withdrawLock = new Object();

        public LightningPayments()
        {
            // Empty constructor
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsNodeBanned(string node, out string message)
        {
            // TODO: This should be in a database with admin view
            Dictionary<string, string> bannedNodes = new Dictionary<string, string>()
                {
                    { "023216c5b9a54b6179645c76b279ae267f3c6b2379b9f305d57c75065006a8e5bd", "Abusive use" },
                };

            if (bannedNodes.Keys.Contains(node))
            {
                message = bannedNodes[node];
                return true;
            }
            message = "";
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void RecordNodeWithdraw(string node)
        {
            nodeWithdrawAttemptTimes.AddOrUpdate(
                node,                               // node of interest
                DateTime.UtcNow,                    // Value to insert if new node
                (key, oldval) => DateTime.UtcNow);  // Update function if existing node
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userAppId"></param>
        /// <param name="lndClient"></param>
        /// <returns></returns>
        public object TryWithdrawal(Models.LNTransaction request, string userAppId, string ip, LndRpcClient lndClient)
        {
            if (request == null)
            {
                return new { success = false, message = "Internal error." };
            }

            if (lndClient == null)
            {
                return HandleLndClientIsNull();
            }

            long FeePaid_Satoshi;   // This is used later if the invoice succeeds.
            User user = null;

            using (var db = new ZapContext())
            {
                // Check when user has made last LN transaction
                var lasttx = db.LightningTransactions
                    .Where(tx => tx.User.AppId == userAppId)            // This user
                    .Where(tx => tx.Id != request.Id)                   // Not the one being processed now
                    .OrderByDescending(tx => tx.TimestampCreated)       // Most recent
                    .FirstOrDefault();

                if (lasttx != null && (DateTime.UtcNow - lasttx.TimestampCreated < TimeSpan.FromMinutes(5)))
                {
                    return new { success = false, message = "Please wait 5 minutes between Lightning transaction requests." };
                }

                // Check if user has sufficient balance
                var userFunds = db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(usr => usr.Funds)
                    .FirstOrDefault();

                if (userFunds == null)
                {
                    return HandleUserIsNull();
                }

                if (userFunds.IsWithdrawLocked)
                {
                    return new { success = false, message = "User withdraw is locked.  Please contact an administrator." };
                }

                SendPaymentResponse paymentresult = null;
                string responseStr = "";

                //all (should be) ok - make the payment
                if (WithdrawRequests.TryAdd(request.PaymentRequest, DateTime.UtcNow))  // This is to prevent flood attacks
                {
                    // Check if user has sufficient balance
                    if (userFunds.Balance < Convert.ToDouble(request.Amount, CultureInfo.InvariantCulture))
                    {
                        return new
                        {
                            success = false,
                            message = "Insufficient Funds. You have "
                                + userFunds.Balance.ToString("0.", CultureInfo.CurrentCulture)
                                + ", invoice is for " + request.Amount.ToString(CultureInfo.CurrentCulture)
                                + "."
                        };
                    }

                    // Mark funds for withdraw as "in limbo" - will be resolved if verified as paid.
                    userFunds.LimboBalance += Convert.ToDouble(request.Amount, CultureInfo.InvariantCulture);
                    userFunds.Balance -= Convert.ToDouble(request.Amount, CultureInfo.InvariantCulture);

                    // Funds are checked for optimistic concurrency here.  If the Balance has been updated,
                    // we shouldn't proceed with the withdraw, so we will abort it.
                    try
                    {
                        db.SaveChanges();  // Synchronous to ensure balance is locked.
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // The balance has changed - don't do withdraw.

                        // This may trigger if the user also gets funds added - such as a tip.
                        // For now, we will fail the withdraw under any condition.
                        // In the future, we may consider ignoring changes increasing balance.

                        // Remove this request from the lock so the user can retry.
                        WithdrawRequests.TryRemove(request.PaymentRequest, out DateTime reqInitTimeReset);

                        return new { success = false, message = "Failed. User balances changed during withdraw." };
                    }

                    // Execute payment
                    try
                    {
                        paymentresult = lndClient.PayInvoice(request.PaymentRequest, out responseStr);
                    }
                    catch (RestException e)
                    {
                        user = db.Users
                            .Where(u => u.AppId == userAppId)
                            .FirstOrDefault();

                        // A RestException happens when there was an error with the LN node.
                        // At this point, the funds will remain in limbo until it is verified as paid by the 
                        //   periodic LNTransactionMonitor service.
                        return HandleClientRestException(userAppId, request.Id, responseStr, e);
                    }
                }
                else
                {
                    //double request!
                    return new { success = false, message = "Please click only once.  Payment already in processing." };
                }

                // If we are at this point, we are now checking the status of the payment.
                if (paymentresult == null)
                {
                    // Something went wrong.  Check if the payment went through
                    var payments = lndClient.GetPayments(include_incomplete: true);

                    var pmt = payments.payments
                        .Where(p => p.payment_hash == request.HashStr)
                        .FirstOrDefault();

                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Withdraw error: PayInvoice returned null result. \r\n hash: " + request.HashStr
                            + "\r\n recovered by getpayments: " + (pmt != null ? "true" : "false") + "\r\n invoice: "
                            + request + "\r\n user: " + userAppId,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User withdraw error 3",
                    });

                    if (pmt != null && pmt.status == "SUCCEEDED")
                    {
                        // Looks like the payment may have gone through.
                        // the payment went through process withdrawal
                        paymentresult = new SendPaymentResponse()
                        {
                            payment_route = new PaymentRoute()
                            {
                                total_fees = "0",
                            }
                        };

                        MailingService.Send(new UserEmailModel()
                        {
                            Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                            Body = " Withdraw error: payment error "
                               + "\r\n user: " + userAppId + "\r\n username: " + user.Name
                               + "\r\n <br><br> response: " + responseStr,
                            Email = "",
                            Name = "zapread.com Exception",
                            Subject = "User withdraw possible error 6",
                        });
                    }
                    else
                    {
                        // Not recovered - it will be cued for checkup later.  This could be caused by LND being "laggy"
                        // Reserve the user funds to prevent another withdraw

                        //user.Funds.LimboBalance += Convert.ToDouble(decoded.num_satoshis);
                        //user.Funds.Balance -= Convert.ToDouble(decoded.num_satoshis);

                        return HandlePaymentRecoveryFailed(userAppId, request.Id);
                    }
                }

                // This shouldn't ever be hit - this response is obsolete.  
                // TODO watch for this error, and if not found by June 2020 - delete this code
                if (paymentresult.error != null && paymentresult.error != "")
                {
                    return HandleLegacyPaymentRecoveryFailed(userAppId, request.Id, paymentresult, responseStr);
                }

                // The LND node returned an error
                if (!String.IsNullOrEmpty(paymentresult.payment_error))//paymentresult.payment_error != null && paymentresult.payment_error != "")
                {
                    // Funds will remain in Limbo until failure verified by LNTransactionMonitor
                    // TODO: verify trust in this method - funds could be returned to user here.
                    return HandleLNPaymentError(userAppId, request.Id, paymentresult, responseStr);
                }

                FeePaid_Satoshi = (paymentresult.payment_route.total_fees == null ? 0 : Convert.ToInt64(paymentresult.payment_route.total_fees, CultureInfo.InvariantCulture));

                // Unblock this request since it was successful
                WithdrawRequests.TryRemove(request.PaymentRequest, out DateTime reqInitTime);
            }

            // We're going to start a new context as we are updating the Limbo Balance
            using (var db = new ZapContext())
            {
                user = db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefault();

                // We have already subtracted the balance from the user account, since the payment was
                // successful, we leave it subtracted from the account and we remove the balance from limbo.
                bool saveFailed;
                int attempts = 0;

                // Get an update-able entity for the transaction from the DB
                var t = db.LightningTransactions
                    .Where(tx => tx.Id == request.Id)
                    .Where(tx => tx.User.AppId == userAppId)
                    .FirstOrDefault();

                do
                {
                    attempts++;

                    if (attempts > 50)
                    {
                        // We REALLY should never get to this point.  If we're here, there is some strange
                        // deadlock, or the user is being abusive and the funds will stay in Limbo.
                    }

                    saveFailed = false;

                    user.Funds.LimboBalance -= Convert.ToDouble(request.Amount, CultureInfo.InvariantCulture);
                    //update transaction status in DB
                    t.IsSettled = true;
                    t.IsLimbo = false;
                    try
                    {
                        t.FeePaid_Satoshi = FeePaid_Satoshi;// (paymentresult.payment_route.total_fees == null ? 0 : Convert.ToInt64(paymentresult.payment_route.total_fees, CultureInfo.InvariantCulture));
                    }
                    catch
                    {
                        t.FeePaid_Satoshi = 0;
                    }

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

                return new { success = true, message = "success", Fees = 0, userBalance = user.Funds.Balance };
            }
        }

        private static object HandleLegacyPaymentRecoveryFailed(string userAppId, int txid, SendPaymentResponse paymentresult, string responseStr)
        {
            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefault();

                var t = db.LightningTransactions
                    .Where(tx => tx.Id == txid)
                    .Where(tx => tx.User.AppId == userAppId)
                    .FirstOrDefault();

                t.ErrorMessage = "Error: " + paymentresult.error;
                t.IsError = true;
                db.SaveChanges();

                BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                    new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Withdraw error: payment error "
                           + "\r\n user: " + userAppId + "\r\n username: " + user.Name
                           + "\r\n <br><br> response: " + responseStr,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User withdraw error 7",
                    }, "Notify", true));

                return new { success = false, message = "Error: " + paymentresult.error };
            }
        }

        private static object HandlePaymentRecoveryFailed(string userAppId, int txid)
        {
            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefault();

                var t = db.LightningTransactions
                    .Where(tx => tx.Id == txid)
                    .Where(tx => tx.User.AppId == userAppId)
                    .FirstOrDefault();

                t.ErrorMessage = "Error validating payment.";
                db.SaveChanges();

                BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                    new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Withdraw error: unknown error "
                           + "\r\n user: " + userAppId + "\r\n username: " + user.Name
                           + "\r\n <br><br> txid: " + txid.ToString(CultureInfo.InvariantCulture),
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "LightningPayments error - User withdraw error X",
                    }, "Notify", true));

                return new { success = false, message = "Error validating payment.  Funds will be held until confirmed or invoice expires." };
            }
        }

        private static object HandleLNPaymentError(string userAppId, int txid, SendPaymentResponse paymentresult, string responseStr)
        {
            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefault();

                var t = db.LightningTransactions
                    .Where(tx => tx.Id == txid)
                    .Where(tx => tx.User.AppId == userAppId)
                    .FirstOrDefault();

                // Save to database
                t.ErrorMessage = "Error: " + paymentresult.payment_error;
                t.IsError = true;
                t.IsLimbo = false;
                db.SaveChanges();


                BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                    new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Withdraw error: payment error "
                           + "\r\n user: " + userAppId + "\r\n username: " + user.Name
                           + "\r\n <br><br> response: " + responseStr,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "LightningPayments error - User withdraw error 5",
                    }, "Notify", true));

                return new { success = false, message = "Error: " + paymentresult.payment_error };
            }
        }

        private static object HandleClientRestException(string userAppId, int txid, string responseStr, RestException e)
        {
            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefault();

                var t = db.LightningTransactions
                    .Where(tx => tx.Id == txid)
                    .Where(tx => tx.User.AppId == userAppId)
                    .FirstOrDefault();
                BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                new UserEmailModel()
                {
                    Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                    Body = "Withdraw error: PayInvoice threw an exception. "
                        + "\r\n message: " + e.Message
                        + "\r\n hash: " + t.HashStr
                        + "\r\n Content: " + e.Content
                        + "\r\n HTTPStatus: " + e.StatusDescription + "\r\n invoice: " + t.PaymentRequest
                        + "\r\n user: " + userAppId + "\r\n username: " + user.Name
                        + "\r\n <br><br> response: " + responseStr,
                    Email = "",
                    Name = "zapread.com Exception",
                    Subject = "LightningPayments error - User withdraw error 4",
                }, "Notify", true));

                t.ErrorMessage = "Error executing payment.";
                db.SaveChanges();
                return new { success = false, message = "Error executing payment." };
            }
        }

        private static object HandleUserIsNull()
        {
            BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                new UserEmailModel()
                {
                    Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                    Body = "Withdraw from user which doesn't exist.",
                    Email = "",
                    Name = "zapread.com Monitoring",
                    Subject = "LightningPayments error - user is null",
                }, "Notify", true));

            // Don't reveal information that user doesn't exist
            return new { success = false, message = "Error processing request." };
        }

        private static object HandleLndClientIsNull()
        {
            BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                new UserEmailModel()
                {
                    Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                    Body = "LND Client null error",
                    Email = "",
                    Name = "zapread.com Monitoring",
                    Subject = "LightningPayments error - lndClient is null",
                }, "Notify", true));

            return new { success = false, message = "Lightning Node error." };
        }
    }
}
