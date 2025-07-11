using Plugin.Maui.Audio;
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
    private IAudioPlayer? _tunedSound;
    private TuningType _currentTuning = TuningType.Standard;
    private readonly ISoundPlayer _soundPlayer;
    // Frequência de referência ajustável (padrão 440 Hz)
    private double _referenceA4Frequency = 440.0;
    public double ReferenceA4Frequency
    {
        get => _referenceA4Frequency;
        set
        {
            if (value > 0)
            {
                _referenceA4Frequency = value;
                // Se desejar, pode disparar evento para atualizar UI aqui
            }
        }
    }

    public AudioCaptureManager(IMicrophoneService microphoneService, ISoundPlayer soundPlayer)
    {
        _microphoneService = microphoneService ?? throw new ArgumentNullException(nameof(microphoneService));
        _soundPlayer = soundPlayer;
    }

    public void SetTargetString(GuitarString? guitarString)
    {
        _targetString = guitarString;
    }

    public void SetTuning(TuningType tuning)
    {
        _currentTuning = tuning;
    }

    //public async Task Start()
    //{
    //    await _microphoneService.StartRecordingAsync(buffer =>
    //    {
    //        float[] samples = ConvertToFloat(buffer);
    //        ApplyHanningWindow(samples);

    //        float amplitude = samples.Select(x => Math.Abs(x)).Average();
    //        if (amplitude < 0.01f)
    //        {
    //            OnNoteDetected?.Invoke("Sem Sinal");
    //            return;
    //        }

    //        double freq = _analyzer.DetectFrequency(samples, SampleRate);
    //        if (double.IsNaN(freq) || freq <= 0)
    //        {
    //            OnNoteDetected?.Invoke("Sem Sinal");
    //            return;
    //        }

    //        if (_targetString != null)
    //        {
    //            var target = GuitarTunings.Tunings[_currentTuning][_targetString.Value];

    //            // Ajusta frequência da nota alvo conforme referência
    //            double adjustedFrequency = target.Frequency * (_referenceA4Frequency / 440.0);

    //            int cents = (int)(1200 * Math.Log2(freq / adjustedFrequency));
    //            string note = NoteDetector.GetNoteInfo(freq, _referenceA4Frequency).Note;
    //            OnNoteDetected?.Invoke($"{note} ({cents:+#;-#;0} cents)");
    //        }
    //        else
    //        {
    //            var noteInfo = NoteDetector.GetNoteInfo(freq, _referenceA4Frequency);
    //            string guessedString = DetectStringFromFrequency(freq);
    //            OnNoteDetected?.Invoke($"{noteInfo.Note} ({noteInfo.Cents:+#;-#;0} cents)");
    //        }
    //    });
    //}


    public async Task Start()
    {
        if (_tunedSound == null)
        {
            _soundPlayer.PlayTunedSound();
            //var audioManager = AudioManager.Current;
            //var stream = await FileSystem.OpenAppPackageFileAsync("tuned.mp3");
            //var player = AudioManager.Current.CreatePlayer(stream);
            //player.Play();

            //_tunedSound = audioManager.CreatePlayer(stream);
        }

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
                var target = GuitarTunings.Tunings[_currentTuning][_targetString.Value];
                double adjustedFrequency = target.Frequency * (_referenceA4Frequency / 440.0);
                int cents = (int)(1200 * Math.Log2(freq / adjustedFrequency));

                // 🎯 Se afinado corretamente (dentro de ±5 cents), toca som
                Console.WriteLine($"[DEBUG] Freq: {freq:F2} | Target: {adjustedFrequency:F2} | Cents: {cents}");

                if (Math.Abs(cents) <= 5)
                {
                    _soundPlayer.PlayTunedSound();
                    
                }


                string note = NoteDetector.GetNoteInfo(freq, _referenceA4Frequency).Note;
                OnNoteDetected?.Invoke($"{note} ({cents:+#;-#;0} cents)");
            }
            else
            {
                var noteInfo = NoteDetector.GetNoteInfo(freq, _referenceA4Frequency);
                OnNoteDetected?.Invoke($"{noteInfo.Note} ({noteInfo.Cents:+#;-#;0} cents)");
            }
        });
    }


    private string DetectStringFromFrequency(double freq)
    {
        var strings = GuitarTunings.Tunings[_currentTuning];

        // Ordena as cordas pela menor diferença de frequência ajustada e retorna a corda mais próxima
        return strings
            .OrderBy(pair => Math.Abs((pair.Value.Frequency * (_referenceA4Frequency / 440.0)) - freq))
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
