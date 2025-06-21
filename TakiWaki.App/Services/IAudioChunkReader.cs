using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakiWaki.App.Services;

public interface IAudioChunkReader
{
    event EventHandler<AudioChunkEventArgs> AudioChunkReady;
    Task StartAsync();
    Task StopAsync();
}
