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
                NoteLabel.Text = note;

                // Parse: nota + cents
                var match = Regex.Match(note, @"([A-G]#?)\s*\(([-+0-9]+)\s*cents\)");
                if (match.Success)
                {
                    int cents = int.Parse(match.Groups[2].Value);
                    UpdateTuningBar(cents);
                }
            });
        };

    }
    private void UpdateTuningBar(int cents)
    {
        // Limite visual entre -50 e +50
        int clampedCents = Math.Max(-50, Math.Min(50, cents));

        // Evita erro se largura ainda não foi definida (antes do layout renderizar)
        if (TuningBarBackground.Width <= 0)
            return;

        double percent = (clampedCents + 50) / 100.0;
        double barWidth = TuningBarBackground.Width;
        double x = percent * barWidth;

        // Atualiza posição do ponteiro
        TuningIndicator.TranslationX = x - (TuningIndicator.Width / 2);

        // Atualiza cor: Verde perto de 0, Vermelho nas pontas
        double abs = Math.Abs(cents);
        TuningIndicator.Color = abs < 5 ? Colors.Green
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