using CommunityToolkit.Maui;
using Goke.Core.Authorization;
using Goke.Core.Interfaces;
using Goke.Maui.Core.Extensions;
using GokeApp.Extensions.DependencyInjection;
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

        builder.Configuration
			.AddEmbeddedJsonResource(Assembly.GetExecutingAssembly(), "GokeApp.appsettings.json");

        builder.Services
            .AddBackendApi(builder.Configuration)
            .AddAuthentication(builder.Configuration)
            .AddAuthorization(builder.Configuration);

        // Add other services
        builder.Services.AddTransient<IFormFactor, FormFactorService>();

        // Add repositories
        builder.Services.AddSingleton<ProjectRepository>();
		builder.Services.AddSingleton<TaskRepository>();
		builder.Services.AddSingleton<CategoryRepository>();
		builder.Services.AddSingleton<TagRepository>();

        // Add services
        builder.Services.AddSingleton<SeedDataService>();

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
