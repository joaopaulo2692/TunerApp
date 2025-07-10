namespace TunerApp.Services;

public class AudioCaptureManager
{
    private readonly IMicrophoneService _microphoneService;
    private readonly FrequencyAnalyzer _analyzer = new();
    private const int SampleRate = 44100;

    public event Action<string>? OnNoteDetected;

    public AudioCaptureManager(IMicrophoneService microphoneService)
    {
        _microphoneService = microphoneService ?? throw new ArgumentNullException(nameof(microphoneService));
    }



    public async Task Start()
    {
        await _microphoneService.StartRecordingAsync(buffer =>
        {
            float[] samples = ConvertToFloat(buffer);
            double freq = _analyzer.DetectFrequency(samples, SampleRate);
            string note = NoteDetector.GetNoteFromFrequency(freq);
            OnNoteDetected?.Invoke(note);
        });
    }

    public void Stop()
    {
        _microphoneService.StopRecording();
    }

    private float[] ConvertToFloat(byte[] buffer)
    {
        float[] floats = new float[buffer.Length / 2];
        for (int i = 0; i < floats.Length; i++)
        {
            short sample = BitConverter.ToInt16(buffer, i * 2);
            floats[i] = sample / 32768f;
        }
        return floats;
    }
}
