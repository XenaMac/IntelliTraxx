using System.Web;
using System.Web.Optimization;

namespace Base_AVL
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui-{version}.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*",
                        "~/Scripts/Perspective/modernizr.custom.25376.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                   "~/Scripts/knockout-{version}.js",
                   "~/Scripts/moment.js",
                   "~/Scripts/toastr.js",
                   "~/Scripts/Perspective/classie.js",
                   "~/Scripts/Perspective/menu.js"
                   ));

            bundles.Add(new ScriptBundle("~/bundles/appAngular").Include(
                  "~/Scripts/angular.min.js",
                  "~/Scripts/angular-sanitize.min.js",
                  "~/Scripts/angular-route.min.js",
                  "~/Scripts/angular-ui/ui-bootstrap.js",
                  "~/Scripts/angular-ui/ui-bootstrap-tpls.js"
                  ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/Site.css",
                      "~/Content/font-awesome.css",
                      "~/Content/glyphicons.css",
                      "~/Content/glyphicons-bootstrap.css",
                      "~/Content/toastr.css",
                      "~/Content/Perspective/normalize.css",
                      "~/Content/Perspective/component.css",
                      "~/Content/bootstrap.css"
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

            BundleTable.EnableOptimizations = false;
        }
    }
}
