using TakiWaki.App.Services;

namespace TakiWaki.App.Pages;

public partial class ClientPage : ContentPage
{
    private readonly UdpAudioReceiverService _receiverService;
    private readonly INetworkService _networkService;
    private bool _isConnected;
    private int _localPort = 0;

    public ClientPage(UdpAudioReceiverService receiverService, INetworkService networkService)
    {
        InitializeComponent();
        _receiverService = receiverService;
        _networkService = networkService;
        UpdateStatus();
        SetClientInfo();
    }

    public void LogClient(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            ClientLogEditor.Text += $"[{timestamp}] {message}\n";
        });
    }

    private async void SetClientInfo()
    {
        var ip = await _networkService.GetLocalIPAddress();
        ClientInfoLabel.Text = $"Client: {ip}:{_localPort}";
    }

    private async void OnConnectButtonClicked(object sender, EventArgs e)
    {
        if (!_isConnected)
        {
            var ip = ServerIpEntry.Text?.Trim();
            var portText = ServerPortEntry.Text?.Trim();
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portText) || !int.TryParse(portText, out int port))
            {
                await DisplayAlert("Error", "Please enter a valid server IP and port.", "OK");
                return;
            }
            try
            {
                _localPort = port; // For display purposes, show the port we're connecting to
                await _receiverService.StartAsync(ip, port, this);
                ConnectButton.Text = "Disconnect";
                ConnectButton.BackgroundColor = Colors.Red;
                _isConnected = true;
                SetClientInfo();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Error", ex.Message, "OK");
            }
        }
        else
        {
            _receiverService.Stop();
            ConnectButton.Text = "Connect";
            ConnectButton.BackgroundColor = null; // Use null instead of Colors.Default
            _isConnected = false;
            _localPort = 0;
            SetClientInfo();
        }
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        Title = _isConnected ? "Client (Connected)" : "Client (Disconnected)";
    }

    private void OnClientLogClearClicked(object sender, EventArgs e)
    {
        ClientLogEditor.Text = string.Empty;
    }
}