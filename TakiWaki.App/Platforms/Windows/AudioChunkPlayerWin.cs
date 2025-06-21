using System.Collections.Concurrent;
using NAudio.Wave;
using TakiWaki.App.Services;

namespace TakiWaki.App.Platforms.Windows;

public class AudioChunkPlayerWin : IAudioChunkPlayer
{
    private WaveOutEvent? _waveOut;
    private BufferedWaveProvider? _bufferedWaveProvider;
    private readonly int _sampleRate = 16000;
    private readonly int _channels = 1;
    private readonly ConcurrentQueue<byte[]> _bufferQueue = new();
    private bool _isPlaying;
    private CancellationTokenSource? _cts;

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
        _waveOut = new WaveOutEvent();
        _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(_sampleRate, 16, _channels));
        _waveOut.Init(_bufferedWaveProvider);
        _waveOut.Play();
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
                if (playBuffer.Count >= _sampleRate * 2) // ~1 second buffer
                {
                    _bufferedWaveProvider?.AddSamples(playBuffer.ToArray(), 0, playBuffer.Count);
                    playBuffer.Clear();
                }
            }
            Thread.Sleep(10);
        }
        if (playBuffer.Count > 0)
        {
            _bufferedWaveProvider?.AddSamples(playBuffer.ToArray(), 0, playBuffer.Count);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _waveOut = null;
        _bufferedWaveProvider = null;
        _isPlaying = false;
    }
}
