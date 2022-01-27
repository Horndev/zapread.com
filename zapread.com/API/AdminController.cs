using Hangfire;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.API;
using zapread.com.Services;

namespace zapread.com.API
{
    /// <summary>
    /// Administrative API
    /// </summary>
    [Authorize]
    [Route("api/v1/admin")]
    public class AdminController : ApiController
    {
        /// <summary>
        /// Refresh check if user is online.  This is needed sometimes when the DB is out of sync.
        ///   Requires Administator role.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>success={true|false}</returns>
        [Route("api/v1/admin/checkonline/{userId}")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator,APIUser")]
        public async Task<ZapReadResponse> CheckOnline(int userId)
        {
            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.Id == userId)
                    .SingleOrDefaultAsync().ConfigureAwait(false);

                var jobId = BackgroundJob.Schedule<UserState>(
                    methodCall: x => x.UserOffline(user.AppId, user.Name, DateTime.UtcNow),
                    delay: TimeSpan.FromMinutes(1));

                // Save the jobId so we don't schedule another check
                user.PGPPubKey = jobId;

                await db.SaveChangesAsync().ConfigureAwait(true);

                return new ZapReadResponse() { success = true };
            }
        }

        /// <summary>
        /// Get status of current assets (total Zapread assets on Node + liquidity)
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/admin/accounting/summary")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> GetAccountingSummary()
        {
            using (var db = new ZapContext())
            {
                var website = await db.ZapreadGlobals
                    .Include(z => z.EarningEvents)
                    .FirstOrDefaultAsync(i => i.Id == 1).ConfigureAwait(false);

                if (website == null)
                {
                    return InternalServerError();
                }

                return Ok();
            }
        }

        /// <summary>
        /// Get status of current assets (total Zapread assets on Node + liquidity)
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Route("api/v1/admin/accounting/assets/{year}/{month}")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> GetAccountingAssets(int year, int month)
        {
            using (var db = new ZapContext())
            {
                return Ok();
            }
        }

        /// <summary>
        /// Get a summary of the accounting liabilities (user balances)
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Route("api/v1/admin/accounting/liability/{year}/{month}")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> GetAccountingLiability(int year, int month)
        {
            using (var db = new ZapContext())
            {
                try
                {

                    var q = await db.Users.Where(u => u.Funds != null).Select(u => u.Funds)
                        .GroupBy(u => u.IsWithdrawLocked)
                        .Select(g => new
                        {
                            isWithdrawLocked = g.Key,
                            balance = g.Sum(f => f.Balance),
                            limbo = g.Sum(f => f.LimboBalance),
                            count = g.Count()
                        }).ToListAsync().ConfigureAwait(false);

                    var header = new Row() { Cells = new List<Cell>() };
                    header.Cells.Add(new Cell() { value = "Account Type" });
                    header.Cells.Add(new Cell() { value = "User Count" });
                    header.Cells.Add(new Cell() { value = "Balance (Sat)" });
                    header.Cells.Add(new Cell() { value = "Limbo (Sat)" });
                    header.Cells.Add(new Cell() { value = "Total (Sat)" });

                    AccountingSummaryResponse response = new AccountingSummaryResponse() { data = new List<Row>() };
                    response.data.Add(header);

                    double totalUsers = 0;
                    double totalBalance = 0;
                    double totalLimbo = 0;
                    foreach (var entry in q)
                    {
                        var row = new Row() { Cells = new List<Cell>() };
                        row.Cells.Add(new Cell() { value = entry.isWithdrawLocked ? "Locked" : "Normal" });
                        row.Cells.Add(new Cell() { value = entry.count });
                        row.Cells.Add(new Cell() { value = entry.balance });
                        row.Cells.Add(new Cell() { value = entry.limbo });
                        row.Cells.Add(new Cell() { value = entry.balance+entry.limbo });
                        response.data.Add(row);
                        totalUsers += entry.count;
                        totalBalance += entry.balance;
                        totalLimbo += entry.limbo;
                    }

                    var footer = new Row() { Cells = new List<Cell>() };
                    footer.Cells.Add(new Cell() { value = "Total:" });
                    footer.Cells.Add(new Cell() { value = totalUsers });
                    footer.Cells.Add(new Cell() { value = totalBalance });
                    footer.Cells.Add(new Cell() { value = totalLimbo });
                    footer.Cells.Add(new Cell() { value = totalBalance+ totalLimbo });

                    response.data.Add(footer);

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Route("api/v1/admin/accounting/revenue/daily/{year}/{month}")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> GetAccountingRevenueByDay(int year, int month)
        {
            using (var db = new ZapContext())
            {
                var website = await db.ZapreadGlobals
                    .Include(z => z.EarningEvents)
                    .FirstOrDefaultAsync(i => i.Id == 1).ConfigureAwait(false);

                if (website == null)
                {
                    return InternalServerError();
                }

                var startDate = new DateTime(year: year, month: month, day: 1, hour: 0, minute: 0, second: 0, millisecond: 0, DateTimeKind.Utc);

                var transactionsByday = website.EarningEvents.Where(e => e.TimeStamp.Value.Year == year && e.TimeStamp.Value.Month == month )
                    .OrderBy(e => e.TimeStamp)
                    .GroupBy(e => e.TimeStamp.Value.Day)
                    .Select(e => new
                    {
                        day = e.Key,
                        total = e.Sum(v => v.Amount),
                        average = e.Average(v => v.Amount),
                        count = e.Count(),
                        posts = e.Count(v => v.OriginType == 0),
                        comments = e.Count(v => v.OriginType == 1),
                    })
                    .ToList();

                var header = new Row() { Cells = new List<Cell>() };
                header.Cells.Add(new Cell() { value = "Year" });
                header.Cells.Add(new Cell() { value = "Month" });
                header.Cells.Add(new Cell() { value = "Day" });
                header.Cells.Add(new Cell() { value = "Earned" });
                header.Cells.Add(new Cell() { value = "Transactions" });
                header.Cells.Add(new Cell() { value = "Average Earned" });
                header.Cells.Add(new Cell() { value = "Posts" });
                header.Cells.Add(new Cell() { value = "Comments" });

                AccountingSummaryResponse response = new AccountingSummaryResponse() { data = new List<Row>() };

                response.data.Add(header);

                double totalEarned = 0;
                double totalEvents = 0;
                double totalPosts = 0;
                double totalComments = 0;
                foreach (var transaction in transactionsByday)
                {                   
                    var row = new Row() { Cells = new List<Cell>() };
                    row.Cells.Add(new Cell() { value = year });
                    row.Cells.Add(new Cell() { value = month });
                    row.Cells.Add(new Cell() { value = transaction.day });
                    row.Cells.Add(new Cell() { value = transaction.total });
                    row.Cells.Add(new Cell() { value = transaction.count });
                    row.Cells.Add(new Cell() { value = transaction.average });
                    row.Cells.Add(new Cell() { value = transaction.posts });
                    row.Cells.Add(new Cell() { value = transaction.comments });
                    response.data.Add(row);
                    totalEarned += transaction.total;
                    totalEvents += transaction.count;
                    totalPosts += transaction.posts;
                    totalComments += transaction.comments;
                }

                var footer = new Row() { Cells = new List<Cell>() };
                footer.Cells.Add(new Cell() { value = "Total:" });
                footer.Cells.Add(new Cell() { value = "" });
                footer.Cells.Add(new Cell() { value = "" });
                footer.Cells.Add(new Cell() { value = totalEarned });
                footer.Cells.Add(new Cell() { value = totalEvents });
                footer.Cells.Add(new Cell() { value = totalEvents > 0 ? totalEarned / totalEvents : 0 });
                footer.Cells.Add(new Cell() { value = totalPosts });
                footer.Cells.Add(new Cell() { value = totalComments });

                response.data.Add(footer);

                return Ok(response);
            }
        }

        private class AccountingSummaryResponse : ZapReadResponse
        {
            public List<Row> data;
        }

        private class Row
        {
            public List<Cell> Cells { get; set; }
        }

        private class Cell
        {
            public object value { get; set; }
        }
    }
}
