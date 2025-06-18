using System.Net;
using System.Net.Sockets;

namespace TakiWaki.App.Services;

public interface INetworkService
{
    Task<bool> IsWifiEnabled();
    Task<string> GetLocalIPAddress();
    Task<bool> StartServer(int port);
    Task<bool> ConnectToServer(string ipAddress, int port);
}

public class NetworkService : INetworkService
{
    private TcpListener? _server;
    private TcpClient? _client;

    public async Task<bool> IsWifiEnabled()
    {
        try
        {
            var current = Connectivity.NetworkAccess;
            return current == NetworkAccess.Internet;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    public async Task<bool> StartServer(int port)
    {
        try
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ConnectToServer(string ipAddress, int port)
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ipAddress, port);
            return true;
        }
        catch
        {
            return false;
        }
    }
} 