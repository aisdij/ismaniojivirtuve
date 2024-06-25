using Project.Shared.RequestModels;
using Project.Shared.ResponseModels;
using System.Security.Claims;

namespace Project.Frontend.Services;

public interface IAuthenticationService
{
    Task LoginAsync(LoginRequest loginParameters);
    
    Task RegisterAsync(RegisterRequest registerParameters);
    
    Task LogoutAsync();

    ValueTask<LoginResponse> GetUserInfo();

    public Task<string> GetClaimValue(string claim);
}
