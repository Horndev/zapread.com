using System.Web;
using System.Web.Optimization;

namespace zapread.com
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/DetailPost").Include(
                        "~/Scripts/DetailPostPage.js"));

            bundles.Add(new ScriptBundle("~/bundles/truncate").Include(
                        "~/Scripts/jquery.truncate.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/node_modules/jquery/dist/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-unobtrusive").Include(
                        "~/node_modules/jquery-ajax-unobtrusive/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/plugins/jquery-ui").Include(
                        "~/node_modules/jquery-ui-dist/jquery-ui.min.js"));

            bundles.Add(new ScriptBundle("~/Content/plugins/css/jquery-ui").Include(
                        "~/node_modules/jquery-ui-dist/jquery-ui.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            /* Datatables */
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/node_modules/datatables.net/js/jquery.dataTables.min.js",
                        "~/node_modules/datatables.net-bs4/js/dataTables.bootstrap4.min.js",
                        "~/node_modules/datatables.net-scroller-bs4/js/scroller.bootstrap4.min.js"));

            bundles.Add(new StyleBundle("~/bundles/css/datatables").Include(
                      "~/node_modules/datatables.net-bs4/css/dataTables.bootstrap4.min",
                      "~/node_modules/datatables.net-scroller-bs4/css/scroller.bootstrap4.min"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/node_modules/popper.js/dist/umd/popper.min.js",
                        "~/node_modules/bootstrap/dist/js/bootstrap.min.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/font-awesome/css").Include(
                      "~/node_modules/font-awesome/css/font-awesome.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/node_modules/bootstrap/dist/css/bootstrap.min.css",
                      "~/Content/site.css"));

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

            // dropZone styles
            bundles.Add(new StyleBundle("~/Content/plugins/dropzone/dropZoneStyles").Include(
                      "~/node_modules/dropzone/dist/min/basic.min.css",
                      "~/node_modules/dropzone/dist/min/dropzone.min.css"));

            // dropZone 
            bundles.Add(new ScriptBundle("~/plugins/dropZone").Include(
                      "~/node_modules/dropzone/dist/min/dropzone.min.js"));

            // SlimScroll
            bundles.Add(new ScriptBundle("~/plugins/slimScroll").Include(
                      "~/node_modules/jquery-slimscroll/jquery.slimscroll.min.js"));

            // summernote styles
            bundles.Add(new StyleBundle("~/plugins/summernoteStyles").Include(
                      "~/Scripts/summernote/dist/summernote.css",
                      "~/Scripts/summernote/dist/summernote-bs4.css"));

            // summernote 
            bundles.Add(new ScriptBundle("~/plugins/summernote").Include(
                      "~/Scripts/summernote/dist/summernote-bs4.js",
                      "~/Scripts/summernote-video-attributes.js"));

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
            BundleTable.EnableOptimizations = false;

        }
    }
}
