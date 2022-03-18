using LightningLib.lndrpc;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using zapread.com.Database;

namespace zapread.com.Services
{
    /// <summary>
    /// This class manages polling checks for LN payments and executes a callback on conditions
    /// </summary>
    public class PaymentPoller
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "<Pending>")]
        public static void Subscribe()
        {
            LndRpcClient lndClient;
            using (var db = new ZapContext())
            {
                lndClient = getLndClient(db);

                // These are non-settled withdraws in the database
                var unpaidWithdraws = db.LightningTransactions
                    .Where(t => t.IsSettled == false)   // Not settled
                    .Where(t => t.IsDeposit == false)   // Withdraw
                    .Where(t => t.IsIgnored == false)   // Still valid
                    .Include(t => t.User)
                    .Include(t => t.User.Funds)
                    .ToList();

                string h = "737ba901f37f523bc4ceeb19bcb3a68c029f32725b3fab2f557d915daab478d3";
                // Very inefficient (LND issue)
                var payments = lndClient.GetPayments();

                var pmti = payments.payments.Where(p => p.payment_hash == h).FirstOrDefault();

                var wi = unpaidWithdraws.Where(w => w.HashStr == h).FirstOrDefault();

                foreach (var i in unpaidWithdraws)
                {
                    //if (i.ErrorMessage != "") // Check if error
                    //{
                    //    ; // Error - this will never be paid
                    //    i.IsIgnored = true;
                    //}
                    if (i.HashStr != null)
                    {
                        var pmt = payments.payments.Where(p => p.payment_hash == i.HashStr).FirstOrDefault();

                        if (pmt != null)
                        {
                            ; // Settled in node, not in zapread DB
                            // Do stuff...
                            if (i.ErrorMessage != null && i.ErrorMessage == "Error: invoice is already paid")
                            {
                                i.IsIgnored = true;
                            }
                            else if (i.ErrorMessage != null && i.ErrorMessage.StartsWith("Error: FinalIncorrectCltvExpiry(expiry="))
                            {
                                i.IsIgnored = true;
                            }
                            else if (i.ErrorMessage != null && i.ErrorMessage.StartsWith("Error: unable to route payment to destination: TemporaryChannelFailure"))
                            {
                                // could be a repeat attempt
                                i.IsIgnored = true;
                            }
                            else if (i.ErrorMessage != null && i.ErrorMessage == "Error executing payment.")
                            {
                                // issue - why was this paid?
                                i.IsIgnored = true;
                            }
                            else if (i.ErrorMessage != null && i.ErrorMessage == "Error: FinalExpiryTooSoon")
                            {
                                // issue - why was this paid?
                                i.IsIgnored = true;
                            }
                            else if (i.ErrorMessage != null && i.ErrorMessage == "Error: payment is in transition")
                            {
                                // probably ok
                                i.IsIgnored = true;
                            }
                            else
                            {
                                ; // Do something
                                i.IsSettled = true;
                                i.FeePaid_Satoshi = pmt.fee != null ? Convert.ToInt64(pmt.fee, CultureInfo.InvariantCulture) : 0;

                                // Should probably update user balance too

                            }
                        }
                        else if (i.TimestampCreated.HasValue)
                        {
                            // Check if expired
                            if (DateTime.UtcNow - i.TimestampCreated > TimeSpan.FromDays(1))
                            {
                                ; // forget about it
                                i.IsIgnored = true;
                            }
                        }
                    }
                    else
                    {
                        ; // Can't look up without HashStr
                    }
                }

                //db.SaveChanges();
            }
        }

        private static LndRpcClient getLndClient(ZapContext db)
        {
            LndRpcClient lndClient;
            var g = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                .AsNoTracking()
                .FirstOrDefault();

            lndClient = new LndRpcClient(
                host: g.LnMainnetHost,
                macaroonAdmin: g.LnMainnetMacaroonAdmin,
                macaroonRead: g.LnMainnetMacaroonRead,
                macaroonInvoice: g.LnMainnetMacaroonInvoice);
            return lndClient;
        }
    }
}