﻿using System.IO;
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

    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Shared scripts
            //bundles.Add(new ScriptBundle("~/bundles/shared").Include(
            //            //"~/Scripts/main.js",                                    // Custom for all
            //            //"~/Scripts/Utility/zr-loadmore.js",                     // Infinite scroll
            //            //"~/Scripts/Posts/quotable.js",                          // For highlight and quote functionality
            //            //"~/Scripts/Posts/post-ui.js",                           // For functions related to posts (NSFW, etc.)
            //            //"~/Scripts/Posts/post-initialize.js",                   // Does any work needed for posts when loaded
            //            "~/Scripts/Utility/clipboard-element.js"               // For copy to clipboard
            //            //"~/Scripts/Lightning/vote-payments-ui.js",              // Related to the user interface for vote LN payments
            //            //"~/Scripts/Lightning/account-payments-ui.js",           // Related to the user interface for deposit/withdraw
            //            //"~/Scripts/Lightning/payments-scan.js"                  // For scanner interface
            //            ).WithLastModifiedToken());

            // This script is imported after the auto-generated signalr hub.
            bundles.Add(new ScriptBundle("~/bundles/realtime").Include(
                        "~/Scripts/Realtime/signalr-initialize.js")
                        .WithLastModifiedToken());

            #region partial scripts

            // Partial script - summary statistics
            bundles.Add(new ScriptBundle("~/bundles/partial/summaryStatistics").Include(
                        "~/Scripts/Partial/summaryStatistics.js")
                        .WithLastModifiedToken());

            // Partial script - top navbar
            bundles.Add(new ScriptBundle("~/bundles/partial/topnavbar").Include(
                        "~/Scripts/Partial/topnavbar.js")
                        .WithLastModifiedToken());

            // Partial script - vote modal
            bundles.Add(new ScriptBundle("~/bundles/partial/vote").Include(
                        "~/Scripts/Partial/vote.js")
                        .WithLastModifiedToken());

            // Partial script - group edit tags
            //bundles.Add(new ScriptBundle("~/bundles/partial/group/tags").Include(
            //            "~/Scripts/Groups/tags.js")
            //            .WithLastModifiedToken());

            // Partial script - group admin bar
            bundles.Add(new ScriptBundle("~/bundles/partial/group/adminbar").Include(
                        "~/Scripts/Groups/adminbar.js")
                        .WithLastModifiedToken());

            // Partial script - group user Roles
            //bundles.Add(new ScriptBundle("~/bundles/partial/group/userRoles").Include(
            //            "~/Scripts/Groups/userRoles.js")
            //            .WithLastModifiedToken());

            // Partial script - group edit icon
            bundles.Add(new ScriptBundle("~/bundles/partial/group/editIcon").Include(
                        "~/Scripts/Groups/editIcon.js")
                        .WithLastModifiedToken());

            // Partial script - vote modal
            bundles.Add(new ScriptBundle("~/bundles/partial/messageCompose").Include(
                        "~/Scripts/Partial/messageCompose.js")
                        .WithLastModifiedToken());

            // Partial scripts - manage
            bundles.Add(new ScriptBundle("~/bundles/manage/partial/updateAlias").Include(
                        "~/Scripts/Manage/updateAlias.js")
                        .WithLastModifiedToken());

            #endregion

            BundlePage(bundles, "account", "login");
            BundlePage(bundles, "admin", "achievements");
            BundlePage(bundles, "admin", "audit");
            BundlePage(bundles, "admin", "icons");
            BundlePage(bundles, "admin", "index");
            BundlePage(bundles, "admin", "jobs");
            BundlePage(bundles, "admin", "lightning");
            BundlePage(bundles, "admin", "users");
            BundlePage(bundles, "group", "detail");
            BundlePage(bundles, "group", "index");
            BundlePage(bundles, "group", "members");
            BundlePage(bundles, "group", "new");
            BundlePage(bundles, "home", "about");
            BundlePage(bundles, "home", "faq");
            BundlePage(bundles, "home", "install");
            BundlePage(bundles, "home", "index");
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
            BundlePage(bundles, "user", "achievements");
            BundlePage(bundles, "user", "index");

            //// User/{username}
            //bundles.Add(new ScriptBundle("~/bundles/users/index").Include(
            //            "~/Scripts/Users/index.js",
            //            "~/Scripts/dist/user_index.js")
            //            .WithLastModifiedToken());

            //// Post/NewPost & Edit scripts
            //bundles.Add(new ScriptBundle("~/bundles/post/edit").Include(
            //            "~/Scripts/dist/post_edit.js",
            //            "~/Scripts/Posts/post-editor.js")                       // For the post editing
            //            .WithLastModifiedToken());

            /* Datatables */
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/node_modules/datatables.net/js/jquery.dataTables.min.js",
                        "~/node_modules/datatables.net-bs4/js/dataTables.bootstrap4.min.js",
                        "~/node_modules/datatables.net-scroller-bs4/js/scroller.bootstrap4.min.js"));

            bundles.Add(new StyleBundle("~/bundles/css/datatables").Include(
                      "~/node_modules/datatables.net-bs4/css/dataTables.bootstrap4.min.css",
                      "~/node_modules/datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/node_modules/popper.js/dist/umd/popper.min.js",
                        "~/node_modules/bootstrap/dist/js/bootstrap.min.js",
                        //"~/Scripts/bootstrap-tour.min.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/font-awesome/css").Include(
                      "~/node_modules/font-awesome/css/font-awesome.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                      "~/node_modules/bootstrap/dist/css/bootstrap.min.css"));

            //bundles.Add(new StyleBundle("~/Content/bootstrap-tour").Include(
            //          "~/Content/bootstrap-tour.min.css"));

            bundles.Add(new StyleBundle("~/Content/css-dark").Include(
                      "~/Content/Site_dark.css")
                      .WithLastModifiedToken());

            // jsSocials - Social Shares
            bundles.Add(new ScriptBundle("~/bundles/jssocials").Include(
                        "~/node_modules/jssocials/dist/jssocials.min.js"));

            bundles.Add(new StyleBundle("~/Content/style/jssocials").Include(
                      "~/node_modules/jssocials/dist/jssocials.css",
                      "~/node_modules/jssocials/dist/jssocials-theme-flat.css"));

            // Social Styles
            bundles.Add(new StyleBundle("~/Content/style/textspinners").Include(
                      "~/Content/spinners.css"));

            // Social Styles
            bundles.Add(new StyleBundle("~/Content/style/social").Include(
                      "~/Content/bootstrap-social.css"));

            // SlimScroll
            bundles.Add(new ScriptBundle("~/plugins/slimScroll").Include(
                      "~/node_modules/jquery-slimscroll/jquery.slimscroll.min.js"));

            // summernote styles
            bundles.Add(new StyleBundle("~/plugins/summernoteStyles").Include(
                      "~/node_modules/summernote/dist/summernote.css",
                      "~/node_modules/summernote/dist/summernote-bs4.css")
                      .WithLastModifiedToken());

            // summernote 
            bundles.Add(new ScriptBundle("~/plugins/summernote").Include(
                      "~/Scripts/summernote/dist/summernote-bs4.js",
                      "~/Scripts/summernote-video-attributes.js")
                      .WithLastModifiedToken());

            // selectize 
            bundles.Add(new ScriptBundle("~/plugins/selectize").Include(
                      "~/node_modules/selectize/dist/js/standalone/selectize.min.js")
                      .WithLastModifiedToken());

            // selectize 
            bundles.Add(new StyleBundle("~/Content/plugins/selectize").Include(
                      "~/node_modules/selectize/dist/css/selectize.css",
                      "~/node_modules/selectize-bootstrap4-theme/dist/css/selectize.bootstrap4.css")
                      .WithLastModifiedToken());

            // Flot chart
            bundles.Add(new ScriptBundle("~/plugins/flot").Include(
                      "~/node_modules/jquery.flot/jquery.flot.js",
                      "~/node_modules/jquery.flot.tooltip/js/jquery.flot.tooltip.min.js",
                      "~/node_modules/jquery.flot/jquery.flot.resize.js",
                      "~/node_modules/jquery.flot/jquery.flot.pie.js",
                      "~/node_modules/jquery.flot/jquery.flot.time.js"));
            /*"~/node_modules/jquery.flot/jquery.flot.spline.js"));*/

            // Needed for some fixes in dependancies
            BundleTable.EnableOptimizations = false;// false;
        }

        private static void BundlePage(BundleCollection bundles, string controller, string page)
        {
            bundles.Add(new ScriptBundle("~/bundles/" + controller + "/" + page).Include(
                        "~/Scripts/dist/" + controller + "/" + page+".js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/" + controller + "/" + page + "/css").Include(
                        "~/Scripts/dist/" + controller + "/" + page + ".css")
                        .WithLastModifiedToken());
        }
    }
}
