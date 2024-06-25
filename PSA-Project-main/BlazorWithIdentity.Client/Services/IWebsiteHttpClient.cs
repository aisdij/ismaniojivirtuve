using Project.Shared.ResponseModels;

namespace Project.Frontend.Website.Services
{
    public interface IWebsiteHttpClient
    {
        public Task<TResponse> DoPostAsync<TRequest, TResponse>(TRequest request, string relativeUri);

        public Task DoPostAsync<TRequest>(TRequest request, string relativeUri);

        public Task<TResponse> DoGetAsync<TResponse>(string relativeUri);

        Task<bool> DeletePostAsync(string postId);
    }
}