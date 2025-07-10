using Microsoft.Maui.Controls;
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
            });
        };
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