using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoRemoveBad.Core;
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

        builder.Services.AddCoreServices();
        builder.Services.AddSingleton<ToastService>();

        await builder.Build().RunAsync();
    }
}
