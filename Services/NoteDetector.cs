using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunerApp.Services
{
    public static class NoteDetector
    {
        static readonly string[] Notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string GetNoteFromFrequency(double frequency)
        {
            if (frequency <= 0) return "Sem sinal";

            int noteNumber = (int)Math.Round(12 * Math.Log2(frequency / 440.0)) + 69;
            int octave = noteNumber / 12 - 1;
            string note = Notes[noteNumber % 12];
            return $"{note}{octave}";
        }
    }
}
