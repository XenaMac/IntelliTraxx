using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;

namespace IntelliTraxx.Common
{   
    public class MobileBaseController : Controller
    {
        public string GetUserEmail()
        {
            return GetUserClaim(ClaimTypes.NameIdentifier);
        }

        public string GetUserName()
        {
            return GetUserClaim(ClaimTypes.Name);
        }

        public string GetUserId()
        {
            return GetUserClaim(ClaimTypes.Sid);
        }

        public string GetUserPhone()
        {
            return GetUserClaim(ClaimTypes.HomePhone);
        }

        public string GetUserRoles()
        {
            return GetUserClaim("Roles");
        }

        public string GetUserCompanies()
        {
            return GetUserClaim("Companies");
        }

        public string GetUserClaim(string claimType)
        {
            var identity = (ClaimsIdentity)HttpContext.User.Identity;
            var claims = identity.Claims;
            return claims.FirstOrDefault(p => p.Type == claimType)?.Value;
        }
    }
}