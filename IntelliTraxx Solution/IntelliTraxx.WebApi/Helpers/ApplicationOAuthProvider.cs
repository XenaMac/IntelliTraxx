using System.Security.Claims;
using System.Threading.Tasks;
using IntelliTraxx.Shared;
using IntelliTraxx.Shared.TruckService;
using Microsoft.Owin.Security.OAuth;

namespace IntelliTraxx.WebApi.Helpers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {       
        private readonly TruckServiceClient _truckService = new TruckServiceClient();       

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var authManager = new AuthManager();
            var userId = authManager.LogonUser(context.UserName, context.Password);
            var appUser = _truckService.getUserProfile(userId);

            if (appUser != null)
            {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, appUser.UserEmail));
                identity.AddClaim(new Claim(ClaimTypes.Name, $"{appUser.UserFirstName} {appUser.UserLastName}"));
                identity.AddClaim(new Claim(ClaimTypes.Email, appUser.UserEmail));
                identity.AddClaim(new Claim(ClaimTypes.HomePhone, appUser.UserPhone));
                identity.AddClaim(new Claim(ClaimTypes.Sid, appUser.UserID.ToString()));
                context.Validated(identity);
            }
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }
    }
}