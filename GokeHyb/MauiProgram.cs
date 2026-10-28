using Goke.Aspnet.Core;
using Goke.Core.Authentication;
using Goke.Core.Interfaces;
using Goke.Core.Map;
using Goke.Core.Options;
using Goke.Maui.Core.Extensions;
using Goke.Maui.Core.Services;
using GokeHyb.Extensions.DependencyInjection;
using GokeHyb.Services;
using GokeSharedCore.ChatApp;
using GokeSharedCore.MapApp;
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

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Configuration
			.AddEmbeddedJsonResource(Assembly.GetExecutingAssembly(), "GokeHyb.appsettings.json");

        builder.Services
			.AddBackendApi(builder.Configuration)
			.AddAuthentication(builder.Configuration)
			.AddAuthorization(builder.Configuration);

        //
        builder.Services.AddSingleton<ChatDbContext>();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IUserService, Services.ChatUserApiClient>();
        builder.Services.Configure<HubOptions>(builder.Configuration.GetSection(HubOptions.SectionName));
        builder.Services.AddScoped<IAccessTokenProvider, MauiAccessTokenProvider>();
        builder.Services.AddScoped<ICookieProvider, MauiCookieProvider>();
        builder.Services.AddScoped<IHubClientFactory, HubClientFactory>();
        builder.Services.AddScoped<ChatHubClientService>();


        // Add other services
        builder.Services.AddTransient<IFormFactor, FormFactorService>();
        builder.Services.AddTransient<ILocationService, LocationService>();
        builder.Services.AddSingleton<TrackerEngine>();


        builder.Services.AddMauiBlazorWebView();


		return builder.Build();
	}
}
