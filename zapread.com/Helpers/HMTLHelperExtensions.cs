using System;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace zapread.com
{
    /// <summary>
    /// 
    /// </summary>
    public static class HMTLHelperExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "html"></param>
        /// <param name = "controller"></param>
        /// <param name = "action"></param>
        /// <param name = "cssClass"></param>
        /// <returns></returns>
        public static string IsSelected(this HtmlHelper html, string controller = null, string action = null, string cssClass = null)
        {
            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];
            if (String.IsNullOrEmpty(controller))
                controller = currentController;
            if (String.IsNullOrEmpty(action))
                action = currentAction;
            return controller == currentController && action == currentAction ? cssClass : String.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "html"></param>
        /// <returns></returns>
        public static string PageClass(this HtmlHelper html)
        {
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            return currentAction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "htmlHelper"></param>
        /// <param name = "template"></param>
        /// <returns></returns>
        public static MvcHtmlString Script(this HtmlHelper htmlHelper, Func<object, HelperResult> template)
        {
            htmlHelper.ViewContext.HttpContext.Items["_script_" + Guid.NewGuid()] = template;
            return MvcHtmlString.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "htmlHelper"></param>
        /// <returns></returns>
        public static IHtmlString RenderPartialViewScripts(this HtmlHelper htmlHelper)
        {
            foreach (object key in htmlHelper.ViewContext.HttpContext.Items.Keys)
            {
                if (key.ToString().StartsWith("_script_"))
                {
                    var template = htmlHelper.ViewContext.HttpContext.Items[key] as Func<object, HelperResult>;
                    if (template != null)
                    {
                        htmlHelper.ViewContext.Writer.Write(template(null));
                    }
                }
            }

            return MvcHtmlString.Empty;
        }
    }
}