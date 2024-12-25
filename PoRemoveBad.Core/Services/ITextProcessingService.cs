using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Core.Services;

public interface ITextProcessingService
{
    Task<(string ProcessedText, TextStatistics Statistics)> ProcessTextAsync(string inputText, IProgress<double>? progress = null);
    Task InitializeDictionaryAsync();
    bool IsInitialized { get; }
} 