using Hangfire;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.API;
using zapread.com.Models.API.Admin;
using zapread.com.Models.Database;
using zapread.com.Services;

namespace zapread.com.API
{
    /// <summary>
    /// Administrative API
    /// </summary>
    [Authorize]
    [Route("api/v1/admin")]
    public partial class AdminController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/admin/pos/subscriptions/sync")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> SyncPOSSubscriptions()
        {
            await pointOfSaleService.SyncSubscriptions();
            
            return Ok();
        }
    }
}