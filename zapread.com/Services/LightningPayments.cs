using LightningLib.lndrpc;
using LightningLib.lndrpc.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using zapread.com.Database;
using zapread.com.Models;

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
            // Check if payment request is ok
            // Check if already paid
            var decoded = lndClient.DecodePayment(request);

            if (decoded == null)
            {
                return new { Result = "Error decoding invoice." };
            }

            if (decoded.destination == null)
            {
                return new { Result = "Error decoding invoice." };
            }

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Include(usr => usr.Funds)
                    .FirstOrDefault(u => u.AppId == userId);

                if (user == null)
                {
                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Withdraw from user which doesn't exist.",
                        Email = "",
                        Name = "zapread.com Monitoring",
                        Subject = "User withdraw error 2",
                    });

                    // Don't reveal information that user doesn't exist
                    return new { Result = "Error processing request." };
                }

                // Check all pending withdraw invoices and update balances before proceeding.


                // Check if user has sufficient balance
                if (user.Funds.Balance < Convert.ToDouble(decoded.num_satoshis))
                {
                    return new { Result = "Insufficient Funds. You have " + user.Funds.Balance.ToString("0.") + ", invoice is for " + decoded.num_satoshis + "." };
                }

                SendPaymentResponse paymentresult = null;
                LNTransaction t = null;

                //all (should be) ok - make the payment
                if (WithdrawRequests.TryAdd(request, DateTime.UtcNow))
                {
                    // Mark funds for withdraw as "in limbo"
                    user.Funds.LimboBalance += Convert.ToDouble(decoded.num_satoshis);
                    user.Funds.Balance -= Convert.ToDouble(decoded.num_satoshis);

                    //insert transaction as pending
                    t = new LNTransaction()
                    {
                        IsSettled = false,
                        Memo = decoded.description ?? "Withdraw",
                        HashStr = decoded.payment_hash,
                        Amount = Convert.ToInt64(decoded.num_satoshis),
                        IsDeposit = false,
                        TimestampSettled = DateTime.UtcNow,
                        TimestampCreated = DateTime.UtcNow, //can't know
                        PaymentRequest = request,
                        FeePaid_Satoshi = 0,
                        NodePubKey = decoded.destination,
                        User = user,
                    };
                    db.LightningTransactions.Add(t);
                    db.SaveChanges();  // Synchronous to ensure balance is locked.

                    // Register polling listener (TODO)

                    // Execute payment
                    try
                    {
                        paymentresult = lndClient.PayInvoice(request);
                    }
                    catch (RestException e)
                    {
                        // A RestException happens when there was an error with the LN node.
                        MailingService.Send(new UserEmailModel()
                        {
                            Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                            Body = " Withdraw error: PayInvoice threw an exception. "
                                + "\r\n message: " + e.Message
                                + "\r\n hash: " + t.HashStr
                                + "\r\n Content: " + e.Content
                                + "\r\n HTTPStatus: " + e.StatusDescription + "\r\n invoice: " + request
                                + "\r\n user: " + userId + "\r\n username: " + user.Name,
                            Email = "",
                            Name = "zapread.com Exception",
                            Subject = "User withdraw error 4",
                        });

                        t.ErrorMessage = "Error executing payment.";
                        db.SaveChanges();
                        return new { Result = "Error executing payment." };
                    }
                }
                else
                {
                    //double request!
                    return new { Result = "Please click only once.  Payment already in processing." };
                }

                // If we are at this point, we are now checking the status of the payment.
                if (paymentresult == null)
                {
                    // Something went wrong.  Check if the payment went through
                    var payments = lndClient.GetPayments();

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

                    if (pmt != null)
                    {
                        // Looks like the payment did go through.
                        // the payment went through process withdrawal
                        paymentresult = new SendPaymentResponse()
                        {
                            payment_route = new PaymentRoute()
                            {
                                total_fees = "0",
                            }
                        };
                    }
                    else
                    {
                        // Not recovered - it will be cued for checkup later.  This could be caused by LND being "laggy"
                        // Reserve the user funds to prevent another withdraw
                        //user.Funds.LimboBalance += Convert.ToDouble(decoded.num_satoshis);
                        //user.Funds.Balance -= Convert.ToDouble(decoded.num_satoshis);

                        t.ErrorMessage = "Error validating payment.";
                        db.SaveChanges();
                        return new { Result = "Error validating payment.  Funds will be held until confirmed or invoice expires." };
                    }
                }

                if (paymentresult.error != null && paymentresult.error != "")
                {
                    t.ErrorMessage = "Error: " + paymentresult.error;
                    db.SaveChanges();
                    return new { Result = "Error: " + paymentresult.error };
                }

                if (paymentresult.payment_error != null)
                {
                    t.ErrorMessage = "Error: " + paymentresult.payment_error;
                    db.SaveChanges();
                    return new { Result = "Error: " + paymentresult.payment_error };
                }

                // Unblock this request since it was successful
                WithdrawRequests.TryRemove(request, out DateTime reqInitTime);

                // should this be done here? Is there an async/sync check that payment was sent successfully?
                
                // We have already subtracted the balance from the user account, since the payment was
                // successful, we leave it subtracted from the account and we remove the balance from limbo.
                
                //user.Funds.Balance -= Convert.ToDouble(decoded.num_satoshis);
                user.Funds.LimboBalance -= Convert.ToDouble(decoded.num_satoshis);

                //update transaction status in DB
                t.IsSettled = true;
                try
                {
                    t.FeePaid_Satoshi = (paymentresult.payment_route.total_fees == null ? 0 : Convert.ToInt64(paymentresult.payment_route.total_fees));
                }
                catch
                {
                    t.FeePaid_Satoshi = 0;
                }

                db.SaveChanges();
                return new { Result = "success", Fees = 0 };
            }
        }
    }
}
