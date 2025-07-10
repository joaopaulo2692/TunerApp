namespace TunerApp.Services;

public interface IMicrophoneService
{
    Task StartRecordingAsync(Action<byte[]> onDataAvailable);
    void StopRecording();
}
