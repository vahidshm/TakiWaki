#if ANDROID
using Android.Content;
using Application = Android.App.Application;
using TakiWaki.App.Platforms.Android;
#endif

namespace TakiWaki.App.Services;

public static class AndroidBackgroundAudioService
{
    public static void StartServer(int port)
    {
#if ANDROID
        var context = Application.Context;
        var intent = new Intent(context, typeof(AndroidAudioForegroundService));
        intent.SetAction(AndroidAudioForegroundService.ACTION_START_SERVER);
        intent.PutExtra("port", port);
        context.StartForegroundService(intent);
#endif
    }

    public static void StartClient(string ip, int port)
    {
#if ANDROID
        var context = Application.Context;
        var intent = new Intent(context, typeof(AndroidAudioForegroundService));
        intent.SetAction(AndroidAudioForegroundService.ACTION_START_CLIENT);
        intent.PutExtra("ip", ip);
        intent.PutExtra("port", port);
        context.StartForegroundService(intent);
#endif
    }

    public static void RegisterClient(string ip, int port)
    {
#if ANDROID
        var context = Application.Context;
        var intent = new Intent(context, typeof(AndroidAudioForegroundService));
        intent.SetAction(AndroidAudioForegroundService.ACTION_REGISTER_CLIENT);
        intent.PutExtra(AndroidAudioForegroundService.EXTRA_CLIENT_IP, ip);
        intent.PutExtra(AndroidAudioForegroundService.EXTRA_CLIENT_PORT, port);
        context.StartService(intent);
#endif
    }

    public static void Stop()
    {
#if ANDROID
        var context = Application.Context;
        var intent = new Intent(context, typeof(AndroidAudioForegroundService));
        intent.SetAction(AndroidAudioForegroundService.ACTION_STOP);
        context.StartService(intent);
#endif
    }
}
