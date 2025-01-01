using Microsoft.Extensions.Logging;
using PoRemoveBad.Core;
using PoRemoveBad.Core.Services;

namespace PoRemoveBad.Mobile;

/// <summary>
/// Provides methods to create and configure the Maui application.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the Maui application.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Core services
        builder.Services.AddCoreServices();

        // Register app and pages
        builder.Services.AddSingleton<App>();
        builder.Services.AddTransient<MainPage>();

        // Configure logging
        builder.Services.AddLogging(logging =>
        {
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Trace);
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Initialize the dictionary at startup
        var textService = app.Services.GetRequiredService<ITextProcessingService>();
        textService.InitializeDictionaryAsync().GetAwaiter().GetResult();

        return app;
    }
}
