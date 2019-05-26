using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NFCLogin
{
    class JWTUtils
    {

        /// <summary>
        /// Gets the username from a JSON Web Token.
        /// </summary>
        /// <param name="authToken">The JSON Web Token to get a username from.</param>
        /// <returns></returns>
        public static String GetUserNameFromJWT(string authToken)
        {
            IPrincipal principal = null;
            ValidateToken(authToken, out principal);

            var identity = principal.Identity as ClaimsIdentity;

            var usernameClaim = identity.FindFirst("username");
            var username = usernameClaim?.Value;

            Debug.WriteLine(username);

            return username;
        }

        /// <summary>
        /// Validate a JSON Web Token.
        /// </summary>
        /// <param name="authToken">The JSON web token to validate.</param>
        /// <param name="principal">A IPrincipal object that will be passed out from the function.</param>
        /// <returns></returns>
        private static bool ValidateToken(string authToken, out IPrincipal principal)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            SecurityToken validatedToken;
            principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
            
            return true;
        }

        /// <summary>
        /// Creates validation parameters to validate JSON Web Tokens.
        /// </summary>
        /// <returns>JSON Web Token validation parameters.</returns>
        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("CDSuDNSIaERroYI1Q3fqcR8mcFqaienU")) // The same key as the one that generate the token
            };
        }
    }
}
