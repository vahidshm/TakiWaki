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
            });

        builder.Services.AddSingleton<INetworkService, NetworkService>();
#if ANDROID
        builder.Services.AddSingleton<IAudioChunkReader, TakiWaki.App.Platforms.Android.AudioChunkReaderAndroid>();
#elif WINDOWS
        builder.Services.AddSingleton<IAudioChunkReader, TakiWaki.App.Platforms.Windows.IAudioChunkReaderWin>();
#else
        builder.Services.AddSingleton<IAudioChunkReader, AudioChunkReaderStub>();
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
