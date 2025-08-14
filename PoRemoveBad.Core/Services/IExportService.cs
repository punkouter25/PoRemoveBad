using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Core.Services;

public interface IExportService
{
    Task<byte[]> ExportToFileAsync(string processedText, TextStatistics statistics, string format = "txt");
    string GetDefaultFileName(string format = "txt");
    string GetFormattedFileName(string format, string? customName = null);
    IEnumerable<string> GetSupportedFormats();
    Task<byte[]> ExportBatchToZipAsync(List<(string ProcessedText, TextStatistics Statistics, string Name)> exports, IEnumerable<string> formats);
    ExportFormatMetadata GetFormatMetadata(string format);
}