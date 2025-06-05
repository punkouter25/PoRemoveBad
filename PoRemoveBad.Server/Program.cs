using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using PoRemoveBad.Core;
using PoRemoveBad.Core.Services;
using Serilog;
using Serilog.Events;

/// <summary>
/// Entry point for the PoRemoveBad Server application.
/// Hosts the Blazor WebAssembly client and provides API endpoints.
/// </summary>
public class Program
{
    /// <summary>
    /// Main method to configure and run the hosted Blazor WebAssembly application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {        // Configure Serilog early to capture startup logs
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "..", "log.txt"), 
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Starting PoRemoveBad Server");

            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog
            builder.Host.UseSerilog();

            // Add services to the container
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Register Core services
            builder.Services.AddCoreServices();

            // Add Application Insights
            var appInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
            if (!string.IsNullOrEmpty(appInsightsConnectionString))
            {
                builder.Services.AddApplicationInsightsTelemetry(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;
                });
                  // Add Application Insights to Serilog
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "..", "log.txt"), 
                        rollingInterval: RollingInterval.Infinite,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.ApplicationInsights(appInsightsConnectionString, TelemetryConverter.Traces)
                    .CreateLogger();
            }

            // Add health checks
            builder.Services.AddHealthChecks();

            // Add CORS for development
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();

            app.MapRazorPages();
            app.MapControllers();
            app.MapHealthChecks("/health");
            app.MapFallbackToFile("index.html");

            Log.Information("PoRemoveBad Server started successfully");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "PoRemoveBad Server failed to start");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
