public class NoteInfo
{
    public string Note { get; set; } = "";
    public int Cents { get; set; }
    public int Octave { get; set; }
    public double Frequency { get; set; }
}

public static class NoteDetector
{
    private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private const double A4Frequency = 440.0;
    private const int A4Index = 69; // MIDI index

    public static NoteInfo GetNoteInfo(double frequency)
    {
        if (frequency <= 0)
            return new NoteInfo { Note = "?" };

        // Calcular o número da nota baseado no padrão MIDI (A4 = 69)
        double noteNumber = 69 + 12 * Math.Log2(frequency / A4Frequency);
        int roundedNoteNumber = (int)Math.Round(noteNumber);

        //int noteIndex = (roundedNoteNumber + 3) % 12; // +3 para alinhar com C=0
        int noteIndex = roundedNoteNumber % 12;

        int octave = (roundedNoteNumber / 12) - 1;

        string noteName = NoteNames[noteIndex];
        int cents = (int)Math.Round((noteNumber - roundedNoteNumber) * 100);

        return new NoteInfo
        {
            Note = $"{noteName}{octave}",
            Octave = octave,
            Cents = cents,
            Frequency = frequency
        };
    }
}
