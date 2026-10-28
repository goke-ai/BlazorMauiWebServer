using Goke.Core.Authentication;
using Goke.Core.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GokeSharedCore.ChatApp
{
    public class ChatUserApiClient(
        IHttpClientFactory httpClientFactory, 
        IAccessTokenProvider accessTokenProvider) : IUserService
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient(BackendApiEndpoints.ClientName);

        public async Task<List<UiChatUser>> GetUsersAsync()
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

                return await response.Content.ReadFromJsonAsync<List<UiChatUser>>() ?? [];
            }
            catch
            {
                return [];
            }
        }

        public async Task<UiChatUser?> GetUserByIdAsync(string userId)
        {
            var accessToken = await accessTokenProvider.GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/chat/users/{userId}");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            using var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UiChatUser>();
        }

        public async Task<List<UiChatUser>> GetOnlineUsersAsync()
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

                return await response.Content.ReadFromJsonAsync<List<UiChatUser>>() ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}