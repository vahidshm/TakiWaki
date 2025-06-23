using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using TakiWaki.App.Services;

namespace TakiWaki.App.Platforms.Android
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback | global::Android.Content.PM.ForegroundService.TypeMicrophone)]
    public class AndroidAudioForegroundService : Service
    {
        public const int SERVICE_NOTIFICATION_ID = 1001;
        public const string CHANNEL_ID = "audio_foreground_channel";
        public const string ACTION_START_SERVER = "START_SERVER";
        public const string ACTION_START_CLIENT = "START_CLIENT";
        public const string ACTION_STOP = "STOP";
        public const string ACTION_REGISTER_CLIENT = "REGISTER_CLIENT";
        public const string EXTRA_CLIENT_IP = "client_ip";
        public const string EXTRA_CLIENT_PORT = "client_port";

        private Task? _audioTask;
        private bool _isRunning;
        private string? _mode;
        private int _port;
        private string? _ip;
        private AndroidAudioBroadcastService? _broadcastService;

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();
            _broadcastService ??= new AndroidAudioBroadcastService();
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent == null) return StartCommandResult.NotSticky;
            var action = intent.Action;
            if (action == ACTION_START_SERVER)
            {
                _mode = "server";
                _port = intent.GetIntExtra("port", 5000);
                StartForeground(SERVICE_NOTIFICATION_ID, BuildNotification("Audio Server Running"));
                StartServer();
            }
            else if (action == ACTION_START_CLIENT)
            {
                _mode = "client";
                _ip = intent.GetStringExtra("ip");
                _port = intent.GetIntExtra("port", 5000);
                StartForeground(SERVICE_NOTIFICATION_ID, BuildNotification("Audio Client Running"));
                StartClient();
            }
            else if (action == ACTION_REGISTER_CLIENT)
            {
                var clientIp = intent.GetStringExtra(EXTRA_CLIENT_IP);
                var clientPort = intent.GetIntExtra(EXTRA_CLIENT_PORT, 0);
                if (!string.IsNullOrEmpty(clientIp) && clientPort > 0 && _broadcastService != null)
                {
                    _broadcastService.RegisterClient(clientIp, clientPort);
                }
            }
            else if (action == ACTION_STOP)
            {
                StopSelf();
            }
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            _isRunning = false;
            base.OnDestroy();
        }

        public override IBinder? OnBind(Intent? intent) => null;

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(CHANNEL_ID, "Audio Foreground", NotificationImportance.Default)
                {
                    Description = "Audio background service"
                };
                var manager = (NotificationManager)GetSystemService(NotificationService)!;
                manager.CreateNotificationChannel(channel);
            }
        }

        private Notification BuildNotification(string text)
        {
            var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("TakiWaki Audio")
                .SetContentText(text)
                .SetSmallIcon(global::Android.Resource.Drawable.IcMediaPlay)
                .SetOngoing(true);
            return builder.Build();
        }

        private void StartServer()
        {
            if (_isRunning) return;
            _isRunning = true;
            _audioTask = Task.Run(async () =>
            {
                var audioReader = new AndroidAudioChunkReader();
                audioReader.AudioChunkReady += (s, e) =>
                {
                    _broadcastService?.BroadcastAudioChunk(e.AudioChunk);
                };
                await audioReader.StartAsync();
                while (_isRunning) await Task.Delay(500);
                await audioReader.StopAsync();
            });
        }

        private void StartClient()
        {
            if (_isRunning) return;
            _isRunning = true;
            _audioTask = Task.Run(async () =>
            {
                var udpClient = new UdpClient(_port);
                var audioPlayer = new AndroidAudioChunkPlayer();
                while (_isRunning)
                {
                    var result = await udpClient.ReceiveAsync();
                    audioPlayer.AddChunk(result.Buffer);
                }
                audioPlayer.Stop();
                udpClient.Dispose();
            });
        }
    }

    // Android-specific implementation of AudioBroadcastServiceBase
    public class AndroidAudioBroadcastService : AudioBroadcastServiceBase
    {
        public AndroidAudioBroadcastService()
        {
            _udpClient = new UdpClient();
        }
    }
}
