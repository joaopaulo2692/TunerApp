using static TunerApp.MainPage;
using static TunerApp.Services.Enums;

namespace TunerApp.Services;

public class AudioCaptureManager
{
    private readonly IMicrophoneService _microphoneService;
    private readonly FrequencyAnalyzer _analyzer = new();
    private const int SampleRate = 44100;
    private GuitarString? _targetString;
    public event Action<string>? OnNoteDetected;
    private TuningType _currentTuning = TuningType.Standard;

    public AudioCaptureManager(IMicrophoneService microphoneService)
    {
        _microphoneService = microphoneService ?? throw new ArgumentNullException(nameof(microphoneService));
    }


    public void SetTargetString(GuitarString? guitarString)
    {
        _targetString = guitarString;
    }
    public async Task Start()
    {

        await _microphoneService.StartRecordingAsync(buffer =>
        {
            float[] samples = ConvertToFloat(buffer);
            ApplyHanningWindow(samples);


            float amplitude = samples.Select(x => Math.Abs(x)).Average();
            if (amplitude < 0.01f)
            {
                OnNoteDetected?.Invoke("Sem Sinal");
                return;
            }

            double freq = _analyzer.DetectFrequency(samples, SampleRate);
            if (double.IsNaN(freq) || freq <= 0)
            {
                OnNoteDetected?.Invoke("Sem Sinal");
                return;
            }
            if (_targetString != null)
            {
                var target = GuitarTuning.StandardTuning[_targetString.Value];
                int cents = (int)(1200 * Math.Log2(freq / target.Frequency));
                string note = NoteDetector.GetNoteInfo(freq).Note;
                OnNoteDetected?.Invoke($"{note} ({cents:+#;-#;0} cents)");
            }
            else
            {
                //var noteInfo = NoteDetector.GetNoteInfo(freq);
                //OnNoteDetected?.Invoke($"{noteInfo.Note} ({noteInfo.Cents:+#;-#;0} cents)");
                var noteInfo = NoteDetector.GetNoteInfo(freq);
                string guessedString = DetectStringFromFrequency(freq);

                OnNoteDetected?.Invoke($"{noteInfo.Note} ({noteInfo.Cents:+#;-#;0} cents) - Provável corda: {guessedString}");
            }

       


        });

    }

    public void SetTuning(TuningType tuning)
    {
        _currentTuning = tuning;
    }

    private string DetectStringFromFrequency(double freq)
    {
        var strings = GuitarTunings.Tunings[_currentTuning];

        return strings
    .OrderBy(pair => Math.Abs(pair.Value.Frequency - freq))
    .First().Key.ToString();
    }

    

    private void ApplyHanningWindow(float[] samples)
    {
        int N = samples.Length;
        for (int i = 0; i < N; i++)
        {
            samples[i] *= 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (N - 1)));
        }
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
