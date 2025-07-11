using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TunerApp.MainPage;

namespace TunerApp.Services
{
    public class CustomTuning
    {
        private readonly Dictionary<GuitarString, double> _baseFrequencies;
        private readonly Dictionary<GuitarString, double> _adjustedFrequencies;
        private double _referenceA4Frequency;

        public CustomTuning(Dictionary<GuitarString, double> baseFrequencies, double referenceA4Frequency = 440.0)
        {
            _baseFrequencies = baseFrequencies;
            _adjustedFrequencies = new Dictionary<GuitarString, double>();
            _referenceA4Frequency = referenceA4Frequency;
            UpdateAdjustedFrequencies();
        }

        public double ReferenceA4Frequency
        {
            get => _referenceA4Frequency;
            set
            {
                _referenceA4Frequency = value;
                UpdateAdjustedFrequencies();
            }
        }

        private void UpdateAdjustedFrequencies()
        {
            foreach (var kv in _baseFrequencies)
            {
                _adjustedFrequencies[kv.Key] = kv.Value * (_referenceA4Frequency / 440.0);
            }
        }

        public double GetFrequency(GuitarString stringName)
        {
            return _adjustedFrequencies[stringName];
        }
    }

}
