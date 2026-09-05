using Goke.Core.Interfaces;
using Goke.Core.Options;
using Goke.Core.Security;
using Goke.Core.Services;
using GokeHyb.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace GokeHyb;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

        //+authentication
        // Load the embedded appsettings.json file
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GokeHyb.appsettings.json")
            ?? throw new InvalidOperationException("Could not find embedded resource 'GokeHyb.appsettings.json'");

        builder.Configuration.AddJsonStream(stream);


        //Register needed elements for authentication:
        // This is the core functionality
        builder.Services.AddAuthorizationCore();

        // Add configuration for the backend API options
        builder.Services.Configure<BackendApiOptions>(builder.Configuration.GetSection(BackendApiOptions.SectionName));
        builder.Services.AddSingleton<IBackendApiBaseUrlResolver, BackendApiBaseUrlResolver>();
        builder.Services.AddSingleton<BackendApiEndpoints>();

        // Add httpclient service for API calls
        builder.Services.AddHttpClient(BackendApiEndpoints.ClientName, (sp, client) => {
            var o = sp.GetRequiredService<BackendApiEndpoints>();
            client.BaseAddress = o.BaseUri ?? throw new InvalidOperationException("API base URL is not configured");
        })
        .ConfigurePrimaryHttpMessageHandler(HttpClientHelper.CreatePlatformMessageHandler);

        builder.Services.AddHttpClient<AuthApiClient>((sp, client) => {
            var endpoint = sp.GetRequiredService<BackendApiEndpoints>();
            client.BaseAddress = endpoint.BaseUri ?? throw new InvalidOperationException("API base URL is not configured");
        })
        .ConfigurePrimaryHttpMessageHandler(HttpClientHelper.CreatePlatformMessageHandler);


        // Add app services
        builder.Services.AddSingleton<TokenStorage>();
        // This is our custom provider
        builder.Services.AddScoped<MauiAuthenticationStateProvider>();
        // Use our custom provider when the app needs an AuthenticationStateProvider
        builder.Services.AddScoped<AuthenticationStateProvider>(s => (MauiAuthenticationStateProvider)s.GetRequiredService<MauiAuthenticationStateProvider>());
        builder.Services.AddScoped<IAuthenticationService>(s => s.GetRequiredService<MauiAuthenticationStateProvider>());

        //-authentication

        // Add other services
        builder.Services.AddTransient<IFormFactor, FormFactorService>();


        builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
