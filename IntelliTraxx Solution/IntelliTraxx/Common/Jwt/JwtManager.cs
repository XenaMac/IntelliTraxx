using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using IntelliTraxx.TruckService;
using Microsoft.IdentityModel.Tokens;

namespace IntelliTraxx.Common.Jwt
{
    public static class JwtManager
    {
        public static string GenerateToken(User user, string userRoleNames, string userCompanyNames)
        {
            var symmetricKey = Convert.FromBase64String(Constants.JwtSecretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserEmail),
                    new Claim(ClaimTypes.Name, $"{user.UserFirstName} {user.UserLastName}"),                   
                    new Claim(ClaimTypes.Email, user.UserEmail),
                    new Claim(ClaimTypes.HomePhone, user.UserPhone),
                    new Claim(ClaimTypes.Sid, user.UserID.ToString()),
                    new Claim("Roles", userRoleNames),
                    new Claim("Companies", userCompanyNames)
                }),
                Expires = now.AddMinutes(Convert.ToInt32(Constants.JwtExpirationInMinutes)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = Convert.FromBase64String(Constants.JwtSecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }

            catch (Exception ex)
            {
                //Invalid token most likely. Or expired.
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}