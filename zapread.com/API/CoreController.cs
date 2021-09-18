using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Models.API.Core;

namespace zapread.com.API
{
    /// <summary>
    /// This controller is for accessing basic/common information about the website
    /// </summary>
    public class CoreController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/core/langages/list")]
        public GetLanguagesResponse GetLanguages()
        {
            try
            {
                // List of languages known
                var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                    .GroupBy(ci => ci.TwoLetterISOLanguageName)
                    .Select(g => g.First())
                    .Select(ci => ci.Name + ":" + ci.NativeName).ToList();

                return new GetLanguagesResponse()
                {
                    Languages = languages,
                    success = true
                };
            }
            catch (Exception e)
            {
                return new GetLanguagesResponse()
                {
                    success = false
                };
            }
        }
    }
}
