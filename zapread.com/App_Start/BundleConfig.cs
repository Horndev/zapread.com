using System.IO;
using System.Web.Hosting;
using System.Web.Optimization;

namespace zapread.com
{
    /// <summary>
    /// from https://stackoverflow.com/questions/15005481/mvc4-stylebundle-can-you-add-a-cache-busting-query-string-in-debug-mode
    /// </summary>
    internal static class BundleExtensions
    {
        public static Bundle WithLastModifiedToken(this Bundle sb)
        {
            sb.Transforms.Add(new LastModifiedBundleTransform());
            return sb;
        }
        public class LastModifiedBundleTransform : IBundleTransform
        {
            public void Process(BundleContext context, BundleResponse response)
            {
                foreach (var file in response.Files)
                {
                    var lastWrite = File.GetLastWriteTime(HostingEnvironment.MapPath(file.IncludedVirtualPath)).Ticks.ToString();
                    file.IncludedVirtualPath = string.Concat(file.IncludedVirtualPath, "?v=", lastWrite);
                }
            }
        }
    }

    /// <summary>
    /// This is the configuration for script bundling.  This is now actually done by webpack, but is relayed through here.
    /// </summary>
    public static class BundleConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundles"></param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region partial scripts

            // Partial script - summary statistics
            bundles.Add(new ScriptBundle("~/bundles/partial/summaryStatistics").Include(
                        "~/Scripts/Partial/summaryStatistics.js")
                        .WithLastModifiedToken());

            // Partial script - group admin bar
            bundles.Add(new ScriptBundle("~/bundles/partial/group/adminbar").Include(
                        "~/Scripts/Groups/adminbar.js")
                        .WithLastModifiedToken());

            // Partial script - group edit icon
            bundles.Add(new ScriptBundle("~/bundles/partial/group/editIcon").Include(
                        "~/Scripts/Groups/editIcon.js")
                        .WithLastModifiedToken());

            // Partial script - vote modal
            bundles.Add(new ScriptBundle("~/bundles/partial/messageCompose").Include(
                        "~/Scripts/Partial/messageCompose.js")
                        .WithLastModifiedToken());

            #endregion

            BundlePage(bundles, "account", "login");
            BundlePage(bundles, "account", "register");
            BundlePage(bundles, "admin", "achievements");
            BundlePage(bundles, "admin", "accounting");
            BundlePage(bundles, "admin", "audit");
            BundlePage(bundles, "admin", "icons");
            BundlePage(bundles, "admin", "index");
            BundlePage(bundles, "admin", "jobs");
            BundlePage(bundles, "admin", "lightning");
            BundlePage(bundles, "admin", "reactions");
            BundlePage(bundles, "admin", "users");
            BundlePage(bundles, "group", "detail");
            BundlePage(bundles, "group", "edit");
            BundlePage(bundles, "group", "index");
            BundlePage(bundles, "group", "members");
            BundlePage(bundles, "group", "new");
            BundlePage(bundles, "home", "about");
            BundlePage(bundles, "home", "faq");
            BundlePage(bundles, "home", "install");
            BundlePage(bundles, "home", "index");
            BundlePage(bundles, "home", "privacy");
            BundlePage(bundles, "home", "terms");
            BundlePage(bundles, "lnauth", "login");
            BundlePage(bundles, "mailer", "default");
            BundlePage(bundles, "manage", "apikeys");
            BundlePage(bundles, "manage", "default");
            BundlePage(bundles, "manage", "financial");
            BundlePage(bundles, "manage", "index");
            BundlePage(bundles, "messages", "alerts");
            BundlePage(bundles, "messages", "all");
            BundlePage(bundles, "messages", "chat");
            BundlePage(bundles, "messages", "chats");
            BundlePage(bundles, "messages", "index");
            BundlePage(bundles, "post", "detail");
            BundlePage(bundles, "post", "edit");
            BundlePage(bundles, "post", "newpost");
            BundlePage(bundles, "post", "postnotfound");
            BundlePage(bundles, "subscription", "unsubscribe");
            BundlePage(bundles, "tag", "detail");
            BundlePage(bundles, "tag", "index");
            BundlePage(bundles, "user", "achievements");
            BundlePage(bundles, "user", "index");

            /* Tarteaucitron */
            bundles.Add(new ScriptBundle("~/bundles/tarteaucitron").Include(
                        "~/node_modules/tarteaucitronjs/tarteaucitron.js"));

            // Social Styles
            bundles.Add(new StyleBundle("~/Content/style/textspinners").Include(
                      "~/Content/spinners.css"));

            // Social Styles
            bundles.Add(new StyleBundle("~/Content/style/social").Include(
                      "~/Content/bootstrap-social.css"));

            // Needed for some fixes in dependancies
            BundleTable.EnableOptimizations = false;// false;
        }

        private static void BundlePage(BundleCollection bundles, string controller, string page)
        {
            bundles.Add(new Bundle("~/bundles/" + controller + "/" + page).Include(
                        "~/Scripts/dist/" + controller + "/" + page+".js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/" + controller + "/" + page + "/css").Include(
                        "~/Scripts/dist/" + controller + "/" + page + ".css")
                        .WithLastModifiedToken());
        }
    }
}