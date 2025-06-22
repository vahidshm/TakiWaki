using System.Net;
using System.Net.Sockets;
using TakiWaki.App.Pages;

namespace TakiWaki.App.Services;

public class UdpAudioReceiverService
{
    private UdpClient? _udpClient;
    private bool _isListening;
    private ClientPage? _clientPage;
    private readonly IAudioChunkPlayer _audioChunkPlayer;

    public UdpAudioReceiverService(IAudioChunkPlayer audioChunkPlayer)
    {
        _audioChunkPlayer = audioChunkPlayer;
    }

    public async Task StartAsync(string serverIP, int port, ClientPage? clientPage = null)
    {
        if (_isListening) return;
        _udpClient = new UdpClient(port);
        _isListening = true;
        _clientPage = clientPage;
        _clientPage?.LogClient("Client Connected");
        await Task.Run(async () =>
        {
            while (_isListening)
            {
                var result = await _udpClient.ReceiveAsync();
                _audioChunkPlayer.AddChunk(result.Buffer);
            }
        });
    }

    public void Stop()
    {
        _isListening = false;
        _udpClient?.Dispose();
        _audioChunkPlayer.Stop();
        _clientPage?.LogClient("Client Disconnected");
    }
}