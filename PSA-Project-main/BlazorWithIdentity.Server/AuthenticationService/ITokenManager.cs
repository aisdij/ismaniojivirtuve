using System.Security.Claims;

namespace Project.Backend.Server.AuthenticationService
{
    public interface ITokenManager
    {
        public string GenerateToken(List<Claim> claims);
    }
}
