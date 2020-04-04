using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace zapread.com.API
{
    /// <summary>
    /// API for ZapRead users
    /// </summary>
    [Route("api/v1/user")]
    public class UserController : ApiController
    {
        // GET api/v1/user
        /// <summary>
        /// Get a user
        /// </summary>
        /// <returns></returns>
        public string Get()
        {
            return "Response from User V1";
        }
    }
}
