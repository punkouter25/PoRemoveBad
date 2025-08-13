using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Core.Services;

public interface IExportService
{
    Task<byte[]> ExportToFileAsync(string processedText, TextStatistics statistics, string format = "txt");
    string GetDefaultFileName();
    IEnumerable<string> GetSupportedFormats();
}