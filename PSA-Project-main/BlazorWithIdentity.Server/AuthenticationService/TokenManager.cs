using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Project.Backend.Server.AuthenticationService
{
    public class TokenManager : ITokenManager
    {
        private static string secret = "my-32-character-ultra-secure-and-ultra-long-secret";

        public string GenerateToken(List<Claim> claims)
        {
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256
                );

            var token = new JwtSecurityToken(
                "Project",
                "Project-Audience",
                claims,
                null,
                DateTime.UtcNow.AddHours(8),
                signingCredentials);

            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }
    }
}
