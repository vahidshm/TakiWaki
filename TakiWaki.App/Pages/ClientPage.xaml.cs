using TakiWaki.App.Services;

namespace TakiWaki.App.Pages;

public partial class ClientPage : ContentPage
{
    private readonly UdpAudioReceiverService _receiverService;
    private readonly INetworkService _networkService;
    private bool _isConnected;

    public ClientPage(UdpAudioReceiverService receiverService, INetworkService networkService)
    {
        InitializeComponent();
        _receiverService = receiverService;
        _networkService = networkService;
        UpdateStatus();
    }

    public void LogClient(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            ClientLogEditor.Text += $"[{timestamp}] {message}\n";
        });
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
                _isConnected = true;
                ConnectButton.Text = "Disconnect";
                ConnectButton.BackgroundColor = Colors.Red;
                ConnectButton.TextColor = Colors.White;
                ConnectButton.IsEnabled = true;
                UpdateStatus();
                await _receiverService.StartAsync(ip, port, this);
            }
            catch (OperationCanceledException oce)
            {
                LogClient($"Connection canceled: {oce.Message}");
            }
            catch (Exception ex)
            {
                LogClient($"Connection error: {ex.Message}");
            }
        }
        else
        {
            _receiverService.Stop();
            _isConnected = false;
            ConnectButton.Text = "Connect";
            ConnectButton.BackgroundColor = Color.FromArgb("#a899e6"); // Light purple
            ConnectButton.TextColor = Colors.Black;
            ConnectButton.IsEnabled = true;
            UpdateStatus();
        }
    }

    private void UpdateStatus()
    {
        if (ClientStatusLabel != null && ClientStatusIcon != null)
        {
            if (_isConnected)
            {
                ClientStatusLabel.Text = "Client (Connected)";
                ClientStatusIcon.Source = "connected.png";
            }
            else
            {
                ClientStatusLabel.Text = "Client (Disconnected)";
                ClientStatusIcon.Source = "disconnected.png";
            }
        }
    }

    private void OnClientLogClearClicked(object sender, EventArgs e)
    {
        ClientLogEditor.Text = string.Empty;
    }
}