// Ignore Spelling: App

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
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            });


#if ANDROID
        builder.Services.AddSingleton<IAudioChunkReader, TakiWaki.App.Platforms.Android.AndroidAudioChunkReader>();
        builder.Services.AddSingleton<IAudioChunkPlayer, TakiWaki.App.Platforms.Android.AndroidAudioChunkPlayer>();
        builder.Services.AddSingleton<INetworkService, Platforms.Android.AndroidNetworkService>();
#elif WINDOWS
        builder.Services.AddSingleton<IAudioChunkReader, TakiWaki.App.Platforms.Windows.WindowsAudioChunkReader>();
        builder.Services.AddSingleton<IAudioChunkPlayer, TakiWaki.App.Platforms.Windows.WindowsAudioChunkPlayer>();
        builder.Services.AddSingleton<INetworkService, NetworkServiceBase>();
#else
        builder.Services.AddSingleton<IAudioChunkReader, Services.AudioChunkReaderStub>();
        builder.Services.AddSingleton<IAudioChunkPlayer, Services.AudioChunkPlayerStub>();
        builder.Services.AddSingleton<INetworkService, NetworkServiceBase>();
#endif
        builder.Services.AddSingleton<TakiWaki.App.Services.ListeningServerService>();
        builder.Services.AddSingleton<UdpAudioReceiverService>();
        builder.Services.AddSingleton<ClientPage>(sp => new ClientPage(sp.GetRequiredService<UdpAudioReceiverService>(), sp.GetRequiredService<INetworkService>()));
        builder.Services.AddSingleton<ServerPage>(sp => new ServerPage(sp.GetRequiredService<ListeningServerService>(), sp.GetRequiredService<INetworkService>()));
        builder.Services.AddSingleton<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
