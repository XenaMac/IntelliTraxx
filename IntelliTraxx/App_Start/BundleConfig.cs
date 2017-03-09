using System.Web;
using System.Web.Optimization;

namespace IntelliTraxx
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.easing.1.3.js",
                        "~/Scripts/jquery-ui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/toastr.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/jquery.tabSlideOut.js",
                      "~/Scripts/jsPanel-3.5.0/source/jquery.jspanel-compiled.min.js",
                      "~/Scripts/bootstrap-table.js",
                      "~/Scripts/bootstrap-table-export.min.js",
                      "~/Scripts/tableExport.js",
                      "~/Scripts/jquery.datetimepicker.full.min.js",
                      "~/Scripts/bootstrap-toggle.min.js"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
                      "~/Scripts/Highcharts/highcharts.js",
                      "~/Scripts/Highcharts/highcharts-3d.js",
                      "~/Scripts/Highcharts/highcharts-more.js",
                      "~/Scripts/Highcharts/modules/drilldown.js",
                      "~/Scripts/Highcharts/modules/solid-gauge.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/logoMenu").Include(
                     "~/Content/Logo.css"
                     ));

            bundles.Add(new ScriptBundle("~/bundles/logoMenu").Include(
                     "~/Scripts/logoMenu.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/Site.css",
                      "~/Content/glyphicons.css",
                      "~/Content/glyphicons-bootstrap.css",
                      "~/Content/toastr.css",
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-table.css",
                      "~/Content/jquery.datetimepicker.min.css",
                      "~/Scripts/jsPanel-3.5.0/source/jquery.jspanel.min.css",
                      "~/Content/bootstrap-toggle.min.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
              "~/Content/themes/base/core.css",
              "~/Content/themes/base/resizable.css",
              "~/Content/themes/base/selectable.css",
              "~/Content/themes/base/accordion.css",
              "~/Content/themes/base/autocomplete.css",
              "~/Content/themes/base/button.css",
              "~/Content/themes/base/dialog.css",
              "~/Content/themes/base/slider.css",
              "~/Content/themes/base/tabs.css",
              "~/Content/themes/base/datepicker.css",
              "~/Content/themes/base/progressbar.css",
              "~/Content/themes/base/theme.css"
            ));
        }
    }
}
