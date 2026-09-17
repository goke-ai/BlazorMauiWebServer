using CommunityToolkit.Maui;
using Goke.Core.Authorization;
using Goke.Core.Interfaces;
using Goke.Core.Options;
using Goke.Core.Security;
using Goke.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Reflection;

namespace GokeApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureSyncfusionToolkit()
			.ConfigureMauiHandlers(handlers =>
			{
#if WINDOWS
				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
				{
					handler.PlatformView.SingleSelectionFollowsFocus = false;
				});

				Microsoft.Maui.Handlers.ContentViewHandler.Mapper.AppendToMapping(nameof(Pages.Controls.CategoryChart), (handler, view) =>
				{
					if (view is Pages.Controls.CategoryChart && handler.PlatformView is Microsoft.Maui.Platform.ContentPanel contentPanel)
					{
						contentPanel.IsTabStop = true;
					}
				});
#endif
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
				fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
			});

#if DEBUG
		builder.Logging.AddDebug();
		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        //+authentication
        // Load the embedded appsettings.json file
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GokeApp.appsettings.json")
            ?? throw new InvalidOperationException("Could not find embedded resource 'GokeApp.appsettings.json'");

        builder.Configuration.AddJsonStream(stream);


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
        // builder.Services.AddSingleton<MauiAuthenticationStateProvider>();
        // Use our custom provider when the app needs an AuthenticationStateProvider
        //builder.Services.AddScoped<AuthenticationStateProvider>(s => (MauiAuthenticationStateProvider)s.GetRequiredService<MauiAuthenticationStateProvider>());
        //builder.Services.AddSingleton<IAuthenticationService>(s => s.GetRequiredService<MauiAuthenticationStateProvider>());
        builder.Services.AddSingleton<IAuthenticationService, MauiAuthenticationStateProvider>();

        //-authentication

        //+authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnlyPolicy", policy =>
                policy.RequireRole("Administrators"));

            options.AddPolicy("DepartmentITPolicy", policy =>
                policy.RequireClaim("Department", "IT"));

            options.AddPolicy("ProfileEditPolicy", policy =>
                policy.RequirePermission("Profile.Edit"));
        });
        //-authorization policies


        // Add repositories
        builder.Services.AddSingleton<ProjectRepository>();
		builder.Services.AddSingleton<TaskRepository>();
		builder.Services.AddSingleton<CategoryRepository>();
		builder.Services.AddSingleton<TagRepository>();

        // Add services
        builder.Services.AddSingleton<SeedDataService>();
        builder.Services.AddTransient<IFormFactor, FormFactorService>();


        // Add error handler
        builder.Services.AddSingleton<ModalErrorHandler>();

        // Add page models
        builder.Services.AddSingleton<MainPageModel>();
        builder.Services.AddSingleton<CounterPageModel>();
        builder.Services.AddSingleton<WeatherPageModel>();
        builder.Services.AddSingleton<LoginPageModel>();
        builder.Services.AddSingleton<LogoutPageModel>();
        //
        builder.Services.AddSingleton<DashboardPageModel>();
		builder.Services.AddSingleton<ProjectListPageModel>();
		builder.Services.AddSingleton<ManageMetaPageModel>();

        // Add pages and page models with shell routes
        builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
		builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");
		
		return builder.Build();
	}
}
