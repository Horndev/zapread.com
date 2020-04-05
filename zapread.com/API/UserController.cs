using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Models.API;
using zapread.com.Services;

namespace zapread.com.API
{
    /// <summary>
    /// API for ZapRead users
    /// </summary>
    
    public class UserController : ApiController
    {
        // GET api/v1/user
        /// <summary>
        /// Test call - doesn't do anything right now
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user")]
        public string Get()
        {
            return "Response from User V1";
        }

        /// <summary>
        /// Get the user balance (as determined by API key used)
        /// </summary>
        /// <returns>UserBalanceResponse</returns>
        [Route("api/v1/user/balance")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator,APIUser")]
        public async Task<UserBalanceResponse> Balance()
        {
            double userBalance = 0.0;
            userBalance = await GetUserBalance().ConfigureAwait(true);

            string balance = userBalance.ToString("0.##", CultureInfo.InvariantCulture);

            HttpContext.Current.Response.Headers.Add("X-Frame-Options", "DENY");
            return new UserBalanceResponse() { success = true, balance=balance };
        }

        private async Task<double> GetUserBalance()
        {
            double balance = 0.0;
            var userAppId = User.Identity.GetUserId();            // Get the logged in user ID

            if (userAppId == null)
            {
                return 0.0;
            }

            try
            {
                using (var db = new ZapContext())
                {
                    var userBalance = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => new
                        {
                            Value = u.Funds == null ? -1 : u.Funds.Balance
                        })
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    if (userBalance == null)
                    {
                        // User not found in database, or not logged in
                        return 0.0;
                    }
                    else
                    {
                        balance = userBalance.Value;
                    }
                }
            }
            catch (Exception e)
            {
                MailingService.Send(new UserEmailModel()
                {
                    Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                    Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n method: UserBalance" + "\r\n user: " + userAppId,
                    Email = "",
                    Name = "zapread.com Exception",
                    Subject = "User ApiController error",
                });

                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                balance = 0.0;
            }

            return Math.Floor(balance);
        }
    }

}
