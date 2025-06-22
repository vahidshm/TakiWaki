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
        builder.Services.AddSingleton<IAudioChunkReader, TakiWaki.App.Platforms.Android.AudioChunkReaderAndroid>();
        builder.Services.AddSingleton<IAudioChunkPlayer, TakiWaki.App.Platforms.Android.AudioChunkPlayerAndroid>();
        builder.Services.AddSingleton<INetworkService, Platforms.Android.AndroidNetworkService>();
#elif WINDOWS
        builder.Services.AddSingleton<IAudioChunkReader, TakiWaki.App.Platforms.Windows.AudioChunkReaderWin>();
        builder.Services.AddSingleton<IAudioChunkPlayer, TakiWaki.App.Platforms.Windows.AudioChunkPlayerWin>();
        builder.Services.AddSingleton<INetworkService, BaseNetworkService>();
#else
        builder.Services.AddSingleton<IAudioChunkReader, Services.AudioChunkReaderStub>();
        builder.Services.AddSingleton<IAudioChunkPlayer, Services.AudioChunkPlayerStub>();
         builder.Services.AddSingleton<INetworkService, BaseNetworkService>();
#endif
        builder.Services.AddSingleton<ListeningService>();
        builder.Services.AddSingleton<UdpAudioReceiverService>();
        builder.Services.AddSingleton<ClientPage>(sp => new ClientPage(sp.GetRequiredService<UdpAudioReceiverService>(), sp.GetRequiredService<INetworkService>()));
        builder.Services.AddSingleton<ServerPage>(sp => new ServerPage(sp.GetRequiredService<ListeningService>(), sp.GetRequiredService<INetworkService>()));
        builder.Services.AddSingleton<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
