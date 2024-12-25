using Microsoft.Extensions.DependencyInjection;
using PoRemoveBad.Core.Services;

namespace PoRemoveBad.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IExportService, ExportService>();
        
        return services;
    }
} 