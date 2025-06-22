using System.Net.Sockets;
using System.Net;
using TakiWaki.App.Pages;

namespace TakiWaki.App.Services;

public class AudioChunkEventArgs : EventArgs
{
    public byte[] AudioChunk { get; }
    public AudioChunkEventArgs(byte[] chunk) => AudioChunk = chunk;
}

public class ListeningService
{
    private readonly IAudioChunkReader _audioChunkReader;
    private UdpClient? _udpClient;
    private bool _isListening;
    private int _port;
    private List<IPEndPoint> _clients = new();
    private ServerPage? _serverPage;

    public ListeningService(IAudioChunkReader audioChunkReader)
    {
        _audioChunkReader = audioChunkReader;
    }

    public void RegisterClient(string ip, int port)
    {
        _clients.Add(new IPEndPoint(IPAddress.Parse(ip), port));
        _serverPage?.LogServer($"Client {ip} Connected");
    }

    public async Task StartAsync(int port, ServerPage? serverPage = null)
    {
        if (_isListening) return;
        _port = port;
        _udpClient = new UdpClient();
        _audioChunkReader.AudioChunkReady += OnAudioChunkReady;
        _serverPage = serverPage;
        _serverPage?.LogServer("Server Started");
        await _audioChunkReader.StartAsync();
        _isListening = true;
    }

    public async Task StopAsync()
    {
        if (!_isListening) return;
        _audioChunkReader.AudioChunkReady -= OnAudioChunkReady;
        await _audioChunkReader.StopAsync();
        _udpClient?.Dispose();
        _isListening = false;
        _serverPage?.LogServer("Server Stopped");
    }

    private void OnAudioChunkReady(object? sender, AudioChunkEventArgs e)
    {
        if (_udpClient == null || _clients.Count == 0) return;
        foreach (var client in _clients)
        {
            _udpClient.Send(e.AudioChunk, client);
        }
    }
}