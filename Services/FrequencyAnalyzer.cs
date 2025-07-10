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

    //public double DetectFrequency(float[] samples, int sampleRate)
    //{
    //    if (samples.Length < _fftSize)
    //        Array.Resize(ref samples, _fftSize);

    //    var spectrum = new float[_fftSize / 2 + 1];
    //    _fft.PowerSpectrum(samples, spectrum);

    //    int maxIndex = spectrum
    //        .Select((val, idx) => new { val, idx })
    //        .OrderByDescending(x => x.val)
    //        .First().idx;

    //    double frequency = (double)maxIndex * sampleRate / _fftSize;
    //    return frequency;
    //}
    public double DetectFrequency(float[] samples, int sampleRate)
    {
        // Aqui vai seu algoritmo (autocorrelação, FFT, etc)
        // Exemplo simples de autocorrelação:

        int size = samples.Length;
        double maxCorrelation = 0;
        int bestLag = 0;

        for (int lag = 20; lag < size / 2; lag++)
        {
            double correlation = 0;

            for (int i = 0; i < size - lag; i++)
            {
                correlation += samples[i] * samples[i + lag];
            }

            if (correlation > maxCorrelation)
            {
                maxCorrelation = correlation;
                bestLag = lag;
            }
        }

        if (bestLag == 0)
            return 0;

        return sampleRate / (double)bestLag;
    }

}
