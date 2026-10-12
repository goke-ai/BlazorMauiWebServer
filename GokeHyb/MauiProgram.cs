using Goke.Core.Interfaces;
using Goke.Maui.Core.Extensions;
using GokeHyb.Extensions.DependencyInjection;
using GokeHyb.Services;
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

        // Add other services
        builder.Services.AddTransient<IFormFactor, FormFactorService>();


        builder.Services.AddMauiBlazorWebView();


		return builder.Build();
	}
}
