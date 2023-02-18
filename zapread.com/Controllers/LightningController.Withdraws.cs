using LightningLib.lndrpc;
using LightningLib.lndrpc.Exceptions;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Lightning Controller methods for handling withdraws
    /// </summary>
	public partial class LightningController : Controller
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "request">Lightning invoice</param>
        /// <returns></returns>
        [HttpPost, ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> ValidatePaymentRequest(string request)
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return Json(new
                {
                    success = false,
                    message = "User not authorized."
                });
            }

            using (var db = new ZapContext())
            {
                var hasWithdrawLock = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Where(u => u.Funds.Locks.Any(f => f.WithdrawLocked))
                    .AnyAsync();

                if (hasWithdrawLock)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Withdraws for this account are locked."
                    });
                }

                var invoice = request.SanitizeXSS();
                LNTransaction t;
                try
                {
                    // Check if the request has previously been submitted
                    t = await db.LightningTransactions
                        .Where(tx => tx.PaymentRequest == invoice)
                        .SingleOrDefaultAsync().ConfigureAwait(true);
                }
                catch (InvalidOperationException)
                {
                    // source has more than one element.
                    return Json(new
                    {
                        success = false,
                        message = "Duplicate invoice - please use a new invoice."
                    });
                }

                if (t == null)
                {
                    // first time
                    // Get interface to LND
                    LndRpcClient lndClient = GetLndClient();
                    // Decode invoice
                    var decoded = lndClient.DecodePayment(invoice);
                    if (decoded != null)
                    {
                        double amount = Convert.ToDouble(decoded.num_satoshis, CultureInfo.InvariantCulture);
                        if (amount < 1)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Zero- or any-value invoices are not supported at this time"
                            });
                        }

                        if (amount > 50000)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Withdraws temporarily limited to 50000 Satoshi"
                            });
                        }

                        // Check user balance
                        var userFunds = await db.Users.Where(u => u.AppId == userAppId).Select(u => new
                        {
                            u.Funds.Balance
                        }).FirstOrDefaultAsync().ConfigureAwait(true);
                        if (userFunds == null)
                        {
                            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return Json(new
                            {
                                success = false,
                                message = "User not found in database."
                            });
                        }

                        if (userFunds.Balance < amount)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Insufficient Funds. You have " + userFunds.Balance.ToString("0.", CultureInfo.InvariantCulture) + ", invoice is for " + decoded.num_satoshis + "."
                            });
                        }

                        // This is less than ideal for time checks...
                        // Check how much user withdrew previous 24 hours
                        var DayAgo = DateTime.UtcNow - TimeSpan.FromDays(1);
                        var txs = await db.LightningTransactions.Where(tx => tx.User.AppId == userAppId).Where(tx => tx.TimestampCreated != null && tx.TimestampCreated > DayAgo).Where(tx => !tx.IsDeposit).Select(tx => tx.Amount).ToListAsync().ConfigureAwait(true);
                        var withdrawn24h = txs.Sum();
                        if (withdrawn24h > 100000)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Withdraws limited to 100,000 Satoshi within a 24 hour limit."
                            });
                        }

                        var HourAgo = DateTime.UtcNow - TimeSpan.FromHours(1);
                        var txs1h = await db.LightningTransactions.Where(tx => tx.User.AppId == userAppId).Where(tx => tx.TimestampCreated != null && tx.TimestampCreated > HourAgo).Where(tx => !tx.IsDeposit).Select(tx => tx.Amount).ToListAsync().ConfigureAwait(true);
                        var withdrawn1h = txs1h.Sum();
                        if (withdrawn1h > 50000)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Withdraws limited to 50,000 Satoshi within a 1 hour limit."
                            });
                        }

                        // Save the invoice to database
                        var user = await db.Users.Where(u => u.AppId == userAppId).FirstAsync().ConfigureAwait(true);
                        //create a new transaction record in database
                        t = new LNTransaction()
                        {
                            IsSettled = false,
                            Memo = (decoded.description ?? "Withdraw").SanitizeXSS(),
                            HashStr = decoded.payment_hash,
                            PaymentHash = decoded.payment_hash,
                            Amount = Convert.ToInt64(decoded.num_satoshis, CultureInfo.InvariantCulture),
                            IsDeposit = false,
                            TimestampSettled = null,
                            TimestampCreated = DateTime.UtcNow, //can't know
                            PaymentRequest = invoice,
                            FeePaid_Satoshi = 0,
                            NodePubKey = decoded.destination,
                            User = user,
                            WithdrawId = Guid.NewGuid(),
                        };
                        db.LightningTransactions.Add(t);
                        await db.SaveChangesAsync().ConfigureAwait(true);

                        return Json(new
                        {
                            success = true,
                            withdrawId = t.WithdrawId,
                            decoded.num_satoshis,
                            decoded.destination,
                        });
                    }
                }
                else
                {
                    // re-submitted - don't create new DB entry
                    // Safety checks
                    if (t.IsSettled || t.IsIgnored || t.IsLimbo || t.IsDeposit || t.IsError)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new
                        {
                            success = false,
                            message = "Invalid withdraw request."
                        });
                    }

                    // Check balance now
                    var userFunds = await db.Users.Where(u => u.AppId == userAppId).Select(u => new
                        {
                            u.Funds.Balance
                        })
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    if (userFunds == null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return Json(new
                        {
                            success = false,
                            message = "User not found in database."
                        });
                    }

                    double amount = Convert.ToDouble(t.Amount, CultureInfo.InvariantCulture);
                    if (userFunds.Balance < amount)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Insufficient Funds. You have " + userFunds.Balance.ToString("0.", CultureInfo.InvariantCulture) + ", invoice is for " + t.Amount + "."
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        withdrawId = t.WithdrawId, //t.Id,
                        num_satoshis = t.Amount,
                        destination = t.NodePubKey,
                    });
                }
            }

            return Json(new
            {
                success = false,
                message = "Error decoding invoice."
            });
        }

        /// <summary>
        /// Pay a lightning invoice.
        /// </summary>
        /// <param name = "withdrawId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> SubmitPaymentRequest(string withdrawId)
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new
                {
                    success = false,
                    message = "User not authorized."
                });
            }

            using (var db = new ZapContext())
            {
                var wguid = new Guid(withdrawId);
                var lntx = db.LightningTransactions.Where(tx => tx.WithdrawId == wguid).Where(tx => tx.User.AppId == userAppId).AsNoTracking().FirstOrDefault();
                if (lntx == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new
                    {
                        success = false,
                        message = "Invalid withdraw request."
                    });
                }

                if (lntx.IsSettled || lntx.IsIgnored || lntx.IsLimbo || lntx.IsDeposit || lntx.IsError)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new
                    {
                        success = false,
                        message = "Invalid withdraw request."
                    });
                }

                // Verify lntx invoice is unique.  This is a layer to protect against spam attacks
                var numTxsWithSamePayreq = db.LightningTransactions.Where(tx => tx.PaymentRequest == lntx.PaymentRequest).Count();
                if (numTxsWithSamePayreq > 1)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new
                    {
                        success = false,
                        message = "Invalid withdraw request."
                    });
                }

                // Get interface to LND
                //LndRpcClient lndClient = GetLndClient();

                // This is used for DoS or other attack detection
                string ip = GetClientIpAddress(Request);

                // Check when user has made last LN transaction - if too soon, reject
                {
                    var lasttx = db.LightningTransactions.Where(tx => tx.User.AppId == userAppId) // This user
                        .Where(tx => tx.Id != lntx.Id) // Not the one being processed now
                        .OrderByDescending(tx => tx.TimestampCreated) // Most recent
                        .AsNoTracking().FirstOrDefault();

                    if (lasttx != null && (DateTime.UtcNow - lasttx.TimestampCreated < TimeSpan.FromMinutes(5)))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Please wait 5 minutes between Lightning transaction requests."
                        });
                    }
                }

                // check user funds.  If not sufficient, reject
                {
                    var userFunds = db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(usr => usr.Funds)
                        .FirstOrDefault();

                    if (userFunds == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Error checking user balance for withdraw."
                        });
                    }

                    if (userFunds.IsWithdrawLocked)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "User withdraw is locked.  Please contact an administrator."
                        });
                    }

                    // Check if user has sufficient balance
                    if (userFunds.Balance < Convert.ToDouble(lntx.Amount, CultureInfo.InvariantCulture))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Insufficient Funds. You have " 
                                + userFunds.Balance.ToString("0.", CultureInfo.CurrentCulture) 
                                + ", invoice is for " 
                                + lntx.Amount.ToString(CultureInfo.CurrentCulture) + "."
                        });
                    }
                }

                // Submit Payment Request
                // asynchronous
                await PaymentsService.EnqueueWithdrawAsync(lntx.WithdrawId.ToString());

                // synchronous (depricated)
                //var paymentResult = PaymentsService.TryWithdrawal(lntx, userAppId, ip, lndClient);

                //return Json(paymentResult);
                return Json(new
                {
                    success = true,
                    message = "Withdraw is processing."
                });
            }
        }

    }
}