using System.Net;
using System.Net.Sockets;
#if ANDROID
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.App;
#endif

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
            return current == Microsoft.Maui.Networking.NetworkAccess.Internet;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetLocalIPAddress()
    {
#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);
            if (wifiManager != null)
            {
                var wifiInfo = wifiManager.ConnectionInfo;
                int ip = wifiInfo.IpAddress;
                if (ip != 0)
                {
                    return string.Format("{0}.{1}.{2}.{3}",
                        (ip & 0xff),
                        (ip >> 8 & 0xff),
                        (ip >> 16 & 0xff),
                        (ip >> 24 & 0xff));
                }
            }
        }
        catch { }
#endif
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !ip.ToString().StartsWith("127."))
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