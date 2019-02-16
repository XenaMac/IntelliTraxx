using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using IntelliTraxx.Shared.Contracts;
using IntelliTraxx.Shared.Identity;

namespace IntelliTraxx.Shared.Concrete
{
    public class UserSecurity : IUserSecurity
    {
        private readonly ClaimsIdentity _identity;


        public UserSecurity()
        {
            _identity = (ClaimsIdentity)HttpContext.Current.User.Identity;
        }

        public UserSecurityContext GetCurrentUserSecurityContext()
        {
            return new UserSecurityContext
            {
                Id = CurrentUserId(),
                Email = CurrentUserEmail(),
                FullName = CurrentUserName()
            };
        }

        private Guid CurrentUserId()
        {
            var userId = Guid.Parse(GetClaim(ClaimTypes.Sid));
            return userId;
        }

        private string CurrentUserEmail()
        {
            return GetClaim(ClaimTypes.Email);
        }

        private string CurrentUserName()
        {
            return GetClaim(ClaimTypes.Name);
        }

        private string GetClaim(string claimType)
        {
            var claims = _identity.Claims;
            return claims.FirstOrDefault(p => p.Type == claimType)?.Value;
        }
    }
}
