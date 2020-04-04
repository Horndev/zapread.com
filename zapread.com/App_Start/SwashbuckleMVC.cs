using System.Web.Http;
using WebActivatorEx;
using zapread.com;
using Swashbuckle.MVC.Handler;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
[assembly: PreApplicationStartMethod(typeof(SwaggerMVCConfig), "Register")]
namespace zapread.com
{
    public class SwaggerMVCConfig
    {
		public static void Register()
        {
			// Need to clean up MVC view for this before enabling!
			//DynamicModuleUtility.RegisterModule(typeof(SwashbuckleMVCModule));
		}
	}
}