using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System.Text.RegularExpressions;
using TunerApp.Services;
using static TunerApp.Services.Enums;

namespace TunerApp;

public partial class MainPage : ContentPage
{
    private readonly AudioCaptureManager _audioManager;
    private TuningType currentTuning = TuningType.Standard;

    private GuitarString? selectedString;

    public MainPage(AudioCaptureManager audioManager)
    {
        InitializeComponent();
        _audioManager = audioManager;

        // Evento de detecção de nota
        _audioManager.OnNoteDetected += note =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DetectedNoteLabel.Text = $"Nota: {note}";

                var match = Regex.Match(note, @"([A-G]#?\d)\s*\(([-+0-9]+)\s*cents\)");
                if (match.Success)
                {
                    int cents = int.Parse(match.Groups[2].Value);
                    UpdateTuningBar(cents);
                }
            });
        };
    }
    private void ToggleTuningList(object sender, EventArgs e)
    {
        TuningOptionsList.IsVisible = !TuningOptionsList.IsVisible;
    }

    private void TuningOptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedTuning)
        {
            SelectedTuningLabel.Text = $"Afinação: {selectedTuning}";
            TuningOptionsList.IsVisible = false;

            if (Enum.TryParse<TuningType>(selectedTuning, out var tuningEnum))
            {
                _audioManager.SetTuning(tuningEnum);
                UpdateStringButtons(tuningEnum);
            }
        }
    }


    //private void TuningPicker_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    if (TuningPicker.SelectedIndex == -1)
    //        return;

    //    string selectedTuning = TuningPicker.Items[TuningPicker.SelectedIndex];
    //    SelectedTuningLabel.Text = $"Afinação: {selectedTuning}";
    //    TuningPicker.IsVisible = false;

    //    // Atualize a afinação no AudioCaptureManager
    //    var tuningEnum = Enum.Parse<TuningType>(selectedTuning.Replace(" ", ""));
    //    _audioManager.SetTuning(tuningEnum);

    //    // Atualiza botões de cordas com base na afinação selecionada
    //    UpdateStringButtons(tuningEnum);
    //}

    //private void ToggleTuningPicker(object sender, EventArgs e)
    //{
    //    TuningPicker.IsVisible = !TuningPicker.IsVisible;
    //}



    private void UpdateStringButtons(TuningType tuning)
    {
        var tuningMap = GuitarTunings.Tunings[tuning];

        ButtonE6.Text = $"6ª {tuningMap[GuitarString.E6].Note}";
        ButtonA5.Text = $"5ª {tuningMap[GuitarString.A5].Note}";
        ButtonD4.Text = $"4ª {tuningMap[GuitarString.D4].Note}";
        ButtonG3.Text = $"3ª {tuningMap[GuitarString.G3].Note}";
        ButtonB2.Text = $"2ª {tuningMap[GuitarString.B2].Note}";
        ButtonE1.Text = $"1ª {tuningMap[GuitarString.E1].Note}";
    }


    private async void PluckString_Clicked(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert("Afinador", "Pronto para afinar! Toque uma corda.", "OK");
            await _audioManager.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar afinador: {ex.Message}");
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    private void OnSelectString(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Button button &&
            Enum.TryParse<GuitarString>(button.CommandParameter?.ToString(), out var selected))
        {
            selectedString = selected;
            var tuning = GuitarTunings.Tunings[currentTuning];
            var (note, freq) = tuning[selected];
            ReferencePitchLabel.Text = $"Afine para: {note} ({freq} Hz)";
        }
    }


    private void UpdateTuningBar(int cents)
    {
        int clampedCents = Math.Max(-50, Math.Min(50, cents));

        if (TuningBarGrid.Width <= 0)
            return;

        double percent = (clampedCents + 50) / 100.0;
        int targetColumn = (int)Math.Round(percent * (TuningBarGrid.ColumnDefinitions.Count - 1));
        Grid.SetColumn(TuningIndicator, targetColumn);

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
        _audioManager.Stop();
    }

    // Enum das cordas
    public enum GuitarString
    {
        E6, A5, D4, G3, B2, E1
    }

    // Dicionário de afinação padrão
    public static class GuitarTuning
    {
        public static readonly Dictionary<GuitarString, (string Note, double Frequency)> StandardTuning = new()
        {
            { GuitarString.E6, ("E2", 82.41) },
            { GuitarString.A5, ("A2", 110.00) },
            { GuitarString.D4, ("D3", 146.83) },
            { GuitarString.G3, ("G3", 196.00) },
            { GuitarString.B2, ("B3", 246.94) },
            { GuitarString.E1, ("E4", 329.63) },
        };
    }
}
