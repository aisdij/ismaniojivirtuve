using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Project.Frontend.Services;

namespace Project.Frontend.Website.Services
{
    public class WebsiteHttpClient : IWebsiteHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationService _authenticationService;
        private readonly NavigationManager _navigationManager;

        public WebsiteHttpClient(HttpClient httpClient, IAuthenticationService authenticationService, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _authenticationService = authenticationService;
            _navigationManager = navigationManager;
        }

        public async Task<TResponse> DoPostAsync<TRequest, TResponse>(TRequest request, string relativeUri)
        {
            var userInfo = await _authenticationService.GetUserInfo();
            var stringContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userInfo.JwtToken);

            var response = await _httpClient.PostAsync(relativeUri, stringContent);
            if ((int)response.StatusCode == 401)
            {
                await _authenticationService.LogoutAsync();
                _navigationManager.NavigateTo("/login");
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadFromJsonAsync<TResponse>();
            if (responseContent == null)
                throw new Exception("No Response Returned");

            return responseContent;
        }

        public async Task DoPostAsync<TRequest>(TRequest request, string relativeUri)
        {
            var userInfo = await _authenticationService.GetUserInfo();
            var stringContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userInfo.JwtToken);

            var response = await _httpClient.PostAsync(relativeUri, stringContent);
            if ((int)response.StatusCode == 401)
            {
                await _authenticationService.LogoutAsync();
                _navigationManager.NavigateTo("/login");
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task<TResponse> DoGetAsync<TResponse>(string relativeUri)
        {
            var userInfo = await _authenticationService.GetUserInfo();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userInfo.JwtToken);

            var response = await _httpClient.GetAsync(relativeUri);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadFromJsonAsync<TResponse>();

            if (responseContent == null)
                throw new Exception("No Response Returned");

            return responseContent;
        }

        public async Task<bool> DeletePostAsync(string postId)
        {
            var userInfo = await _authenticationService.GetUserInfo();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userInfo.JwtToken);
            var response = await _httpClient.DeleteAsync($"api/post/delete/{postId}");
            return response.IsSuccessStatusCode;
        }
    }
}