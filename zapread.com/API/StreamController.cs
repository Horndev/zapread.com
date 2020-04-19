using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Models.API.Stream;

namespace zapread.com.API
{
    /// <summary>
    /// API interface for websocket real-time streaming
    /// </summary>
    public class StreamController : ApiController
    {
        /// <summary>
        /// Request connection information for a streaming socket.
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Authorize]
        [Route("api/v1/stream/request")]
        public async Task<ConnectionInfoResponse> RequestConnection()
        {
            var url = ConfigurationManager.AppSettings.Get("wshost");
            return new ConnectionInfoResponse() 
            { 
                success = true, 
                url = url 
            };
        }
    }
}
