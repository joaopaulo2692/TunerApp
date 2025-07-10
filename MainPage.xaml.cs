using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;
using TunerApp.Services;

namespace TunerApp;

public partial class MainPage : ContentPage
{
    private readonly AudioCaptureManager _audioManager;

    // Inject AudioCaptureManager through constructor
    public MainPage(AudioCaptureManager audioManager)
    {
        InitializeComponent();
        _audioManager = audioManager;

        _audioManager.OnNoteDetected += note =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Atualiza a nota na UI
                DetectedNoteLabel.Text = $"Nota: {note}";

                // Extrai o valor em cents (caso queira usar a barra)
                var match = Regex.Match(note, @"([A-G]#?)\s*\(([-+0-9]+)\s*cents\)");
                if (match.Success)
                {
                    int cents = int.Parse(match.Groups[2].Value);
                    UpdateTuningBar(cents);
                }
            });
        };


    }
    private async void PluckString_Clicked(object sender, EventArgs e)
    {
        try
        {
            DisplayAlert("Afinador", "Pronto para afinar! Toque uma corda.", "OK");
            await _audioManager.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar afinador: {ex.Message}");
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    //private void PluckString_Clicked(object sender, EventArgs e)
    //{
    //    // This is where you put the logic for when the "PLUCK A STRING" button is clicked.
    //    // For example, you might want to start the tuner here, or display a message.
       

    //    // You might also want to start the audio manager if it's not already running
    //    // If your original "StartTuner_Clicked" button is being replaced, you can move its logic here:
    //    // try
    //    // {
    //    //     await _audioManager.Start();
    //    // }
    //    // catch (Exception ex)
    //    // {
    //    //     Console.WriteLine($"Erro ao iniciar afinador: {ex.Message}");
    //    //     DisplayAlert("Erro", ex.Message, "OK");
    //    // }
    //}

    private void UpdateTuningBar(int cents)
    {
        int clampedCents = Math.Max(-50, Math.Min(50, cents));

        // Use the new TuningBarGrid for width calculation
        // Ensure TuningBarGrid has an x:Name in your XAML.
        // If you used the provided XAML, it already has x:Name="TuningBarGrid"
        if (TuningBarGrid.Width <= 0)
            return;

        double percent = (clampedCents + 50) / 100.0;
        double barWidth = TuningBarGrid.Width; // Reference the new Grid's width

        // The TranslationX calculation needs to be adjusted because the indicator is now inside a Grid
        // and its position is relative to its parent cell or the grid itself.
        // A simpler way for a visual bar might be to directly set its Grid.Column.
        // However, if you want smooth movement, TranslationX on the entire grid or a container within it
        // that holds the indicator might be more appropriate.
        // For now, let's keep the TranslationX logic but be aware it might need fine-tuning.

        // Calculate the target X position within the TuningBarGrid
        // The TuningIndicator is a specific column. We need to figure out which column it should be in.
        // Assuming 25 columns in TuningBarGrid (0 to 24)
        int targetColumn = (int)Math.Round(percent * (TuningBarGrid.ColumnDefinitions.Count - 1));
        Grid.SetColumn(TuningIndicator, targetColumn);

        // Optional: If you want finer movement within a column, you'd need a more complex setup,
        // potentially by placing the TuningIndicator inside an AbsoluteLayout within a single Grid.Column,
        // or by calculating its TranslationX very precisely relative to the Grid's total width
        // while it's in a single column span across the grid.
        // For a segmented bar as in the image, moving the column might be enough.
        // Let's remove the TranslationX on the indicator directly if we are setting Grid.Column.
        // TuningIndicator.TranslationX = x - (TuningIndicator.Width / 2); // Remove or re-evaluate this line

        double abs = Math.Abs(cents);
        TuningIndicator.Color = abs < 5 ? Colors.LimeGreen
                                     : abs < 15 ? Colors.Orange
                                                : Colors.Red;
    }



    private async void StartTuner_Clicked(object sender, EventArgs e)
    {
        try
        {
            await _audioManager.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar afinador: {ex.Message}");
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }


    private void StopTuner_Clicked(object sender, EventArgs e)
    {
        _audioManager.Stop();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _audioManager.Stop(); // Ensure we stop when page disappears
    }
}