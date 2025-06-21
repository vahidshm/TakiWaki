namespace TakiWaki.App.Services;

public interface IAudioChunkPlayer
{
    void AddChunk(byte[] chunk);
    void Stop();
}
