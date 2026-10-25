using Goke.Core.Authentication;
using Goke.Core.Services;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace GokeWebServer.Services
{
    public interface IHubClientFactory
    {
        Task<HubClientService> CreateWithCookieAsync(string hubPath);
        Task<HubClientService> CreateWithAssessTokenAsync(string hubPath);
    }

    public class HubClientFactory(
        BackendApiEndpoints backendApiEndpoints, 
        IAccessTokenProvider accessTokenProvider, 
        ICookieProvider cookieProvider): IHubClientFactory
    {


        public async Task<HubClientService> CreateWithCookieAsync(
        string hubPath)
        {
            var service = new HubClientService();

            await service.InitializeRealtimeAsync(
                hubUri: CreateHubUri(hubPath),
                cookies: await CreateAuthCookies());

            return service;
        }
        public async Task<HubClientService> CreateWithAssessTokenAsync(
        string hubPath)
        {
            var service = new HubClientService();

            await service.InitializeRealtimeAsync(
                hubUri: CreateHubUri(hubPath),
                accessTokenProvider: GetAccessTokenAsync
                );

            return service;
        }

        private Uri CreateHubUri(string relativePath) => new(backendApiEndpoints.BaseUri, relativePath);

        private Task<string?> GetAccessTokenAsync() => accessTokenProvider.GetAccessTokenAsync();

        private Task<CookieContainer?> CreateAuthCookies() => cookieProvider.CreateAuthCookies();
    }
}
