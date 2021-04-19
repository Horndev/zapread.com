using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using zapread.com.Database;
using zapread.com.Models.API.DataTables;
using zapread.com.Models.Database;
using System.Globalization;
using zapread.com.Models.API.Groups;
using System.Web.Http;

namespace zapread.com.API
{
    /// <summary>
    /// This controller should support a front end using https://imballinst.github.io/react-bs-datatable/?path=/story/advanced-guides--asynchronous-table
    /// </summary>
    public class GroupsController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/groups/list")]
        public async Task<ListGroupsResponse> List(DataTableParameters dataTableParameters)
        {
            if (dataTableParameters == null)
            {
                //Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new ListGroupsResponse() { success = false, message = "No query provided" };
            }

            using (var db = new ZapContext())
            {
                var search = dataTableParameters.Search;
                
                // We need to know the user making the call (if any) so we know if they are a member of the group or not
                User user = await GetCurrentUser(db).ConfigureAwait(true);
                int userid = user != null ? user.Id : 0;

                // Build query
                var groupsQ = db.Groups
                    .Select(g => new
                    {
                        numPosts = g.Posts.Count,
                        numMembers = g.Members.Count,
                        IsMember = g.Members.Select(m => m.Id).Contains(userid),
                        IsModerator = g.Moderators.Select(m => m.Id).Contains(userid),
                        IsAdmin = g.Administrators.Select(m => m.Id).Contains(userid),
                        g.GroupId,
                        g.GroupName,
                        g.Tags,
                        g.TotalEarned,
                        g.TotalEarnedToDistribute,
                        g.CreationDate,
                        g.Icon,
                        g.Tier,
                        IconId = g.GroupImage == null ? 0 : g.GroupImage.ImageId
                    }).AsNoTracking();

                if (search != null && search.Value != null)
                {
                    groupsQ = groupsQ.Where(g => g.GroupName.Contains(search.Value) || g.Tags.Contains(search.Value));
                }

                groupsQ = groupsQ.OrderByDescending(g => g.TotalEarned + g.TotalEarnedToDistribute);

                var groups = await groupsQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToListAsync().ConfigureAwait(false);

                var values = groups.Select(g => new GroupInfo()
                {
                    Id = g.GroupId,
                    CreatedddMMMYYYY = g.CreationDate == null ? "2 Aug 2018" : g.CreationDate.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                    Name = g.GroupName,
                    NumMembers = g.numMembers,
                    NumPosts = g.numPosts,
                    Tags = g.Tags != null ? g.Tags.Split(',').ToList() : new List<string>(),
                    Icon = g.Icon != null ? "fa-" + g.Icon : null, // "fa-bolt",  // NOTE: this is legacy, and will eventually be replaced.  All new groups will have image icons.
                    Level = g.Tier,
                    Progress = GetGroupProgress(g.TotalEarned, g.TotalEarnedToDistribute, g.Tier),
                    IsMember = g.IsMember,
                    IsLoggedIn = user != null,
                    IsMod = g.IsModerator,
                    IsAdmin = g.IsAdmin,
                }).ToList();

                var ret = new ListGroupsResponse()
                {
                    success = true,
                    draw = dataTableParameters.Draw,
                    recordsTotal = await db.Groups.CountAsync().ConfigureAwait(false),
                    recordsFiltered = await groupsQ.CountAsync().ConfigureAwait(false),
                    data = values
                };

                return ret;
            }
            
        }

        private async Task<User> GetCurrentUser(ZapContext db)
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users
                .Include(u => u.Settings)
                .FirstOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(true);
            return user;
        }

        protected static int GetGroupProgress(double TotalEarned, double TotalEarnedToDistribute, double Tier)
        {
            var e = TotalEarned + TotalEarnedToDistribute;
            //var level = GetGroupLevel(g);

            if (Tier == 0)
            {
                return Convert.ToInt32(100.0 * e / 1000.0);
            }
            if (Tier == 1)
            {
                return Convert.ToInt32(100.0 * (e - 1000.0) / 10000.0);
            }
            if (Tier == 2)
            {
                return Convert.ToInt32(100.0 * (e - 10000.0) / 50000.0);
            }
            if (Tier == 3)
            {
                return Convert.ToInt32(100.0 * (e - 50000.0) / 200000.0);
            }
            if (Tier == 4)
            {
                return Convert.ToInt32(100.0 * (e - 200000.0) / 500000.0);
            }
            if (Tier == 5)
            {
                return Convert.ToInt32(100.0 * (e - 500000.0) / 1000000.0);
            }
            if (Tier == 6)
            {
                return Convert.ToInt32(100.0 * (e - 1000000.0) / 5000000.0);
            }
            if (Tier == 7)
            {
                return Convert.ToInt32(100.0 * (e - 5000000.0) / 10000000.0);
            }
            if (Tier == 8)
            {
                return Convert.ToInt32(100.0 * (e - 10000000.0) / 20000000.0);
            }
            if (Tier == 9)
            {
                return Convert.ToInt32(100.0 * (e - 20000000.0) / 50000000.0);
            }
            return 100;
        }
    }
}