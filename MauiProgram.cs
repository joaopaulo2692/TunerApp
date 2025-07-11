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
        builder.Services.AddSingleton<ISoundPlayer>(provider =>
        {
#if ANDROID
            return new TunerApp.Platforms.Android.SoundPlayer();
#else
            throw new NotImplementedException("ISoundPlayer não implementado nesta plataforma.");
#endif
        });

        return builder.Build();
    }
}