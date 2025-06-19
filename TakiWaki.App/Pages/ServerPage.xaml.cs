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
            ServerLogEditor.Text += message + "\n";
        });
    }

    private async void SetServerInfo()
    {
        var ip = await _networkService.GetLocalIPAddress();
        ServerInfoLabel.Text = $"Server: {ip}:{DefaultPort}";
    }

    private async void OnStartStopButtonClicked(object sender, EventArgs e)
    {
        if (!_isRecording)
        {
            await _listeningService.StartAsync(DefaultPort, this);
            StartStopButton.Text = "Stop";
            StartStopButton.BackgroundColor = Colors.Red;
            _isRecording = true;
        }
        else
        {
            await _listeningService.StopAsync();
            StartStopButton.Text = "Start";
            StartStopButton.BackgroundColor = null; // Use null instead of Colors.Default
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
        _listeningService.RegisterClient(ip, port);
        _clients.Add($"{ip}:{port}");
        ClientIpEntry.Text = string.Empty;
        ClientPortEntry.Text = string.Empty;
    }

    private void UpdateStatus()
    {
        Title = _isRecording ? "Server (Listening)" : "Server (Stopped)";
    }
}