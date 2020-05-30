using LightningLib.lndrpc;
using LightningLib.lndrpc.Exceptions;
using Microsoft.AspNet.Identity;
using MvcSiteMapProvider.Globalization;
using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Database.Financial;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    [RoutePrefix("Lightning")]
    public class LightningController : Controller
    {
        /// <summary>
        /// This is the interface to a singleton payments service which is injected for IOC.
        /// </summary>
        public ILightningPayments PaymentsService { get; private set; }

        /// <summary>
        /// Constructor with dependency injection for IOC and controller singleton control.
        /// </summary>
        /// <param name="paymentsService"></param>
        public LightningController(ILightningPayments paymentsService)
        {
            this.PaymentsService = paymentsService;
        }

        private static ConcurrentDictionary<Guid, TransactionListener> lndTransactionListeners = new ConcurrentDictionary<Guid, TransactionListener>();

        // GET: Lightning
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CheckPayment(string invoice, bool isDeposit = true)
        {
            // STEP 1: check the database
            using (ZapContext db = new ZapContext())
            {
                var userId = User.Identity.GetUserId();

                var u = await db.Users
                    .Include(usr => usr.Funds)
                    .FirstOrDefaultAsync(usr => usr.AppId == userId);

                var p = await db.LightningTransactions
                    .FirstOrDefaultAsync(t => t.PaymentRequest == invoice);

                if (p == null)
                    return Json(new { success = false, message = "invoice is not known to this node" });

                if (isDeposit && !p.IsDeposit)
                    return Json(new { success = false, message = "invoice is not a deposit invoice" });
                if (p.IsSettled)
                {
                    return Json(new { success = true, result = true, invoice = invoice, balance = u != null ? u.Funds.Balance : 0, txid = p.Id });
                }

                if (isDeposit)
                {
                    Invoice inv = FetchInvoiceFromNode(invoice);
                    if (!inv.settled.HasValue || (inv.settled.HasValue && inv.settled.Value == false))
                    {
                        return Json(new { success = true, result = false });
                    }
                    if (inv.settled.HasValue && inv.settled.Value)
                    {
                        // TODO: check if invoice listeners are running - start if not running
                        // Use the standard receiving logic to send real time notification to clients
                        await NotifyClientsInvoicePaid(inv).ConfigureAwait(true);
                        return Json(new { success = true, result = true, invoice = invoice, balance = u != null ? u.Funds.Balance : 0, txid = p.Id });
                    }
                }
                return Json(new { success = true });
            }
        }

        private static Invoice FetchInvoiceFromNode(string invoice)
        {
            LndRpcClient lndClient = GetLndClient();
            LightningLib.DataEncoders.HexEncoder h = new LightningLib.DataEncoders.HexEncoder();

            // Decode the payment request
            var decoded = lndClient.DecodePayment(invoice);

            // Get the hash
            var hash = decoded.payment_hash;

            // GetInvoice expects the hash in base64 encoded format

            var hash_bytes = h.DecodeData(hash);
            var hash_b64 = Convert.ToBase64String(hash_bytes);
            var inv = lndClient.GetInvoice(hash_b64);
            return inv;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetInvoiceStatus/{request?}")]
        public ActionResult GetInvoiceStatus(string request)
        {
            using (ZapContext db = new ZapContext())
            {
                var tx = db.LightningTransactions.AsNoTracking().FirstOrDefault(t => t.PaymentRequest == request);
                return Json(tx, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Provide a new LN invoice with given parameters
        /// </summary>
        /// <param name="amount">number in Satoshi</param>
        /// <param name="memo">encoded in invoice</param>
        /// <param name="anon">flag to specify if invoice is unrelated to a user account</param>
        /// <param name="use">what the invoice is used for</param>
        /// <param name="useId">target of invoice (user id, post id, etc)</param>
        /// <returns>LnRequestInvoiceResponse object which contains Invoice field</returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult GetDepositInvoice(string amount, string memo, string anon, string use, int? useId, int? useAction)
        {
            Response.AddHeader("X-Frame-Options", "DENY");
            bool isAnon = !(anon == null || anon != "1");
            if (!isAnon && !User.Identity.IsAuthenticated)
            {
                // This is a user-related invoice, and no user is logged in.
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
            }

            string userId;
            if (isAnon)
            {
                userId = null;
            }
            else
            {
                userId = User.Identity.GetUserId();
            }

            if (memo == null || memo == "")
            {
                memo = "Zapread.com";
            }

            if (Convert.ToInt64(amount, CultureInfo.InvariantCulture) > 1000)
            {
                return Json(new { success = false, message = "Deposits temporarily limited to 1000 satoshi" });
            }

            LndRpcClient lndClient = GetLndClient();

            var inv = lndClient.AddInvoice(Convert.ToInt64(amount, CultureInfo.InvariantCulture), memo: memo.SanitizeXSS(), expiry: "3600");

            LnRequestInvoiceResponse resp = new LnRequestInvoiceResponse()
            {
                Invoice = inv.payment_request,
                Result = "success",
                success = true,
            };

            //Create transaction record (not settled)
            using (ZapContext db = new ZapContext())
            {
                // TODO: ensure user exists?
                User user = null;
                if (userId != null)
                {
                    user = db.Users.Where(u => u.AppId == userId).First();
                }
                TransactionUse usedFor = TransactionUse.Undefined;
                TransactionUseAction usedForAction = TransactionUseAction.Undefined;
                int usedForId = useId != null ? useId.Value : -1;
                if (use == "tip")
                {
                    usedFor = TransactionUse.Tip;
                }
                else if (use == "votePost")
                {
                    usedFor = TransactionUse.VotePost;
                }
                else if (use == "voteComment")
                {
                    usedFor = TransactionUse.VoteComment;
                }
                else if (use == "userDeposit")
                {
                    usedFor = TransactionUse.UserDeposit;
                    usedForId = userId != null ? user.Id : -1;
                }

                if (useAction != null)
                {
                    if (useAction.Value == 0)
                    {
                        usedForAction = TransactionUseAction.VoteDown;
                    }
                    else if (useAction.Value == 1)
                    {
                        usedForAction = TransactionUseAction.VoteUp;
                    }
                }

                //create a new transaction record in database
                LNTransaction t = new LNTransaction()
                {
                    User = user,
                    IsSettled = false,
                    IsSpent = false,
                    Memo = memo.SanitizeXSS(),
                    Amount = Convert.ToInt64(amount, CultureInfo.InvariantCulture),
                    HashStr = inv.r_hash,
                    IsDeposit = true,
                    TimestampCreated = DateTime.Now,
                    PaymentRequest = inv.payment_request,
                    UsedFor = usedFor,
                    UsedForId = usedForId,
                    UsedForAction = usedForAction,
                };
                db.LightningTransactions.Add(t);
                db.SaveChanges();
                resp.Id = t.Id;
            }

            if (true) // debugging
            {
                // If a listener is not already running, this should start
                // Check if there is one already online.
                var numListeners = lndTransactionListeners.Count(kvp => kvp.Value.IsLive);

                // If we don't have one running - start it and subscribe
                if (numListeners < 1)
                {
                    var listener = lndClient.GetListener();
                    lndTransactionListeners.TryAdd(listener.ListenerId, listener);           // keep alive while we wait for payment
                    listener.InvoicePaid += 
                        async (invoice) => await NotifyClientsInvoicePaid(invoice)
                                                    .ConfigureAwait(true);                   // handle payment message
                    listener.StreamLost += OnListenerLost;                                   // stream lost
                    var a = new Task(() => listener.Start());                                // listen for payment
                    a.Start();
                }
            }
            return Json(resp);
        }

        /// <summary>
        /// If the connection to LND is closed, this is triggered.  Remove our reference to the listener object for the stream.
        /// </summary>
        /// <param name="l"></param>
        private static void OnListenerLost(TransactionListener l)
        {
            lndTransactionListeners.TryRemove(l.ListenerId, out TransactionListener oldListener);
        }

        // We have received asynchronous notification that a lightning invoice has been paid
        private async static Task NotifyClientsInvoicePaid(Invoice invoice)
        {
            // Check if the invoice received was paid.  LND also sends updates 
            // for new invoices to the invoice stream.  We want to listen for settled invoices here.
            if (!invoice.settled.HasValue)
            {
                // Bad invoice
                // Todo - logging
                return;
            }
            if (!invoice.settled.Value)
            {
                // Optional - add some logic to check invoices on the stream.  These invoices
                // which are not settled are likely new deposit requests.  For the purposes of
                // this function, we only care about settled invoices.
                return;
            }

            // This is the amount which was paid - needed in case of 0 (any) value invoices
            var amount = Convert.ToInt64(invoice.amt_paid_sat, CultureInfo.InvariantCulture);

            // Update LN transaction status in db
            using (ZapContext db = new ZapContext())
            {
                // Check if unsettled transaction exists in db matching the invoice that was just settled.
                var tx = db.LightningTransactions
                    .Include(tr => tr.User)
                    .Where(tr => tr.PaymentRequest == invoice.payment_request)
                    .ToList();

                DateTime settletime = DateTime.UtcNow;
                LNTransaction t;
                if (tx.Count > 0) // Shouldn't ever be more than one entry - could add a check for this.
                {
                    // We found it - mark it as settled.
                    t = tx.First();
                    t.TimestampSettled = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc) + TimeSpan.FromSeconds(Convert.ToInt64(invoice.settle_date));
                    t.IsSettled = true;

                    if (t.Amount != amount)
                    {
                        if (t.Amount == 0)
                        {
                            // This was a zero-invoice
                            t.Amount = amount; // this will be saved to DB
                        }
                    }
                }
                else
                {
                    // This invoice is not in the db - it may not be related to this service.  
                    // We still record it in our database for any possible user forensics/history later.
                    settletime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc) 
                        + TimeSpan.FromSeconds(Convert.ToInt64(invoice.settle_date, CultureInfo.InvariantCulture));
                    t = new LNTransaction()
                    {
                        IsSettled = invoice.settled.Value,
                        Memo = invoice.memo.SanitizeXSS(),
                        Amount = amount,//Convert.ToInt64(invoice.value, CultureInfo.InvariantCulture),
                        HashStr = invoice.r_hash,
                        IsDeposit = true,
                        TimestampSettled = settletime,
                        TimestampCreated = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc) + TimeSpan.FromSeconds(Convert.ToInt64(invoice.creation_date)),
                        PaymentRequest = invoice.payment_request,
                        User = null,
                    };
                    db.LightningTransactions.Add(t);
                }
                await db.SaveChangesAsync().ConfigureAwait(true);

                // Financial transaction
                double userBalance = 0.0;   // This value will be returned later

                if (t.User != null) // the user could be null if it is an anonymous payment.
                {
                    var userFunds = await db.Users
                        .Where(u => u.Id == t.User.Id)
                        .Select(u => u.Funds)
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    // Make every attempt to save user balance in DB
                    int attempts = 0;
                    bool saveFailed;
                    bool saveAborted = false;
                    do
                    {
                        attempts++;
                        saveFailed = false;

                        if (attempts < 50)
                        {
                            // This really shouldn't happen!
                            if (userFunds == null)
                            {
                                // this should not happen? - verify.  Maybe this is the case for transactions related to votes?
                                // throw new Exception("Error accessing user information related to settled LN Transaction.");
                            }
                            else
                            {
                                // Update user balance - this is a deposit.
                                userFunds.Balance += amount;// Convert.ToInt64(invoice.value, CultureInfo.InvariantCulture);
                                userBalance = Math.Floor(userFunds.Balance);
                            }
                            try
                            {
                                db.SaveChanges(); // synchronous
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
                            saveAborted = true;
                        }
                    }
                    while (saveFailed);

                    // Don't record as settled if save was aborted due to DB concurrency failure.  
                    // LND database will show it was settled, but DB not.
                    // Another process can check DB sync and correct later.
                    if (saveAborted == false)
                    {
                        t.IsSettled = invoice.settled.Value;
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }
                else
                {
                    t.IsSettled = invoice.settled.Value;
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }

                // Send live signal to listening clients on websockets/SignalR
                //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                //context.Clients.All.NotifyInvoicePaid(new { invoice = invoice.payment_request, balance = userBalance, txid = t.Id });
                await NotificationService.SendPaymentNotification(
                    t.User == null ? "" : t.User.AppId,
                    invoice: invoice.payment_request, 
                    userBalance: userBalance, 
                    txid: t.Id).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request">Lightning invoice</param>
        /// <returns></returns>
        [HttpPost, ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> ValidatePaymentRequest(string request)
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, message = "User not authorized." });
            }

            using (var db = new ZapContext())
            {
                var invoice = request.SanitizeXSS();

                // Check if the request has previously been submitted
                var t = await db.LightningTransactions
                    .Where(tx => tx.PaymentRequest == invoice)
                    .SingleOrDefaultAsync().ConfigureAwait(true);

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
                            return Json(new { success = false, message = "Zero- or any-value invoices not supported" });
                        }

                        if (amount > 5000)
                        {
                            return Json(new { success = false, message = "Withdraws temporarily limited to 5000 Satoshi" });
                        }

                        // Check user balance
                        var userFunds = await db.Users
                            .Where(u => u.AppId == userAppId)
                            .Select(u => new
                            {
                                u.Funds.Balance
                            })
                            .FirstOrDefaultAsync().ConfigureAwait(true);

                        if (userFunds == null)
                        {
                            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return Json(new { success = false, message = "User not found in database." });
                        }

                        if (userFunds.Balance < amount)
                        {
                            return Json(new { success = false, message = "Insufficient Funds. You have " + userFunds.Balance.ToString("0.", CultureInfo.InvariantCulture) + ", invoice is for " + decoded.num_satoshis + "." });
                        }

                        // Save the invoice to database
                        var user = await db.Users
                           .Where(u => u.AppId == userAppId)
                           .FirstAsync().ConfigureAwait(true);

                        //create a new transaction record in database
                        t = new LNTransaction()
                        {
                            IsSettled = false,
                            Memo = (decoded.description ?? "Withdraw").SanitizeXSS(),
                            HashStr = decoded.payment_hash,
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
                            withdrawId = t.WithdrawId,//.Id,
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
                        return Json(new { success = false, message = "Invalid withdraw request." });
                    }

                    // Check balance now
                    var userFunds = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => new
                        {
                            u.Funds.Balance
                        })
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    if (userFunds == null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return Json(new { success = false, message = "User not found in database." });
                    }

                    double amount = Convert.ToDouble(t.Amount, CultureInfo.InvariantCulture);

                    if (userFunds.Balance < amount)
                    {
                        return Json(new 
                        {
                            success = false, 
                            message = "Insufficient Funds. You have " 
                                + userFunds.Balance.ToString("0.", CultureInfo.InvariantCulture) 
                                + ", invoice is for " + t.Amount + "." });
                    }

                    return Json(new
                    {
                        success = true,
                        withdrawId = t.WithdrawId,//t.Id,
                        num_satoshis = t.Amount,
                        destination = t.NodePubKey,
                    });
                }
            }
            return Json(new 
            { 
                success=false, 
                message="Error decoding invoice." 
            });
        }

        /// <summary>
        /// Pay a lightning invoice.
        /// </summary>
        /// <param name="withdrawId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public ActionResult SubmitPaymentRequest(string withdrawId)//string request)
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, message = "User not authorized." });
            }

            using (var db = new ZapContext())
            {
                var wguid = new Guid(withdrawId);
                var lntx = db.LightningTransactions
                    .Where(tx => tx.WithdrawId == wguid)
                    .Where(tx => tx.User.AppId == userAppId)
                    .AsNoTracking()
                    .FirstOrDefault();

                if (lntx == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "Invalid withdraw request." });
                }

                if (lntx.IsSettled || lntx.IsIgnored || lntx.IsLimbo || lntx.IsDeposit || lntx.IsError)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "Invalid withdraw request." });
                }

                // Get interface to LND
                LndRpcClient lndClient = GetLndClient();

                // This is used for DoS or other attack detection
                string ip = GetClientIpAddress(Request);

                try
                {
                    // Submit Payment Request
                    var paymentResult = PaymentsService.TryWithdrawal(lntx, userAppId, ip, lndClient);
                    return Json(paymentResult);
                }
                catch (RestException e)
                {
                    // The request to LND threw an exception
                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n invoice: " + lntx.PaymentRequest
                            + "\r\n user: " + userAppId
                            + "\r\n error Content: " + e.Content
                            + "\r\n HTTP Status: " + e.StatusDescription,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User withdraw error 1",
                    });
                    return Json(new { Result = "Error processing request." });
                }
                catch (Exception e)
                {
                    MailingService.Send(new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n invoice: " + lntx.PaymentRequest + "\r\n user: " + userAppId,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User withdraw error 1b",
                    });
                    return Json(new { Result = "Error processing request." });
                }
            }
        }

        private static LndRpcClient GetLndClient()
        {
            LndRpcClient lndClient;
            using (var db = new ZapContext())
            {
                var g = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                    .AsNoTracking()
                    .FirstOrDefault();

                lndClient = new LndRpcClient(
                host: g.LnMainnetHost,
                macaroonAdmin: g.LnMainnetMacaroonAdmin,
                macaroonRead: g.LnMainnetMacaroonRead,
                macaroonInvoice: g.LnMainnetMacaroonInvoice);
            }

            return lndClient;
        }

        public static string GetClientIpAddress(HttpRequestBase request)
        {
            try
            {
                var userHostAddress = request.UserHostAddress;

                // Attempt to parse.  If it fails, we catch below and return "0.0.0.0"
                // Could use TryParse instead, but I wanted to catch all exceptions
                IPAddress.Parse(userHostAddress);

                var xForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(xForwardedFor))
                    return userHostAddress;

                // Get a list of public ip addresses in the X_FORWARDED_FOR variable
                var publicForwardingIps = xForwardedFor.Split(',').Where(ip => !IsPrivateIpAddress(ip)).ToList();

                // If we found any, return the last one, otherwise return the user host address

                var retval = publicForwardingIps.Any() ? publicForwardingIps.Last() : userHostAddress;

                return retval;
            }
            catch (Exception)
            {
                // Always return all zeroes for any failure (my calling code expects it)
                return "0.0.0.0";
            }
        }

        private static bool IsPrivateIpAddress(string ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var ip = IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }
    }
}