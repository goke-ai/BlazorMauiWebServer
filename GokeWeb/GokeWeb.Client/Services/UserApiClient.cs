using Goke.Core.Authentication;
using Goke.Core.Chat;
using Goke.Core.Services;
using GokeSharedCore.ChatApp;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GokeWeb.Client.Services
{
    public class UserApiClient(
        IHttpClientFactory httpClientFactory, 
        IAccessTokenProvider accessTokenProvider) : IUserService
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient(BackendApiEndpoints.ClientName);

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var accessToken = await accessTokenProvider.GetAccessTokenAsync();

                using var request = new HttpRequestMessage(HttpMethod.Get, "api/chat/online-users");

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                using var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<User>>() ?? [];
            }
            catch
            {
                return [];
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            var accessToken = await accessTokenProvider.GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/chat/users/{userId}");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            using var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task<List<User>> GetOnlineUsersAsync()
        {
            try
            {
                var accessToken = await accessTokenProvider.GetAccessTokenAsync();

                using var request = new HttpRequestMessage(HttpMethod.Get, "api/chat/online-users");

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                using var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<User>>() ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}