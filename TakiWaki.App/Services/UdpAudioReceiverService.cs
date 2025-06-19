using System.Net;
using System.Net.Sockets;
using TakiWaki.App.Pages;

namespace TakiWaki.App.Services;

public class UdpAudioReceiverService
{
    // TODO: Implement platform-specific audio playback
    private UdpClient? _udpClient;
    private bool _isListening;
    private ClientPage? _clientPage;

    public UdpAudioReceiverService()
    {
    }

    public async Task StartAsync(string serverIp, int port, ClientPage? clientPage = null)
    {
        if (_isListening) return;
        _udpClient = new UdpClient(port);
        _isListening = true;
        _clientPage = clientPage;
        await Task.Run(async () =>
        {
            while (_isListening)
            {
                var result = await _udpClient.ReceiveAsync();
                _clientPage?.LogClient($"Received packet: {result.Buffer.Length} bytes");
                // TODO: Play audio using platform-specific code
            }
        });
    }

    public void Stop()
    {
        _isListening = false;
        _udpClient?.Dispose();
        // TODO: Stop playback if needed
    }
}