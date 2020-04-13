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

    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Shared scripts
            bundles.Add(new ScriptBundle("~/bundles/shared").Include(
                        //"~/Scripts/main.js",                                    // Custom for all
                        "~/Scripts/Utility/zr-loadmore.js",                     // Infinite scroll
                        //"~/Scripts/Posts/quotable.js",                          // For highlight and quote functionality
                        "~/Scripts/Posts/readmore.js",                          // Fade out posts and show read more button
                        "~/Scripts/Posts/post-functions.js",                    // For functions related to posts (NSFW, etc.)
                        "~/Scripts/Posts/post-ui.js",                           // For functions related to posts (NSFW, etc.)
                        "~/Scripts/Posts/post-initialize.js",                   // Does any work needed for posts when loaded
                        "~/Scripts/Utility/clipboard-element.js",               // For copy to clipboard
                        "~/Scripts/Lightning/vote-payments-ui.js",              // Related to the user interface for vote LN payments
                        "~/Scripts/Lightning/account-payments-ui.js",           // Related to the user interface for deposit/withdraw
                        "~/Scripts/Lightning/payments-scan.js"                  // For scanner interface
                        ).WithLastModifiedToken());

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
            bundles.Add(new ScriptBundle("~/bundles/partial/group/tags").Include(
                        "~/Scripts/Groups/tags.js")
                        .WithLastModifiedToken());

            // Partial script - group admin bar
            bundles.Add(new ScriptBundle("~/bundles/partial/group/adminbar").Include(
                        "~/Scripts/Groups/adminbar.js")
                        .WithLastModifiedToken());

            // Partial script - group user Roles
            bundles.Add(new ScriptBundle("~/bundles/partial/group/userRoles").Include(
                        "~/Scripts/Groups/userRoles.js")
                        .WithLastModifiedToken());

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

            #region account

            // Account/Login scripts
            bundles.Add(new ScriptBundle("~/bundles/account/login").Include(
                        "~/Scripts/dist/account_login.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/account/login/css").Include(
                          "~/Scripts/dist/account_login.css")
                          .WithLastModifiedToken());
            #endregion

            #region admin

            // Admin/Achievements scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/achievements").Include(
                        "~/Scripts/dist/admin_achievements.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/admin/achievements/css").Include(
                          "~/Scripts/dist/admin_achievements.css")
                          .WithLastModifiedToken());

            // Admin/Audit scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/audit").Include(
                        "~/Scripts/dist/admin_audit.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/admin/audit/css").Include(
                          "~/Scripts/dist/admin_audit.css")
                          .WithLastModifiedToken());

            // Admin/Icons scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/icons").Include(
                        "~/Scripts/dist/admin_icons.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/admin/icons/css").Include(
                          "~/Scripts/dist/admin_icons.css")
                          .WithLastModifiedToken());

            // Admin/ scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/index").Include(
                        "~/Scripts/dist/admin_index.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/admin/index/css").Include(
                          "~/Scripts/dist/admin_index.css")
                          .WithLastModifiedToken());

            // Admin/Jobs scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/jobs").Include(
                        "~/Scripts/dist/admin_jobs.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/admin/jobs/css").Include(
                          "~/Scripts/dist/admin_jobs.css")
                          .WithLastModifiedToken());

            // Admin/Lightning scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/lightning").Include(
                        "~/Scripts/dist/admin_lightning.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/admin/lightning/css").Include(
                          "~/Scripts/dist/admin_lightning.css")
                          .WithLastModifiedToken());

            // Admin/Users scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/users").Include(
                        "~/Scripts/dist/admin_users.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/admin/users/css").Include(
                          "~/Scripts/dist/admin_users.css")
                          .WithLastModifiedToken());

            #endregion

            #region group

            // Group/GroupDetail scripts
            bundles.Add(new ScriptBundle("~/bundles/group/detail").Include(
                        "~/Scripts/dist/group_detail.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/group/detail/css").Include(
                        "~/Scripts/dist/group_detail.css")
                        .WithLastModifiedToken());

            // Group/Index scripts
            bundles.Add(new ScriptBundle("~/bundles/group/index").Include(
                        "~/Scripts/dist/group_index.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/group/index/css").Include(
                        "~/Scripts/dist/group_index.css")
                        .WithLastModifiedToken());

            // Group/Members scripts
            bundles.Add(new ScriptBundle("~/bundles/group/members").Include(
                        "~/Scripts/dist/group_members.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/group/members/css").Include(
                        "~/Scripts/dist/group_members.css")
                        .WithLastModifiedToken());

            // Group/New scripts
            bundles.Add(new ScriptBundle("~/bundles/group/new").Include(
                        "~/Scripts/Groups/new.js")
                        .WithLastModifiedToken());

            #endregion

            #region manage scripts

            // Manage view default scripts
            bundles.Add(new ScriptBundle("~/bundles/manage/default").Include(
                        "~/Scripts/dist/manage_default.js")
                        .WithLastModifiedToken());
            bundles.Add(new StyleBundle("~/bundles/manage/default/css").Include(
                          "~/Scripts/dist/manage_default.css")
                          .WithLastModifiedToken());

            // Manage/Index scripts
            bundles.Add(new ScriptBundle("~/bundles/manage/index").Include(
                        "~/node_modules/dropzone/dist/min/dropzone.min.js",
                        "~/node_modules/bootstrap-chosen/dist/chosen.jquery-1.4.2/chosen.jquery.min.js",
                        "~/Scripts/Manage/index.js",
                        "~/Scripts/Achievements/achievementhover.js",
                        "~/Scripts/dist/manage_index.js")
                        .WithLastModifiedToken());

            // Manage/Financial scripts
            bundles.Add(new ScriptBundle("~/bundles/manage/financial").Include(
                        "~/Scripts/Manage/financial.js")
                        .WithLastModifiedToken());

            #endregion

            // User/{username}
            bundles.Add(new ScriptBundle("~/bundles/users/index").Include(
                        "~/Scripts/Users/index.js",
                        "~/Scripts/dist/user_index.js")
                        .WithLastModifiedToken());

            // Home/Install scripts
            bundles.Add(new ScriptBundle("~/bundles/admin/install").Include(
                        "~/Scripts/Admin/install.js")
                        .WithLastModifiedToken());

            // Home/Index scripts
            bundles.Add(new ScriptBundle("~/bundles/home/index").Include(
                        "~/Scripts/dist/home_index.js")
                        .WithLastModifiedToken());
            // Post/NewPost & Edit styles
            bundles.Add(new StyleBundle("~/bundles/home/index/css").Include(
                          "~/Scripts/dist/home_index.css")
                          .WithLastModifiedToken());

            #region messages

            // Messages/Index scripts
            bundles.Add(new ScriptBundle("~/bundles/messages/index").Include(
                        "~/Scripts/Messages/index.js")
                        .WithLastModifiedToken());

            // Messages/All scripts
            bundles.Add(new ScriptBundle("~/bundles/messages/all").Include(
                        "~/Scripts/Messages/all.js")
                        .WithLastModifiedToken());

            // Messages/Chat scripts
            bundles.Add(new ScriptBundle("~/bundles/messages/chat").Include(
                        "~/Scripts/Messages/chat-ui.js")
                        .WithLastModifiedToken());

            // Messages/Chats scripts
            bundles.Add(new ScriptBundle("~/bundles/messages/chats").Include(
                        "~/Scripts/Messages/chats.js")
                        .WithLastModifiedToken());

            // Messages/Alerts scripts
            bundles.Add(new ScriptBundle("~/bundles/messages/alerts").Include(
                        "~/Scripts/Messages/alerts.js")
                        .WithLastModifiedToken());

            #endregion

            // Post/NewPost & Edit scripts
            bundles.Add(new ScriptBundle("~/bundles/post/edit").Include(
                        "~/Scripts/dist/post_edit.js",
                        "~/Scripts/Posts/post-editor.js")                       // For the post editing
                        .WithLastModifiedToken());
            // Post/NewPost & Edit styles
            bundles.Add(new StyleBundle("~/bundles/post/edit/css").Include(
                          "~/Scripts/dist/post_edit.css")
                          .WithLastModifiedToken());

            // Post/Detail scripts
            bundles.Add(new ScriptBundle("~/bundles/post/detail").Include(
                        "~/Scripts/Posts/post-detail.js",
                        "~/Scripts/dist/post_detail.js")
                        .WithLastModifiedToken());

            // User scripts
            bundles.Add(new ScriptBundle("~/bundles/users").Include(
                        "~/Scripts/Users/hover.js")
                        .WithLastModifiedToken());

            // User scripts
            bundles.Add(new ScriptBundle("~/bundles/groups").Include(
                        "~/Scripts/Groups/hover.js")
                        .WithLastModifiedToken());

            // Achievement scripts
            bundles.Add(new ScriptBundle("~/bundles/achievements").Include(
                        "~/Scripts/Achievements/achievementhover.js")
                        .WithLastModifiedToken());

            bundles.Add(new ScriptBundle("~/bundles/realtime").Include(
                        "~/Scripts/Realtime/signalr-initialize.js")
                        .WithLastModifiedToken());

            // chosen scripts
            bundles.Add(new ScriptBundle("~/plugins/chosen").Include(
                      "~/node_modules/bootstrap-chosen/dist/chosen.jquery-1.4.2/chosen.jquery.min.js"));

            // chosen styles
            bundles.Add(new StyleBundle("~/Content/plugins/chosen/chosenStyles").Include(
                      "~/node_modules/bootstrap-chosen/bootstrap-chosen.css", new CssRewriteUrlTransform()));

            // dropZone scripts
            bundles.Add(new ScriptBundle("~/plugins/dropZone").Include(
                      "~/node_modules/dropzone/dist/min/dropzone.min.js"));

            // dropZone styles
            bundles.Add(new StyleBundle("~/Content/plugins/dropzone/dropZoneStyles").Include(
                      "~/node_modules/dropzone/dist/min/basic.min.css",
                      "~/node_modules/dropzone/dist/min/dropzone.min.css"));

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

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/style/hover.css",
                      "~/Content/style/roundlinks.css")
                      .WithLastModifiedToken());

            bundles.Add(new StyleBundle("~/Content/css-dark").Include(
                      "~/Content/Site_dark.css")
                      .WithLastModifiedToken());

            // Sweet Alert
            bundles.Add(new ScriptBundle("~/bundles/sweetalert").Include(
                        "~/node_modules/sweetalert/dist/sweetalert.min.js"));

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
            BundleTable.EnableOptimizations = true;// false;
        }
    }
}
