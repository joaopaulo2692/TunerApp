using NWaves.Transforms;
using NWaves.Windows;

namespace TunerApp.Services;

public class FrequencyAnalyzer
{
    private readonly RealFft _fft;
    private readonly int _fftSize;

    public FrequencyAnalyzer(int fftSize = 2048)
    {
        _fftSize = fftSize;
        _fft = new RealFft(_fftSize);
    }

    public double DetectFrequency(float[] samples, int sampleRate)
    {
        if (samples.Length < _fftSize)
            Array.Resize(ref samples, _fftSize);

        var spectrum = new float[_fftSize / 2 + 1];
        _fft.PowerSpectrum(samples, spectrum);

        int maxIndex = spectrum
            .Select((val, idx) => new { val, idx })
            .OrderByDescending(x => x.val)
            .First().idx;

        double frequency = (double)maxIndex * sampleRate / _fftSize;
        return frequency;
    }
}
