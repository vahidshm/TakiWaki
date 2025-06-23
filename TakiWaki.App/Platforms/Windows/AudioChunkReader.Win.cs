using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using TakiWaki.App.Services;

namespace TakiWaki.App.Platforms.Windows;

public class WindowsAudioChunkReader : IAudioChunkReader
{
    public event EventHandler<AudioChunkEventArgs>? AudioChunkReady;
    private WaveInEvent? _waveIn;
    private readonly int _sampleRate = 16000;
    private readonly int _channels = 1;
    private readonly int _chunkSize = 3200; // 100ms at 16kHz, 16bit mono
    private byte[]? _buffer;
    private int _bufferOffset;

    public Task StartAsync()
    {
        _buffer = new byte[_chunkSize];
        _bufferOffset = 0;
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(_sampleRate, 16, _channels),
            BufferMilliseconds = 50
        };
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_waveIn != null)
        {
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.StopRecording();
            _waveIn.Dispose();
            _waveIn = null;
        }
        return Task.CompletedTask;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        int bytesToCopy = e.BytesRecorded;
        int srcOffset = 0;
        while (bytesToCopy > 0 && _buffer != null)
        {
            int copy = Math.Min(_chunkSize - _bufferOffset, bytesToCopy);
            Array.Copy(e.Buffer, srcOffset, _buffer, _bufferOffset, copy);
            _bufferOffset += copy;
            srcOffset += copy;
            bytesToCopy -= copy;
            if (_bufferOffset == _chunkSize)
            {
                AudioChunkReady?.Invoke(this, new AudioChunkEventArgs(_buffer));
                _buffer = new byte[_chunkSize];
                _bufferOffset = 0;
            }
        }
    }
}
