using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PoRemoveBad.Core;
using PoRemoveBad.Core.Services;
using PoRemoveBad.Client;
using PoRemoveBad.Client.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Entry point for the Blazor WebAssembly client application.
/// This client is hosted by the PoRemoveBad.Server project.
/// </summary>
public class Program
{
    /// <summary>
    /// Main method to configure and run the Blazor WebAssembly client application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Configure HttpClient to communicate with the hosting server
        builder.Services.AddScoped(sp => new HttpClient 
        { 
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
        });

        // Configure enhanced HttpClient with better performance for API calls
        builder.Services.AddHttpClient<IAdvancedTextAnalysisService, AdvancedTextAnalysisService>(client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Register Core services
        builder.Services.AddCoreServices();

        // Register the ToastService as a singleton
        builder.Services.AddSingleton<ToastService>();

        // Configure enhanced logging for client-side debugging
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("System", LogLevel.Warning);

        // Note: Integrity validation is disabled in index.html with custom Blazor.start()
        // configuration for .NET 9.0 compatibility

        // Create and run the WebAssembly host
        var host = builder.Build();
        await host.RunAsync();
    }
}
