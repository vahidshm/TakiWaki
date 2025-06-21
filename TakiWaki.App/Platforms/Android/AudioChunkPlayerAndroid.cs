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
        _audioTrack = new AudioTrack(
            global::Android.Media.Stream.Music,
            _sampleRate,
            _channelConfig,
            _audioFormat,
            Math.Max(_minBufferSize, 3200 * 10),
            AudioTrackMode.Stream);
        _audioTrack.Play();
        Task.Run(() => PlaybackLoop(_cts.Token));
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
                    _audioTrack?.Write(playBuffer.ToArray(), 0, playBuffer.Count);
                    playBuffer.Clear();
                }
            }
            Thread.Sleep(10);
        }
        // Play any remaining buffer
        if (playBuffer.Count > 0)
        {
            _audioTrack?.Write(playBuffer.ToArray(), 0, playBuffer.Count);
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
