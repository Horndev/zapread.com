using LightningLib.lndrpc;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zapread.com.Models;
using zapread.com.Database;

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
                        Destination = "steven.horn.mail@gmail.com",
                        Body = " Withdraw from user which doesn't exist.",
                        Email = "",
                        Name = "zapread.com Monitoring",
                        Subject = "User withdraw error",
                    });

                    // Don't reveal information that user doesn't exist
                    return new { Result = "Error processing request." };
                }

                if (user.Funds.Balance < Convert.ToDouble(decoded.num_satoshis))
                {
                    return new { Result = "Insufficient Funds. You have " + user.Funds.Balance.ToString("0.") + ", invoice is for " + decoded.num_satoshis + "." };
                }

                //insert transaction as pending
                LNTransaction t = new LNTransaction()
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
                db.SaveChanges();

                SendPaymentResponse paymentresult;

                //all (should be) ok - make the payment
                if (WithdrawRequests.TryAdd(request, DateTime.UtcNow))
                {
                    paymentresult = lndClient.PayInvoice(request);
                }
                else
                {
                    //double request!
                    return new { Result = "Please click only once.  Payment already in processing." };
                }

                WithdrawRequests.TryRemove(request, out DateTime reqInitTime);

                if (paymentresult == null)
                {
                    return new { Result = "Error executing payment." };
                }

                if (paymentresult.error != null && paymentresult.error != "")
                {
                    return new { Result = "Error: " + paymentresult.error };
                }
                
                if (paymentresult.payment_error != null)
                {
                    return new { Result = "Error: " + paymentresult.payment_error };
                }

                // should this be done here? Is there an async/sync check that payment was sent successfully?
                user.Funds.Balance -= Convert.ToDouble(decoded.num_satoshis);
                db.SaveChanges();

                //update transaction status
                t.IsSettled = true;
                t.FeePaid_Satoshi = (paymentresult.payment_route.total_fees == null ? 0 : Convert.ToInt64(paymentresult.payment_route.total_fees));

                //db.LightningTransactions.Add(t);
                db.SaveChanges();
                return new { Result = "success", Fees = 0 };
            }
        }
    }
}
