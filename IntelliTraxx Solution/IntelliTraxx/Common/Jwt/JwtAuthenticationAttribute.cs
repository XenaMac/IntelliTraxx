using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using IntelliTraxx.Common.Jwt;

namespace IntelliTraxx.Common
{
    public class JwtAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public string Realm { get; set; }

        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;

            if (authorization == null || authorization.Scheme.ToLower() != "bearer")
                return;

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing Jwt Token", request);
                return;
            }

            var token = authorization.Parameter;
            var principal = await AuthenticateJwtToken(token);

            if (principal == null)
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);
            else
                context.Principal = principal;
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            string userEmail;
            string name;
            string email;
            string phone;
            string userId;
            string roleNames;
            string companyNames;

            if (!ValidateToken(token, out userEmail, out name, out email, out phone, out userId, out roleNames, out companyNames)) return Task.FromResult<IPrincipal>(null);

            // based on username to get more information from database in order to build local identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userEmail),
                new Claim(ClaimTypes.Name, name),                   
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.HomePhone, phone),
                new Claim(ClaimTypes.Sid,userId),
                new Claim("Roles", roleNames),
                new Claim("Companies", companyNames)      
            };

            var identity = new ClaimsIdentity(claims, "Jwt");
            IPrincipal user = new ClaimsPrincipal(identity);

            return Task.FromResult(user);
        }

        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter = null;

            if (!string.IsNullOrEmpty(Realm))
                parameter = "realm=\"" + Realm + "\"";

            context.ChallengeWith("Bearer", parameter);
        }

        private static bool ValidateToken(string token, out string userEmail, out string name, out string email,
            out string phone, out string userId, out string roleNames, out string companyNames)
        {
            userEmail = null;
            name = null;
            email = null;
            phone = null;
            userId = null;
            roleNames = null;
            companyNames = null;

            var simplePrinciple = JwtManager.GetPrincipal(token);

            if (simplePrinciple == null)
                return false;

            var identity = simplePrinciple.Identity as ClaimsIdentity;
            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var userEmailClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            var nameClaim = identity.FindFirst(ClaimTypes.Name);
            var emailClaim = identity.FindFirst(ClaimTypes.Email);
            var phoneClaim = identity.FindFirst(ClaimTypes.HomePhone);
            var userIdClaim = identity.FindFirst(ClaimTypes.Sid);
            var roleNamesClaim = identity.FindFirst("Roles");
            var companyNamesClaim = identity.FindFirst("Companies");

            userEmail = userEmailClaim?.Value;
            name = nameClaim?.Value;
            email = emailClaim?.Value;
            phone = phoneClaim?.Value;
            userId = userIdClaim?.Value;
            roleNames = roleNamesClaim?.Value;
            companyNames = companyNamesClaim?.Value;

            return !string.IsNullOrEmpty(userEmail);

            // More validate to check whether username exists in system
        }
    }
}