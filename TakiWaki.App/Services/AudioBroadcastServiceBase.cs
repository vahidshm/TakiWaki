using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TakiWaki.App.Services;

public class AudioChunkEventArgs : EventArgs
{
    public byte[] AudioChunk { get; }
    public AudioChunkEventArgs(byte[] chunk) => AudioChunk = chunk;
}

public abstract class AudioBroadcastServiceBase
{
    protected UdpClient? _udpClient;
    protected List<IPEndPoint> _clients = new();
    protected int _port;

    public virtual void RegisterClient(string ip, int port)
    {
        var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        if (!_clients.Contains(endpoint))
            _clients.Add(endpoint);
    }

    public virtual void BroadcastAudioChunk(byte[] chunk)
    {
        if (_udpClient == null || _clients.Count == 0) return;
        foreach (var client in _clients)
        {
            _udpClient.Send(chunk, client);
        }
    }

    public virtual void ClearClients()
    {
        _clients.Clear();
    }
}
