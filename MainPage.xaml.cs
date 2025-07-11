using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;
using TunerApp.Services;
using static TunerApp.Services.Enums;

namespace TunerApp;

public partial class MainPage : ContentPage
{
    private readonly AudioCaptureManager _audioManager;

    private TuningType currentTuning = TuningType.Standard;
    private double _referencePitch = 440.0;
    private bool _userChangedReferencePitch = false;
    private GuitarString? selectedString;


    public MainPage(AudioCaptureManager audioManager)
    {
        Title = "Afinador";

        InitializeComponent();

        _audioManager = audioManager;

        // Inicializa referência A4
        _audioManager.ReferenceA4Frequency = _referencePitch;
        PitchSlider.Value = _referencePitch;
        ReferencePitchLabel.Text = $"A: {_referencePitch} Hz";

        // Assina evento de nota detectada
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

        // Inicializa botões das cordas conforme afinação padrão
        UpdateStringButtons(currentTuning);
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
                currentTuning = tuningEnum;
                _audioManager.SetTuning(currentTuning);
                UpdateStringButtons(currentTuning);
            }
        }
    }

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
            TuneButton.IsVisible = false;
            TuneButton.Text = "Afinando...";
            await DisplayAlert("Afinador", "Pronto para afinar! Toque uma corda.", "OK");
            await _audioManager.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar afinador: {ex.Message}");
            await DisplayAlert("Erro", ex.Message, "OK");
            TuneButton.IsVisible = true;
            TuneButton.Text = "AFINAR";
        }
    }

    private void PitchSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        _userChangedReferencePitch = true;
        _referencePitch = Math.Round(e.NewValue, 1);

        ReferencePitchLabel.Text = $"A: {_referencePitch} Hz";
        _audioManager.ReferenceA4Frequency = _referencePitch;

        // Se uma corda já foi selecionada antes, atualize a frequência dela
        if (selectedString.HasValue)
        {
            UpdateSelectedStringLabel();
        }
    }

    private void IncreasePitch_Clicked(object sender, EventArgs e)
    {
        double newValue = Math.Min(PitchSlider.Value + 0.1, PitchSlider.Maximum);
        PitchSlider.Value = Math.Round(newValue, 1); // Vai disparar o ValueChanged
    }

    private void DecreasePitch_Clicked(object sender, EventArgs e)
    {
        double newValue = Math.Max(PitchSlider.Value - 0.1, PitchSlider.Minimum);
        PitchSlider.Value = Math.Round(newValue, 1); // Vai disparar o ValueChanged
    }


    private void UpdateSelectedStringLabel()
    {
        var tuning = GuitarTunings.Tunings[currentTuning];
        var target = tuning[selectedString.Value];

        double freqToShow = target.Frequency;

        if (_userChangedReferencePitch)
        {
            freqToShow *= (_referencePitch / 440.0);
        }

        SelectedStringTargetLabel.Text = $"Afine para: {target.Note} ({freqToShow:F2} Hz)";
    }


    private void OnSelectString(object sender, EventArgs e)
    {
        if (sender is Button button &&
            Enum.TryParse<GuitarString>(button.CommandParameter?.ToString(), out var selected))
        {
            selectedString = selected;

            var tuning = GuitarTunings.Tunings[currentTuning];
            var target = tuning[selected];

            double freqToShow = target.Frequency;

            if (_userChangedReferencePitch)
            {
                freqToShow *= (_referencePitch / 440.0);
            }

            // NÃO altere o ReferencePitchLabel!
            SelectedStringTargetLabel.Text = $"Afine para: {target.Note} ({freqToShow:F2} Hz)";
        }
    }



    private void UpdateTuningBar(int cents)
    {
        int clampedCents = Math.Clamp(cents, -50, 50);

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
        TuneButton.IsVisible = true;
        TuneButton.Text = "AFINAR";
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
}
