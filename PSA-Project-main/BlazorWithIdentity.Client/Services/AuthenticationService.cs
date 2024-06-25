using Blazored.SessionStorage;
using Project.Shared.RequestModels;
using Project.Shared.ResponseModels;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Project.Frontend.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ISessionStorageService _sessionStorage;

        private const string USER_INFO = nameof(USER_INFO);
        private LoginResponse? _authenticationCache;

        public AuthenticationService(HttpClient httpClient, ISessionStorageService sessionStorage)
        {
            _httpClient = httpClient;
            _sessionStorage = sessionStorage;
        }

        public async ValueTask<LoginResponse> GetUserInfo()
        {
            if (_authenticationCache == null || string.IsNullOrWhiteSpace(_authenticationCache.JwtToken))
                _authenticationCache = await _sessionStorage.GetItemAsync<LoginResponse>(USER_INFO);

            return _authenticationCache;
        }

        public async Task LogoutAsync() 
        {
            await _sessionStorage.RemoveItemAsync(USER_INFO);
            _authenticationCache = null;
        }

        public async Task LoginAsync(LoginRequest request)
        {
            var stringContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/authentication/login", stringContent);
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (responseContent?.JwtToken == null)
                throw new Exception("No Response Returned");

            await _sessionStorage.SetItemAsync(USER_INFO, responseContent);
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var stringContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/authentication/register", stringContent);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (responseContent == null)
                throw new Exception("No Response Returned");

            await _sessionStorage.SetItemAsync(USER_INFO, responseContent);
        }

        public async Task<string> GetClaimValue(string claim)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken((await GetUserInfo()).JwtToken);

            if (jwtToken is null)
                return string.Empty;

            foreach (Claim jwtClaim in jwtToken.Claims)
                if (jwtClaim.Type == claim)
                    return jwtClaim.Value;

            return string.Empty;
        }
    }
}
