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
using zapread.com.Helpers;

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
                    recordsTotal = await groupsQ.CountAsync().ConfigureAwait(false),
                    //await db.Groups.CountAsync().ConfigureAwait(false),
                    recordsFiltered = await groupsQ.CountAsync().ConfigureAwait(false),
                    data = values
                };

                return ret;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/groups/checkexists")]
        public async Task<CheckExistsGroupResponse> CheckExists(CheckExistsGroupParameters p)
        {
            using (var db = new ZapContext())
            {
                var groupIdCheck = -1;
                if (p.GroupId.HasValue)
                {
                    groupIdCheck = p.GroupId.Value;
                }
                bool exists = await GroupExists(p.GroupName.CleanUnicode(), groupIdCheck, db).ConfigureAwait(true);
                return new CheckExistsGroupResponse() { exists = exists, success = true };
            }
        }

        private static async Task<bool> GroupExists(string GroupName, int groupId, ZapContext db)
        {
            Group matched = await db.Groups.Where(g => g.GroupName == GroupName).FirstOrDefaultAsync().ConfigureAwait(true);
            if (matched != null)
            {
                if (matched.GroupId != groupId)
                {
                    return true;
                }
            }
            return false;
        }

        [AcceptVerbs("POST")]
        [Route("api/v1/groups/add")]
        public async Task<AddGroupResponse> Add(AddGroupParameters newGroup)
        {
            if (newGroup == null)
            {
                // use this to return status code
                // https://www.tutorialsteacher.com/webapi/action-method-return-type-in-web-api
                return new AddGroupResponse()
                {
                    success = false
                };
            }

            using (var db = new ZapContext())
            {
                // Ensure not a duplicate group!
                var cleanName = newGroup.GroupName.CleanUnicode();
                bool exists = await GroupExists(cleanName, -1, db).ConfigureAwait(true);
                if (exists)
                {
                    return new AddGroupResponse() { success = false };
                }

                var user = await GetCurrentUser(db).ConfigureAwait(true);
                var icon = await GetGroupIcon(newGroup.ImageId, db).ConfigureAwait(true);

                Group g = new Group()
                {
                    GroupName = cleanName,
                    TotalEarned = 0.0,
                    TotalEarnedToDistribute = 0.0,
                    Moderators = new List<User>(),
                    Members = new List<User>(),
                    Administrators = new List<User>(),
                    Tags = newGroup.Tags,
                    Icon = null, //m.Icon,  // This field is now depricated - will be removed
                    GroupImage = icon,
                    CreationDate = DateTime.UtcNow,
                    DefaultLanguage = newGroup.Language == null ? "en" : newGroup.Language, // Ensure value
                };

                g.Members.Add(user);
                g.Moderators.Add(user);
                g.Administrators.Add(user);

                db.Groups.Add(g);
                await db.SaveChangesAsync().ConfigureAwait(true);

                return new AddGroupResponse()
                {
                    success = true,
                    GroupId = g.GroupId
                };
            }
        }

        /// <summary>
        /// Loads the information on a specified group
        /// </summary>
        /// <param name="groupInfo"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/groups/load")]
        public async Task<LoadGroupResponse> Load(LoadGroupParameters groupInfo)
        {
            // validate
            if (groupInfo == null)
            {
                return new LoadGroupResponse() { success = false };
                //throw new ArgumentNullException(nameof(groupInfo));
            }

            if (groupInfo.groupId == 0)
            {
                return new LoadGroupResponse() { success = false };
                //throw new ArgumentNullException(nameof(groupInfo));
            }

            using (var db = new ZapContext())
            {
                var reqGroupQ = await db.Groups
                    .Where(g => g.GroupId == groupInfo.groupId)
                    .Select(g => new
                    {
                        g.GroupId,
                        g.DefaultLanguage,
                        g.GroupName,
                        g.GroupImage.ImageId,
                        g.Tags
                    }).FirstOrDefaultAsync().ConfigureAwait(true);

                if (reqGroupQ == null)
                {
                    //Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return new LoadGroupResponse() { success = false, message = "Group not found." };
                }

                // Convert to GroupInfo object for return
                var reqGroup = new GroupInfo()
                {
                    Id = reqGroupQ.GroupId,
                    DefaultLanguage = reqGroupQ.DefaultLanguage == null ? "en" : reqGroupQ.DefaultLanguage,
                    Name = reqGroupQ.GroupName,
                    IconId = reqGroupQ.ImageId,
                    Tags = reqGroupQ.Tags != null ? reqGroupQ.Tags.Split(',').ToList() : new List<string>(),
                };
                    

                return new LoadGroupResponse() { success = true, group = reqGroup };
            }


            return new LoadGroupResponse()
            {
                success = false
            };
        }

        [AcceptVerbs("PUT")]
        [Route("api/v1/groups/update")]
        public async Task<IHttpActionResult> Update(UpdateGroupParameters existingGroup)
        {
            // validate
            if (existingGroup == null)
            {
                return BadRequest();
                //throw new ArgumentNullException(nameof(groupInfo));
            }

            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                var group = await db.Groups
                    .Where(g => g.GroupId == existingGroup.GroupId)
                    .Include(gr => gr.Administrators)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (group == null)
                {
                    return BadRequest("Group not found");
                }

                if (!group.Administrators.Select(a => a.AppId).Contains(user.AppId))
                {
                    return Unauthorized();
                }

                var cleanName = existingGroup.GroupName.CleanUnicode();
                bool exists = await GroupExists(cleanName, existingGroup.GroupId, db).ConfigureAwait(true);

                if (exists)
                {
                    return BadRequest("Group with that name already exists.");
                }

                // Make updates
                var icon = await GetGroupIcon(existingGroup.ImageId, db).ConfigureAwait(true);

                if (icon == null)
                {
                    return BadRequest("Icon not found");
                }

                group.DefaultLanguage = existingGroup.Language == null ? "en" : existingGroup.Language; // Ensure value
                group.Tags = existingGroup.Tags;
                
                group.GroupName = cleanName;
                group.GroupImage = icon;

                await db.SaveChangesAsync().ConfigureAwait(true);

                return Ok(new AddGroupResponse()
                {
                    success = true,
                    GroupId = group.GroupId
                });
            }
        }

        private static async Task<Models.UserImage> GetGroupIcon(int ImageId, ZapContext db)
        {
            var icon = await db.Images.Where(im => im.ImageId == ImageId).FirstOrDefaultAsync().ConfigureAwait(true);

            if (icon == null || icon.Image == null)
            {
                // Image 1 is usually the default
                icon = await db.Images.Where(im => im.ImageId == 1).FirstOrDefaultAsync().ConfigureAwait(true);
                if (icon == null || icon.Image == null)
                {
                    // Get any icon
                    icon = await db.Images.Where(im => im.Image != null).FirstOrDefaultAsync().ConfigureAwait(true);
                }
            }

            return icon;
        }

        private async Task<User> GetCurrentUser(ZapContext db)
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users
                .Include(u => u.Settings)
                .FirstOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(true);
            return user;
        }

        /// <summary>
        /// Returns the progress of the group to get to the next tier
        /// </summary>
        /// <param name="TotalEarned"></param>
        /// <param name="TotalEarnedToDistribute"></param>
        /// <param name="Tier"></param>
        /// <returns></returns>
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