using System.Net.Sockets;
using System.Net;
using TakiWaki.App.Pages;

namespace TakiWaki.App.Services;

public interface IAudioChunkReader
{
    event EventHandler<AudioChunkEventArgs> AudioChunkReady;
    Task StartAsync();
    Task StopAsync();
}

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
    }

    public async Task StartAsync(int port, ServerPage? serverPage = null)
    {
        if (_isListening) return;
        _port = port;
        _udpClient = new UdpClient();
        _audioChunkReader.AudioChunkReady += OnAudioChunkReady;
        _serverPage = serverPage;
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
    }

    private void OnAudioChunkReady(object? sender, AudioChunkEventArgs e)
    {
        if (_udpClient == null || _clients.Count == 0) return;
        foreach (var client in _clients)
        {
            int sent = _udpClient.Send(e.AudioChunk, client);
            _serverPage?.LogServer($"Sent {sent} bytes to {client}");
        }
    }
}