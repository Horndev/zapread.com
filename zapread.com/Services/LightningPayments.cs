using Hangfire;
using LightningLib.lndrpc;
using LightningLib.lndrpc.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
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
        /// <param name="userId"></param>
        /// <param name="lndClient"></param>
        /// <returns></returns>
        public object TryWithdrawal(string request, string userId, string ip, LndRpcClient lndClient)
        {
            if (lndClient == null)
            {
                return HandleLndClientIsNull();
            }
            
            // Check if already paid // Check if payment request is ok
            var decoded = lndClient.DecodePayment(request);

            if (decoded == null || decoded.destination == null)
            {
                return new { success = false, Result = "Error decoding invoice." };
            }

            using (var db = new ZapContext())
            {
                User user = GetUserFromDB(userId, db);

                if (user == null)
                {
                    return HandleUserIsNull();
                }

                // Check all pending withdraw invoices and update balances before proceeding.

                // Check if user has sufficient balance
                if (user.Funds.Balance < Convert.ToDouble(decoded.num_satoshis, CultureInfo.InvariantCulture))
                {
                    return new { success = false, Result = "Insufficient Funds. You have " + user.Funds.Balance.ToString("0.", CultureInfo.CurrentCulture) + ", invoice is for " + decoded.num_satoshis + "." };
                }

                SendPaymentResponse paymentresult = null;
                LNTransaction t = null;
                string responseStr = "";

                //all (should be) ok - make the payment
                if (WithdrawRequests.TryAdd(request, DateTime.UtcNow))  // This is to prevent flood attacks
                {
                    // Mark funds for withdraw as "in limbo" - will be resolved if verified as paid.
                    user.Funds.LimboBalance += Convert.ToDouble(decoded.num_satoshis, CultureInfo.InvariantCulture);
                    user.Funds.Balance -= Convert.ToDouble(decoded.num_satoshis, CultureInfo.InvariantCulture);

                    //insert transaction record in db as pending
                    t = new LNTransaction()
                    {
                        IsSettled = false,
                        Memo = (decoded.description ?? "Withdraw").SanitizeXSS(),
                        HashStr = decoded.payment_hash,
                        Amount = Convert.ToInt64(decoded.num_satoshis, CultureInfo.InvariantCulture),
                        IsDeposit = false,
                        TimestampSettled = DateTime.UtcNow,
                        TimestampCreated = DateTime.UtcNow, //can't know
                        PaymentRequest = request,
                        FeePaid_Satoshi = 0,
                        NodePubKey = decoded.destination,
                        User = user,
                        IsLimbo = true,
                    };
                    db.LightningTransactions.Add(t);
                    db.SaveChanges();  // Synchronous to ensure balance is locked.

                    // Register polling listener (TODO)

                    // Execute payment
                    try
                    {
                        paymentresult = lndClient.PayInvoice(request, out responseStr);
                    }
                    catch (RestException e)
                    {
                        // A RestException happens when there was an error with the LN node.
                        // At this point, the funds will remain in limbo until it is verified as paid by the 
                        //   periodic LNTransactionMonitor service.
                        return HandleClientRestException(request, userId, db, user, t, responseStr, e);
                    }
                }
                else
                {
                    //double request!
                    return new { success = false, Result = "Please click only once.  Payment already in processing." };
                }

                // If we are at this point, we are now checking the status of the payment.
                if (paymentresult == null)
                {
                    // Something went wrong.  Check if the payment went through
                    var payments = lndClient.GetPayments(include_incomplete: true);

                    var pmt = payments.payments.Where(p => p.payment_hash == t.HashStr).FirstOrDefault();

                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Withdraw error: PayInvoice returned null result. \r\n hash: " + t.HashStr
                            + "\r\n recovered by getpayments: " + (pmt != null ? "true" : "false") + "\r\n invoice: "
                            + request + "\r\n user: " + userId,
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
                               + "\r\n user: " + userId + "\r\n username: " + user.Name
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

                        t.ErrorMessage = "Error validating payment.";
                        db.SaveChanges();
                        return new { success = false, Result = "Error validating payment.  Funds will be held until confirmed or invoice expires." };
                    }
                }

                // This shouldn't ever be hit - this response is obsolete.  
                // TODO watch for this error, and if not found by June 2020 - delete this code
                if (paymentresult.error != null && paymentresult.error != "")
                {
                    t.ErrorMessage = "Error: " + paymentresult.error;
                    t.IsError = true;
                    db.SaveChanges();

                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Withdraw error: payment error "
                               + "\r\n user: " + userId + "\r\n username: " + user.Name
                               + "\r\n <br><br> response: " + responseStr,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User withdraw error 7",
                    });
                    return new { success = false, Result = "Error: " + paymentresult.error };
                }

                // The LND node returned an error
                if (!String.IsNullOrEmpty(paymentresult.payment_error))//paymentresult.payment_error != null && paymentresult.payment_error != "")
                {
                    // Funds will remain in Limbo until failure verified by LNTransactionMonitor
                    // TODO: verify trust in this method - funds could be returned to user here.
                    return HandleLNPaymentError(userId, db, user, paymentresult, t, responseStr);
                }

                // Unblock this request since it was successful
                WithdrawRequests.TryRemove(request, out DateTime reqInitTime);

                // should this be done here? Is there an async/sync check that payment was sent successfully?

                // We have already subtracted the balance from the user account, since the payment was
                // successful, we leave it subtracted from the account and we remove the balance from limbo.

                //user.Funds.Balance -= Convert.ToDouble(decoded.num_satoshis);
                user.Funds.LimboBalance -= Convert.ToDouble(decoded.num_satoshis, CultureInfo.InvariantCulture);

                //update transaction status in DB
                t.IsSettled = true;
                t.IsLimbo = false;

                try
                {
                    t.FeePaid_Satoshi = (paymentresult.payment_route.total_fees == null ? 0 : Convert.ToInt64(paymentresult.payment_route.total_fees, CultureInfo.InvariantCulture));
                }
                catch
                {
                    t.FeePaid_Satoshi = 0;
                }

                db.SaveChanges();
                return new { success = true, Result = "success", Fees = 0, userBalance = user.Funds.Balance };
            }
        }

        private static object HandleLNPaymentError(string userId, ZapContext db, User user, SendPaymentResponse paymentresult, LNTransaction t, string responseStr)
        {
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
                       + "\r\n user: " + userId + "\r\n username: " + user.Name
                       + "\r\n <br><br> response: " + responseStr,
                    Email = "",
                    Name = "zapread.com Exception",
                    Subject = "LightningPayments error - User withdraw error 5",
                }, "Notify"));

            return new { success = false, Result = "Error: " + paymentresult.payment_error };
        }

        private static object HandleClientRestException(string request, string userId, ZapContext db, User user, LNTransaction t, string responseStr, RestException e)
        {
            BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                new UserEmailModel()
                {
                    Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                    Body = "Withdraw error: PayInvoice threw an exception. "
                        + "\r\n message: " + e.Message
                        + "\r\n hash: " + t.HashStr
                        + "\r\n Content: " + e.Content
                        + "\r\n HTTPStatus: " + e.StatusDescription + "\r\n invoice: " + request
                        + "\r\n user: " + userId + "\r\n username: " + user.Name
                        + "\r\n <br><br> response: " + responseStr,
                    Email = "",
                    Name = "zapread.com Exception",
                    Subject = "LightningPayments error - User withdraw error 4",
                }, "Notify"));

            t.ErrorMessage = "Error executing payment.";
            db.SaveChanges();
            return new { success = false, Result = "Error executing payment." };
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
                }, "Notify"));

            // Don't reveal information that user doesn't exist
            return new { success = false, Result = "Error processing request." };
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
                }, "Notify"));

            return new { success = false, Result = "Lightning Node error." };
        }

        private static User GetUserFromDB(string userId, ZapContext db)
        {
            return db.Users
                                .Include(usr => usr.Funds)
                                .FirstOrDefault(u => u.AppId == userId);
        }
    }
}
