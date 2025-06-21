using Android.Media;
using System.Collections.Concurrent;
using System.Threading;
using TakiWaki.App.Services;

namespace TakiWaki.App.Platforms.Android;

public class AudioChunkPlayerAndroid : IAudioChunkPlayer
{
    private AudioTrack? _audioTrack;
    private readonly int _sampleRate = 16000;
    private readonly ChannelOut _channelConfig = ChannelOut.Mono;
    private readonly Encoding _audioFormat = Encoding.Pcm16bit;
    private readonly ConcurrentQueue<byte[]> _bufferQueue = new();
    private CancellationTokenSource? _cts;
    private int _minBufferSize;
    private bool _isPlaying;

    public AudioChunkPlayerAndroid()
    {
        _minBufferSize = AudioTrack.GetMinBufferSize(_sampleRate, _channelConfig, _audioFormat);
    }

    public void AddChunk(byte[] chunk)
    {
        _bufferQueue.Enqueue(chunk);
        if (!_isPlaying)
        {
            StartPlaybackLoop();
        }
    }

    private void StartPlaybackLoop()
    {
        _isPlaying = true;
        _cts = new CancellationTokenSource();

        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
        {
            // Use AudioTrack.Builder for API 23+ (M and above)
            var audioAttributes = new AudioAttributes.Builder()!
                .SetUsage(AudioUsageKind.Media)!
                .SetContentType(AudioContentType.Music)!
                .Build()!; // Suppress null able warning, Build() should not return null

            var audioFormat = new AudioFormat.Builder()!
                .SetSampleRate(_sampleRate)!
                .SetEncoding(_audioFormat)!
                .SetChannelMask(_channelConfig)!
                .Build()!; // Suppress null able warning

            _audioTrack = new AudioTrack.Builder()
                .SetAudioAttributes(audioAttributes)
                .SetAudioFormat(audioFormat)
                .SetBufferSizeInBytes(Math.Max(_minBufferSize, 3200 * 10))
                .SetTransferMode(AudioTrackMode.Stream)
                .Build()!; // Suppress null able warning
        }
        else
        {
            // Use legacy constructor for API < 23
            _audioTrack = new AudioTrack(
                global::Android.Media.Stream.Music,
                _sampleRate,
                _channelConfig,
                _audioFormat,
                Math.Max(_minBufferSize, 3200 * 10),
                AudioTrackMode.Stream);
        }

        _audioTrack?.Play();
        var token = _cts.Token;
        Task.Run(() => PlaybackLoop(token));
    }

    private void PlaybackLoop(CancellationToken token)
    {
        var playBuffer = new List<byte>();
        while (!token.IsCancellationRequested)
        {
            while (_bufferQueue.TryDequeue(out var chunk))
            {
                playBuffer.AddRange(chunk);
                if (playBuffer.Count >= _minBufferSize * 2) // ~1-2 seconds buffer
                {
                    if (_audioTrack != null)
                    {
                        _audioTrack.Write(playBuffer.ToArray(), 0, playBuffer.Count);
                    }
                    playBuffer.Clear();
                }
            }
            Thread.Sleep(10);
        }
        // Play any remaining buffer
        if (playBuffer.Count > 0 && _audioTrack != null)
        {
            _audioTrack.Write(playBuffer.ToArray(), 0, playBuffer.Count);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _audioTrack?.Stop();
        _audioTrack?.Release();
        _audioTrack = null;
        _isPlaying = false;
    }
}
