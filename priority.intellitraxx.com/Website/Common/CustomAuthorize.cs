using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Base_AVL.Common
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // The user is not authenticated
                base.HandleUnauthorizedRequest(filterContext);
            }
            else if (!this.Roles.Split(',').Any(filterContext.HttpContext.User.IsInRole))
            {
                // The user is not in any of the listed roles => 
                // show the unauthorized view
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/_Unauthorized.cshtml"
                };
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

            if (!skipAuthorization)
            {
                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    // The user is not authenticated
                    base.HandleUnauthorizedRequest(filterContext);
                }
                else
                {
                    if (this.Roles.Any())
                    {
                        bool authed = false;

                        //get current identity and claims
                        var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                        var roles = identity.Claims.Where(c => c.Type == "Roles").Select(c => c.Value).SingleOrDefault();
                        var companies = identity.Claims.Where(c => c.Type == "Companies").Select(c => c.Value).SingleOrDefault();
                        string[] RolesNeeded = this.Roles.Split(',');

                        foreach (string r in RolesNeeded)
                        {
                            if (roles.Contains(r))
                            {
                                authed = true;
                            }
                        }

                        if (!authed)
                        {
                            // The user is not in any of the listed roles => 
                            // show the unauthorized view
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
}