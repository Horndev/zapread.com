﻿using Microsoft.AspNet.Identity;
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
using zapread.com.Models.API.User;
using zapread.com.Services;

namespace zapread.com.API
{
    /// <summary>
    /// API for ZapRead users
    /// </summary>
    public class UserController : ApiController
    {
        /// <summary>
        /// Find a user
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/search")]
        public async Task<IHttpActionResult> Search(UserSearchRequest req)
        {
            if (req == null || req.Prefix == null || req.Max < 1)
            {
                return BadRequest();
            }

            using (var db = new ZapContext())
            {
                var users = await db.Users
                    .Where(u => u.Name.Contains(req.Prefix))
                    .OrderByDescending(u => u.DateLastActivity)
                    .Take(req.Max)
                    .Select(u => new UserResultInfo()
                    {
                        UserName = u.Name,
                        UserAppId = u.AppId,
                        ProfileImageVersion = u.ProfileImage != null ? u.ProfileImage.Version : 0
                    }).ToListAsync().ConfigureAwait(false);

                return Ok(new UserSearchResponse() { Users = users });
            }
        }

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
            return new UserBalanceResponse() { success = true, balance = balance };
        }

        /// <summary>
        /// Gets statistics on the user referrals
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user/referralstats")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetReferralStats()
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return Unauthorized();
            }

            using (var db = new ZapContext())
            {
                var referralInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Where(u => u.ReferralInfo != null)
                    .Select(u => u.ReferralInfo)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(true);

                var referredInfos = await db.Users
                       .Where(u => u.ReferralInfo != null && u.ReferralInfo.ReferredByAppId == userAppId)
                       .Select(u => u.ReferralInfo)
                       .AsNoTracking()
                       .ToListAsync().ConfigureAwait(true);

                // Check how many active and total
                var dateNow = DateTime.UtcNow;
                var numActive = referredInfos.Where(r => (dateNow - r.TimeStamp) < TimeSpan.FromDays(6 * 30)).Count();
                var numTotal = referredInfos.Count() - numActive;

                return Ok(new GetRefStatsResponse()
                {
                    ReferredByAppId = referralInfo != null ? referralInfo.ReferredByAppId ?? null : null,
                    TotalReferred = numTotal,
                    TotalReferredActive = numActive,
                    IsActive = referralInfo != null && (dateNow - referralInfo.TimeStamp) < TimeSpan.FromDays(6 * 30),
                });
            }
        }

        /// <summary>
        /// If not signed up for a referral, you can add another user as a referral.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/user/giftreferral")]
        public async Task<IHttpActionResult> GiftReferral(UserRefGiftRequest req)
        {
            if (req == null || String.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null)
            {
                return Unauthorized();
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (user.ReferralInfo != null)
                {
                    return BadRequest();
                }

                user.ReferralInfo = new Models.Database.Referral()
                {
                    ReferredByAppId = req.UserAppId,
                    TimeStamp = DateTime.UtcNow,
                };

                await db.SaveChangesAsync();

                return Ok(new UserRefGiftResponse() { success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user/referralcode")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetReferralCode()
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return Unauthorized();
            }

            using (var db = new ZapContext())
            {
                var refCode = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.ReferralCode)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (refCode == null)
                {
                    var user = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .FirstOrDefaultAsync().ConfigureAwait(true);
                    if (user == null) return NotFound();
                    user.ReferralCode = CryptoService.GetNewRefCode();
                    refCode = user.ReferralCode;
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }

                var numrefs = await db.Referrals
                    .Where(r => r.ReferredByAppId == userAppId)
                    .CountAsync();

                return Ok(new GetRefCodeResponse()
                {
                    refCode = refCode
                });
            }
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
