using System.Net.Sockets;
using System.Net;
using TakiWaki.App.Pages;

namespace TakiWaki.App.Services;

public class WindowsAudioBroadcaseService : AudioBroadcastServiceBase
{
    private readonly IAudioChunkReader _audioChunkReader;
    private bool _isListening;
    private ServerPage? _serverPage;

    public WindowsAudioBroadcaseService(IAudioChunkReader audioChunkReader)
    {
        _audioChunkReader = audioChunkReader;
    }

    public override void RegisterClient(string ip, int port)
    {
        base.RegisterClient(ip, port);
        _serverPage?.LogServer($"Client {ip} Registered.");
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
        BroadcastAudioChunk(e.AudioChunk);
    }
}