using Microsoft.Extensions.Logging;
using TakiWaki.App.Pages;
using TakiWaki.App.Services;

namespace TakiWaki.App;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<INetworkService, NetworkService>();
		builder.Services.AddSingleton<ClientPage>();
		builder.Services.AddSingleton<ServerPage>();
		builder.Services.AddSingleton<SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
