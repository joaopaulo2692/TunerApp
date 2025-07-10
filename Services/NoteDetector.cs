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
        public record NoteInfo(string Note, double Cents, double Frequency);

        public static string GetNoteFromFrequency(double frequency)
        {
            if (frequency <= 0) return "Sem sinal";

            int noteNumber = (int)Math.Round(12 * Math.Log2(frequency / 440.0)) + 69;
            int octave = noteNumber / 12 - 1;
            string note = Notes[noteNumber % 12];
            return $"{note}{octave}";
        }
        public static NoteInfo GetNoteInfo(double frequency)
        {
            double a4 = 440.0;
            double semitones = 12 * Math.Log2(frequency / a4);
            int roundedSemitone = (int)Math.Round(semitones);
            int noteIndex = (roundedSemitone + 9) % 12; // +9 para que A = 0
            string note = Notes[(noteIndex + 12) % 12]; // evita índices negativos

            double noteFreq = a4 * Math.Pow(2, roundedSemitone / 12.0);
            double cents = 1200 * Math.Log2(frequency / noteFreq);

            return new NoteInfo(note, cents, frequency);
        }

    }
}
