using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    class JwtUtils
    {
        /// <summary>
        /// Extracts the user from the JWT token 
        /// </summary>
        /// <param name="httpContext">The information of the request</param>
        /// <returns>The user that was associated with the token</returns>
        public static string GetUserFromToken(HttpContext httpContext)
        {
            //Extract the authorization header
            var authorized = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            //Check if token exists in header
            if (string.IsNullOrEmpty(authorized) || !authorized.StartsWith("Bearer"))
            {
                return null;
            }

            //Remove the prefix "Bearer " from the token
            var token = authorized.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();

            //Check if token is valid
            if (!handler.CanReadToken(token))
            {
                return null;
            }

            var jwt = handler.ReadJwtToken(token);
            //Extract the user from the token claim
            var user = jwt.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;

            return user;
        }
    }
}
