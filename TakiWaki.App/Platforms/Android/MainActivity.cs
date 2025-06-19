using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.Maui.ApplicationModel;

namespace TakiWaki.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    const int RequestRecordAudioId = 10;
    readonly string[] Permissions = { Manifest.Permission.RecordAudio };

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestAudioPermission();
    }

    void RequestAudioPermission()
    {
        if ((int)Build.VERSION.SdkInt < 23)
            return;
        if (CheckSelfPermission(Manifest.Permission.RecordAudio) != Permission.Granted)
        {
            RequestPermissions(Permissions, RequestRecordAudioId);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        // Optionally handle permission result here
    }
}
