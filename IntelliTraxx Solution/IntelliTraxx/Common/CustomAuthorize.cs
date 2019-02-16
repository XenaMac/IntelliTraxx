using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;

namespace IntelliTraxx.Common
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                base.HandleUnauthorizedRequest(filterContext);
            else if (!Roles.Split(',').Any(filterContext.HttpContext.User.IsInRole))
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/_Unauthorized.cshtml"
                };
            else
                base.HandleUnauthorizedRequest(filterContext);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) ||
                                    filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(
                                        typeof(AllowAnonymousAttribute), true);

            if (!skipAuthorization)
            {
                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    // The user is not authenticated
                    base.HandleUnauthorizedRequest(filterContext);
                }
                else
                {
                    if (Roles.Any())
                    {
                        var authed = false;

                        //get current identity and claims
                        var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
                        var roles = identity.Claims.Where(c => c.Type == "Roles").Select(c => c.Value)
                            .SingleOrDefault();
                        var companies = identity.Claims.Where(c => c.Type == "Companies").Select(c => c.Value)
                            .SingleOrDefault();
                        var RolesNeeded = Roles.Split(',');

                        foreach (var r in RolesNeeded)
                            if (roles.Contains(r))
                                authed = true;

                        if (!authed)
                            filterContext.Result = new ViewResult
                            {
                                ViewName = "~/Views/Shared/_Unauthorized.cshtml"
                            };
                    }
                }
            }
        }
    }
}