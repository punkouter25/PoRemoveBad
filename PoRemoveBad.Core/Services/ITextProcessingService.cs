using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Core.Services;

public interface ITextProcessingService
{
    Task<(string ProcessedText, TextStatistics Statistics)> ProcessTextAsync(string inputText, IProgress<double>? progress = null);
    Task InitializeDictionaryAsync(string dictionaryType = "default");
    bool IsInitialized { get; }
    string CurrentDictionaryType { get; }
}