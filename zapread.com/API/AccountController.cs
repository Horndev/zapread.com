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
using zapread.com.Models.API.DataTables;
using zapread.com.Models.Database;

namespace zapread.com.API
{
    /// <summary>
    /// API controller to manage a user's account
    /// </summary>
    public class AccountController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Authorize]
        [Route("api/v1/account/quickvote/update")]
        public async Task<IHttpActionResult> UpdateQuickVote(UpdateQuickVoteRequest req)
        {
            var userAppId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userAppId))
                return Unauthorized();
            using (var db = new ZapContext())
            {
                var userFunds = await db.Users.Where(u => u.AppId == userAppId).Select(u => u.Funds).FirstOrDefaultAsync();
                bool saveFailed;
                int attempts = 0;
                do
                {
                    attempts++;
                    saveFailed = false;
                    if (attempts > 50)
                        return InternalServerError();
                    if (req.QuickVoteAmount != userFunds.QuickVoteAmount)
                    {
                        userFunds.QuickVoteAmount = req.QuickVoteAmount;
                    }

                    if (req.QuickVoteOn != userFunds.QuickVoteOn)
                    {
                        userFunds.QuickVoteOn = req.QuickVoteOn;
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
                    {
                        saveFailed = true;
                        foreach (var entry in ex.Entries) //.Single();
                        {
                            entry.Reload();
                        }
                    }
                }
                while (saveFailed);
                return Ok(new ZapReadResponse()
                {success = true, });
            }
        }

        /// <summary>
        /// Generate a new API key assigned to the authorized user.
        /// </summary>
        /// <param name = "roles">A comma-separated list of roles which the new key should have.</param>
        /// <returns>APIKeyResponse</returns>
        [AcceptVerbs("GET")]
        [Authorize]
        [Route("api/v1/account/apikeys/new")]
        public async Task<APIKeyResponse> RequestAPIKey(string roles)
        {
            var userAppId = User.Identity.GetUserId();
            // Only an administrator can grant Administrator
            roles = ""; //only grant APIUser
            using (var db = new ZapContext())
            {
                string apiRoles = "APIUser";
                if (!String.IsNullOrEmpty(roles))
                {
                    apiRoles = apiRoles + "," + roles;
                }

                var user = await db.Users.Where(u => u.AppId == userAppId).FirstOrDefaultAsync().ConfigureAwait(true);
                APIKey newKey = new APIKey()
                {Key = Guid.NewGuid().ToString(), Roles = apiRoles, User = user, };
                db.APIKeys.Add(newKey);
                if (roles != "test")
                {
                    // don't save if testing
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }

                return new APIKeyResponse()
                {success = true, Key = new UserAPIKey()
                {Key = "ZR" + newKey.Key, Roles = apiRoles, }, UserAppId = userAppId, };
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
                var keys = await db.APIKeys.Where(k => k.User.AppId == userAppId).Select(k => new UserAPIKey()
                {Key = "ZR" + k.Key, Roles = k.Roles, }).ToListAsync().ConfigureAwait(true);
                return new APIKeysResponse()
                {success = true, UserAppId = userAppId, Keys = keys, };
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
                var dbKey = await db.APIKeys.Where(k => k.User.AppId == userAppId).Where(k => "ZR" + k.Key == key).FirstOrDefaultAsync().ConfigureAwait(true);
                if (dbKey == null)
                {
                    return new ZapReadResponse()
                    {success = false, message = Properties.Resources.ErrorAPIKeyNotFound, };
                }

                db.APIKeys.Remove(dbKey);
                await db.SaveChangesAsync().ConfigureAwait(true);
                return new ZapReadResponse()
                {success = true, };
            }
        }

        /// <summary>
        /// Query account lightning transaction history
        /// </summary>
        /// <param name = "dataTableParameters"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Authorize]
        [Route("api/v1/account/transactions/lightning")]
        public async Task<LightningTransactionsPageResponse> LNHistory(DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                return new LightningTransactionsPageResponse()
                {success = false};
            }

            var userId = User.Identity.GetUserId();
            using (var db = new ZapContext())
            {
                var valuesq = db.Users.Where(u => u.AppId == userId).SelectMany(u => u.LNTransactions).OrderByDescending(t => t.TimestampCreated).Select(t => new
                {
                t.Id, Time = t.TimestampCreated, Type = t.IsDeposit, t.Amount, t.Memo, t.IsSettled, t.IsLimbo, });
                if (dataTableParameters.Filter == "Completed")
                {
                    valuesq = valuesq.Where(t => t.IsSettled);
                }

                if (dataTableParameters.Filter == "Processing")
                {
                    valuesq = valuesq.Where(t => t.IsLimbo);
                }

                if (dataTableParameters.Filter == "Failed/Cancelled")
                {
                    valuesq = valuesq.Where(t => !t.IsSettled);
                }

                // paginate
                valuesq = valuesq.Skip(dataTableParameters.Start).Take(dataTableParameters.Length);
                int numrec = await db.Users.Where(u => u.AppId == userId).SelectMany(u => u.LNTransactions).CountAsync().ConfigureAwait(true);
                return new LightningTransactionsPageResponse()
                {draw = dataTableParameters.Draw, recordsTotal = numrec, recordsFiltered = numrec, data = await valuesq.Select(v => new LightningTransactionsInfo()
                {Id = v.Id, Time = v.Time, // == null ? "" : v.Time.Value.ToString("yyyy-MM-dd HH:mm:ss"),
 Type = v.Type, Amount = v.Amount, Memo = v.Memo, IsSettled = v.IsSettled, IsLimbo = v.IsLimbo, }).ToListAsync().ConfigureAwait(true)};
            }
        }
    }
}