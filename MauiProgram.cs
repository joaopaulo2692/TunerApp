using TunerApp;
using TunerApp.Platforms.Android.Services;
using TunerApp.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register services
#if ANDROID
        builder.Services.AddSingleton<IMicrophoneService, MicrophoneService>();
#endif

        builder.Services.AddSingleton<AudioCaptureManager>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}