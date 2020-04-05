using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Models.API;

namespace zapread.com.API
{
    public class PostController : ApiController
    {
        /// <summary>
        /// Get information about a post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [Route("api/v1/post/getinfo/{postId}")]
        [AcceptVerbs("GET")]
        public async Task<ZapReadResponse> GetInfo(int postId)
        {
            return new ZapReadResponse() { success = false, message = "Not yet implemented" };
        }
    }
}
