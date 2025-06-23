using Android.Media;
using System;
using System.Threading;
using System.Threading.Tasks;
using TakiWaki.App.Services;

namespace TakiWaki.App.Platforms.Android;

public class AndroidAudioChunkReader : IAudioChunkReader
{
    public event EventHandler<AudioChunkEventArgs>? AudioChunkReady;
    private AudioRecord? _audioRecord;
    private CancellationTokenSource? _cts;
    private readonly int _sampleRate = 16000;
    private readonly ChannelIn _channelConfig = ChannelIn.Mono;
    private readonly Encoding _audioFormat = Encoding.Pcm16bit;
    private readonly int _bufferSize;
    private readonly int _chunkSize = 3200; // 100ms at 16kHz, 16bit mono

    public AndroidAudioChunkReader()
    {
        _bufferSize = AudioRecord.GetMinBufferSize(_sampleRate, _channelConfig, _audioFormat);
    }

    public Task StartAsync()
    {
        if (_bufferSize <= 0)
            throw new InvalidOperationException("Invalid buffer size for AudioRecord. Check device audio support.");
        _cts = new CancellationTokenSource();
        _audioRecord = new AudioRecord(
            AudioSource.Mic,
            _sampleRate,
            _channelConfig,
            _audioFormat,
            Math.Max(_bufferSize, _chunkSize * 2));
        if (_audioRecord.State != State.Initialized)
        {
            _audioRecord.Release();
            _audioRecord = null;
            throw new InvalidOperationException("AudioRecord failed to initialize. Check permissions and device support.");
        }
        _audioRecord.StartRecording();
        Task.Run(() => ReadLoop(_cts.Token));
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _cts?.Cancel();
        _audioRecord?.Stop();
        _audioRecord?.Release();
        _audioRecord = null;
        return Task.CompletedTask;
    }

    private void ReadLoop(CancellationToken token)
    {
        var buffer = new byte[_chunkSize];
        while (!token.IsCancellationRequested && _audioRecord != null)
        {
            int read = _audioRecord.Read(buffer, 0, buffer.Length);
            if (read > 0)
            {
                var chunk = new byte[read];
                Array.Copy(buffer, chunk, read);
                AudioChunkReady?.Invoke(this, new AudioChunkEventArgs(chunk));
            }
        }
    }
}
