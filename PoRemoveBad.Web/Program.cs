using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoRemoveBad.Core.Services;
using PoRemoveBad.Web;
using PoRemoveBad.Web.Services;

/// <summary>
/// Entry point for the Blazor WebAssembly application.
/// </summary>
public class Program
{
    /// <summary>
    /// Main method to configure and run the Blazor WebAssembly application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Configure enhanced HttpClient with better performance
        builder.Services.AddHttpClient<IAdvancedTextAnalysisService, AdvancedTextAnalysisService>(client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Register the TextProcessingService
        builder.Services.AddScoped<ITextProcessingService, TextProcessingService>();

        // Register the ExportService
        builder.Services.AddScoped<IExportService, ExportService>();

        // Register the ToastService as a singleton
        builder.Services.AddSingleton<ToastService>();

        // Configure enhanced logging
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("System", LogLevel.Warning);

        await builder.Build().RunAsync();
    }
}
