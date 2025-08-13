using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoRemoveBad.Core.HealthChecks;
using PoRemoveBad.Core.Services;

namespace PoRemoveBad.Core;

/// <summary>
/// Provides extension methods for registering core services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="tableStorageConnectionString">The Azure Table Storage connection string.</param>
    /// <returns>The <see cref="IServiceCollection"/> with the core services added.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services, string tableStorageConnectionString)
    {
        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IExportService, ExportService>();
        
        // Only register health checks if we have a valid connection string
        if (!string.IsNullOrEmpty(tableStorageConnectionString))
        {
            services.AddHealthChecks()
                .Add(new HealthCheckRegistration(
                    "TableStorage",
                    sp => new TableStorageHealthCheck(tableStorageConnectionString, "PoRemoveBadWords"),
                    HealthStatus.Unhealthy,
                    new[] { "azure", "storage" }));
        }

        return services;
    }
}
