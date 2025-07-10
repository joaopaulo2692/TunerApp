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

    private void StartTuner_Clicked(object sender, EventArgs e)
    {
        _audioManager.Start();
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