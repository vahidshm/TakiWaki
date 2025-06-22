using TakiWaki.App.Services;
using System.Collections.ObjectModel;

namespace TakiWaki.App.Pages;

public partial class ServerPage : ContentPage
{
    private readonly ListeningService _listeningService;
    private readonly INetworkService _networkService;
    private bool _isRecording;
    private const int DefaultPort = 5000;
    private ObservableCollection<string> _clients = new();

    public ServerPage(ListeningService listeningService, INetworkService networkService)
    {
        InitializeComponent();
        _listeningService = listeningService;
        _networkService = networkService;
        ClientsListView.ItemsSource = _clients;
        UpdateStatus();
        SetServerInfo();
    }

    public void LogServer(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            ServerLogEditor.Text += $"[{timestamp}] {message}\n";
        });
    }

    private async void SetServerInfo()
    {
        var ip = await _networkService.GetLocalIPAddress();
        ServerInfoLabel.Text = ServerInfoLabel.Text.Replace("{IP}", ip);
        PortInfoLabel.Text = PortInfoLabel.Text.Replace("{Port}", DefaultPort.ToString());
    }

    private async void OnStartStopButtonClicked(object sender, EventArgs e)
    {
        if (!_isRecording)
        {
            await _listeningService.StartAsync(DefaultPort, this);
            StartStopButton.Text = "Stop";
            StartStopButton.BackgroundColor = Colors.Red;
            StartStopButton.TextColor = Colors.White;
            StartStopButton.IsEnabled = true;
            _isRecording = true;
        }
        else
        {
            await _listeningService.StopAsync();
            StartStopButton.Text = "Start";
            StartStopButton.BackgroundColor = Color.FromArgb("#a899e6"); // Light purple
            StartStopButton.TextColor = Colors.Black;
            StartStopButton.IsEnabled = true;
            _isRecording = false;
        }
        UpdateStatus();
    }

    private void OnAddClientClicked(object sender, EventArgs e)
    {
        var ip = ClientIpEntry.Text?.Trim();
        var portText = ClientPortEntry.Text?.Trim();
        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portText) || !int.TryParse(portText, out int port))
        {
            DisplayAlert("Error", "Please enter a valid client IP and port.", "OK");
            return;
        }
        var clientString = $"{ip}:{port}";
        if (!_clients.Contains(clientString))
        {
            _listeningService.RegisterClient(ip, port);
            _clients.Add(clientString);
        }
        ClientIpEntry.Text = string.Empty;
        ClientPortEntry.Text = string.Empty;
    }

    private void UpdateStatus()
    {
        // No longer set Title for status, handled in XAML
    }

    private void OnServerLogClearClicked(object sender, EventArgs e)
    {
        ServerLogEditor.Text = string.Empty;
    }
}