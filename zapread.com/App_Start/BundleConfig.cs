using System.IO;
using System.Web;
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
            // Shared view plugins together
            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                        "~/Scripts/summernote/dist/summernote-bs4.js",          // Summernote WYSIWYG editor
                        "~/Scripts/summernote-video-attributes.js",             // Summernote plugin
                        "~/node_modules/sweetalert/dist/sweetalert.min.js",     // Sweet Alert
                        "~/node_modules/jssocials/dist/jssocials.min.js",       // jsSocials - Social Shares
                        "~/node_modules/toastr/build/toastr.min.js",            // toastr notification 
                        "~/node_modules/moment/min/moment-with-locales.min.js"  // Time formatting
                        ));

            // Shared scripts
            bundles.Add(new ScriptBundle("~/bundles/shared").Include(
                        "~/Scripts/main.js",                                    // Custom for all
                        "~/Scripts/Posts/quotable.js",                          // For highlight and quote functionality
                        "~/Scripts/Posts/readmore.js",                          // Fade out posts and show read more button
                        "~/Scripts/Posts/post-functions.js",                    // For functions related to posts (NSFW, etc.)
                        "~/Scripts/Posts/post-ui.js",                           // For functions related to posts (NSFW, etc.)
                        "~/Scripts/Posts/post-initialize.js",                   // Does any work needed for posts when loaded
                        "~/Scripts/Utility/clipboard-element.js",               // For copy to clipboard
                        "~/Scripts/Lightning/vote-payments-ui.js",              // Related to the user interface for vote LN payments
                        "~/Scripts/Lightning/account-payments-ui.js",           // Related to the user interface for deposit/withdraw
                        "~/Scripts/Lightning/payments-scan.js"                  // For scanner interface
                        ).WithLastModifiedToken());

            // Manage/Index scripts
            bundles.Add(new ScriptBundle("~/bundles/manage/index").Include(
                        "~/node_modules/dropzone/dist/min/dropzone.min.js",
                        //"~/node_modules/dropzone/dist/dropzone.js",
                        "~/node_modules/bootstrap-chosen/dist/chosen.jquery-1.4.2/chosen.jquery.min.js",
                        "~/Scripts/Manage/index.js")
                        .WithLastModifiedToken());

            // Post/NewPost scripts
            bundles.Add(new ScriptBundle("~/bundles/post/edit").Include(
                        "~/Scripts/Posts/post-editor.js")                       // For the post editing
                        .WithLastModifiedToken());

            bundles.Add(new ScriptBundle("~/bundles/DetailPost").Include(
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

            // Jquery ui
            bundles.Add(new ScriptBundle("~/Content/plugins/css/jquery-ui").Include(
                        "~/node_modules/jquery-ui-dist/jquery-ui.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/truncate").Include(
                        "~/Scripts/jquery.truncate.js"));

            // Jquery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/node_modules/jquery/dist/jquery.js",
                        "~/node_modules/jquery-ajax-unobtrusive/jquery.unobtrusive-ajax.min.js",
                        "~/node_modules/jquery-ui-dist/jquery-ui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            /* Datatables */
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/node_modules/datatables.net/js/jquery.dataTables.min.js",
                        "~/node_modules/datatables.net-bs4/js/dataTables.bootstrap4.min.js",
                        "~/node_modules/datatables.net-scroller-bs4/js/scroller.bootstrap4.min.js"));

            bundles.Add(new StyleBundle("~/bundles/css/datatables").Include(
                      "~/node_modules/datatables.net-bs4/css/dataTables.bootstrap4.min.css",
                      "~/node_modules/datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/node_modules/popper.js/dist/umd/popper.min.js",
                        "~/node_modules/bootstrap/dist/js/bootstrap.min.js",
                        "~/Scripts/bootstrap-tour.min.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/font-awesome/css").Include(
                      "~/node_modules/font-awesome/css/font-awesome.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                      "~/node_modules/bootstrap/dist/css/bootstrap.min.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap-tour").Include(
                      "~/Content/bootstrap-tour.min.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
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

            // toastr notification 
            bundles.Add(new ScriptBundle("~/plugins/toastr").Include(
                      "~/node_modules/toastr/build/toastr.min.js"));

            // toastr notification styles
            bundles.Add(new StyleBundle("~/Content/style/toastr").Include(
                      "~/node_modules/toastr/build/toastr.min.css"));

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

            // summernote 
            bundles.Add(new ScriptBundle("~/plugins/moment").Include(
                      "~/node_modules/moment/min/moment-with-locales.min.js"));

            // selectize 
            bundles.Add(new ScriptBundle("~/plugins/selectize").Include(
                      "~/node_modules/selectizebootstrap4/dist/js/selectize/standalone/selectize.min.js"));

            // selectize 
            bundles.Add(new StyleBundle("~/Content/plugins/selectize").Include(
                      "~/node_modules/selectizebootstrap4/dist/css/selectize.bootstrap4.css"));

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
