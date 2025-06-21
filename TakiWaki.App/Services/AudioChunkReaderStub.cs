using System;
using System.Threading.Tasks;

namespace TakiWaki.App.Services;

public class AudioChunkReaderStub : IAudioChunkReader
{
    public event EventHandler<AudioChunkEventArgs>? AudioChunkReady { add { } remove { } }
    public Task StartAsync() => Task.CompletedTask;
    public Task StopAsync() => Task.CompletedTask;
}
