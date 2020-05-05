using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.API;
using zapread.com.Models.API.Account;
using zapread.com.Models.API.Account.Transactions;
using zapread.com.Models.Database;

namespace zapread.com.API
{
    /// <summary>
    /// API controller to manage a user's account
    /// </summary>
    public class AccountController : ApiController
    {
        /// <summary>
        /// Generate a new API key assigned to the authorized user.
        /// </summary>
        /// <param name="roles">A comma-separated list of roles which the new key should have.</param>
        /// <returns>APIKeyResponse</returns>
        [AcceptVerbs("GET")]
        [Authorize]
        [Route("api/v1/account/apikeys/new")]
        public async Task<APIKeyResponse> RequestAPIKey(string roles)
        {
            var userAppId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                string apiRoles = "APIUser";
                if (!String.IsNullOrEmpty(roles))
                {
                    apiRoles = apiRoles + "," + roles;
                }

                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                APIKey newKey = new APIKey()
                {
                    Key = Guid.NewGuid().ToString(),
                    Roles = apiRoles,
                    User = user,
                };
                db.APIKeys.Add(newKey);

                if (roles != "test")
                {
                    // don't save if testing
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }

                return new APIKeyResponse() { 
                    success = true, 
                    Key = new UserAPIKey()
                    {
                        Key = "ZR" + newKey.Key,
                        Roles = apiRoles,
                    },
                    UserAppId = userAppId,
                };
            }
        }

        /// <summary>
        /// Request a list of the user's API keys
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Authorize]
        [Route("api/v1/account/apikeys/list")]
        public async Task<APIKeysResponse> ListAPIKeys()
        {
            var userAppId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var keys = await db.APIKeys
                    .Where(k => k.User.AppId == userAppId)
                    .Select(k => new UserAPIKey()
                    {
                        Key = "ZR" + k.Key,
                        Roles = k.Roles,
                    })
                    .ToListAsync().ConfigureAwait(true);

                return new APIKeysResponse()
                {
                    success = true,
                    UserAppId = userAppId,
                    Keys = keys,
                };
            }
        }

        /// <summary>
        /// Deletes an API Key
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Authorize]
        [Route("api/v1/account/apikeys/revoke/{key}")]
        public async Task<ZapReadResponse> RevokeAPIKey(string key)
        {
            var userAppId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var dbKey = await db.APIKeys
                    .Where(k => k.User.AppId == userAppId)
                    .Where(k => "ZR" + k.Key == key)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (dbKey == null)
                {
                    return new ZapReadResponse()
                    {
                        success = false,
                        message = "API Key not found.",
                    };
                }

                db.APIKeys.Remove(dbKey);

                await db.SaveChangesAsync().ConfigureAwait(true);

                return new ZapReadResponse()
                {
                    success = true,
                };
            }
        }

        /// <summary>
        /// Query account lightning transaction history
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Authorize]
        [Route("api/v1/account/transactions/lightning")]
        public async Task<LightningTransactionsPageResponse> LNHistory(DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                return new LightningTransactionsPageResponse() { success = false };
            }

            var userId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var values = await db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.LNTransactions)
                    .OrderByDescending(t => t.TimestampCreated)
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .Select(t => new
                    {
                        t.Id,
                        Time = t.TimestampCreated,
                        Type = t.IsDeposit,
                        t.Amount,
                        t.Memo,
                        t.IsSettled,
                        t.IsLimbo,
                    })
                    .ToListAsync().ConfigureAwait(true);

                int numrec = await db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.LNTransactions)
                    .CountAsync().ConfigureAwait(true);

                return new LightningTransactionsPageResponse()
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values.Select(v => new LightningTransactionsInfo()
                    {
                        Id = v.Id,
                        Time = v.Time == null ? "" : v.Time.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        Type = v.Type,
                        Amount = v.Amount,
                        Memo = v.Memo,
                        IsSettled = v.IsSettled,
                        IsLimbo = v.IsLimbo,
                    })
                };
            }
        }
    }
}
