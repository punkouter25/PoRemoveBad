using Microsoft.Extensions.DependencyInjection;
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
    /// <returns>The <see cref="IServiceCollection"/> with the core services added.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IExportService, ExportService>();
        
        return services;
    }
}
