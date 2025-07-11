using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TunerApp.MainPage;
using static TunerApp.Services.Enums;

namespace TunerApp.Services
{
    public static class GuitarTunings
    {
        public static readonly Dictionary<TuningType, Dictionary<GuitarString, (string Note, double Frequency)>> Tunings =
            new()
            {
            {
                TuningType.Standard, new()
                {
                    { GuitarString.E6, ("E2", 82.41) },
                    { GuitarString.A5, ("A2", 110.00) },
                    { GuitarString.D4, ("D3", 146.83) },
                    { GuitarString.G3, ("G3", 196.00) },
                    { GuitarString.B2, ("B3", 246.94) },
                    { GuitarString.E1, ("E4", 329.63) },
                }
            },
            {
                TuningType.HalfStepDown, new()
                {
                    { GuitarString.E6, ("Eb2", 77.78) },
                    { GuitarString.A5, ("Ab2", 103.83) },
                    { GuitarString.D4, ("Db3", 138.59) },
                    { GuitarString.G3, ("Gb3", 185.00) },
                    { GuitarString.B2, ("Bb3", 233.08) },
                    { GuitarString.E1, ("Eb4", 311.13) },
                }
            },
            {
                TuningType.OpenD, new()
                {
                    { GuitarString.E6, ("D2", 73.42) },
                    { GuitarString.A5, ("A2", 110.00) },
                    { GuitarString.D4, ("D3", 146.83) },
                    { GuitarString.G3, ("F#3", 185.00) },
                    { GuitarString.B2, ("A3", 220.00) },
                    { GuitarString.E1, ("D4", 293.66) },
                }
            },
            {
                TuningType.OpenG, new()
                {
                    { GuitarString.E6, ("D2", 73.42) },
                    { GuitarString.A5, ("G2", 98.00) },
                    { GuitarString.D4, ("D3", 146.83) },
                    { GuitarString.G3, ("G3", 196.00) },
                    { GuitarString.B2, ("B3", 246.94) },
                    { GuitarString.E1, ("D4", 293.66) },
                }
            }
            };
    }

}
