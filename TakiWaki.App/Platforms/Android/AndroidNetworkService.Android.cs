// Ignore Spelling: Taki Waki App

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Net.Wifi;
using TakiWaki.App.Services;

namespace TakiWaki.App.Platforms.Android
{
    internal class AndroidNetworkService : NetworkServiceBase, INetworkService
    {
        public override async Task<string> GetLocalIPAddress()
        {
#if ANDROID
            try
            {
                var context = global::Android.App.Application.Context;
                var wifiManager = (WifiManager?)context.GetSystemService(Context.WifiService);
                if (wifiManager != null)
                {
                    var wifiInfo = wifiManager.ConnectionInfo;
                    int ip = wifiInfo?.IpAddress ?? 0;
                    if (ip != 0)
                    {
                        return string.Format("{0}.{1}.{2}.{3}",
                            (ip & 0xff),
                            (ip >> 8 & 0xff),
                            (ip >> 16 & 0xff), (ip >> 24 & 0xff));
                    }
                }
            }
            catch { }
#endif
            return await base.GetLocalIPAddress();
        }
    }
}