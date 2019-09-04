using System.Reflection;
using System.Web.Mvc;

namespace zapread.com.Helpers
{
    public static class UrlHelperExtensions
    {
        //https://stackoverflow.com/questions/5818799/autoversioning-css-js-in-asp-net-mvc
        public static string ContentVersioned(this UrlHelper self, string contentPath)
        {
            string versionedContentPath = contentPath + "?v=" + Assembly.GetAssembly(typeof(UrlHelperExtensions)).GetName().Version.ToString();
            return self.Content(versionedContentPath);
        }
    }
}