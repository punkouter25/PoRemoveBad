using System.Net; // Added for WebUtility
using System.Text;
using PoRemoveBad.Core.Models;
using Microsoft.Extensions.Logging;

namespace PoRemoveBad.Core.Services;

/// <summary>
/// Service for exporting processed text and statistics to various file formats.
/// </summary>
public class ExportService : IExportService
{
    private static readonly string[] SupportedFormats = { "txt", "html", "json" };
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the supported file formats for export.
    /// </summary>
    /// <returns>An enumerable of supported file formats.</returns>
    public IEnumerable<string> GetSupportedFormats() => SupportedFormats;

    /// <summary>
    /// Gets the default file name for the exported file.
    /// </summary>
    /// <returns>The default file name.</returns>
    public string GetDefaultFileName() =>
        $"cleaned_text_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

    /// <summary>
    /// Exports the processed text and statistics to a file in the specified format.
    /// </summary>
    /// <param name="processedText">The processed text to export.</param>
    /// <param name="statistics">The text statistics to include in the export.</param>
    /// <param name="format">The file format for the export (default is "txt").</param>
    /// <returns>A task representing the asynchronous operation, with a byte array containing the file content.</returns>
    public async Task<byte[]> ExportToFileAsync(string processedText, TextStatistics statistics, string format = "txt")
    {
        if (!SupportedFormats.Contains(format.ToLower()))
        {
            _logger.LogError("Unsupported format: {Format}", format);
            throw new ArgumentException($"Unsupported format: {format}");
        }

        try
        {
            return await Task.Run(() =>
            {
                var content = format.ToLower() switch
                {
                    "txt" => GenerateTxtContent(processedText, statistics),
                    "html" => GenerateHtmlContent(processedText, statistics),
                    "json" => GenerateJsonContent(processedText, statistics),
                    _ => throw new ArgumentException($"Unsupported format: {format}")
                };

                return Encoding.UTF8.GetBytes(content);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export file in format: {Format}", format);
            throw new InvalidOperationException($"Failed to export file in format: {format}", ex);
        }
    }

    private static string GenerateTxtContent(string processedText, TextStatistics statistics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Cleaned Text ===");
        sb.AppendLine();
        sb.AppendLine(processedText);
        sb.AppendLine();
        sb.AppendLine("=== Statistics ===");
        sb.AppendLine($"Total Words: {statistics.TotalWords}");
        sb.AppendLine($"Total Characters: {statistics.TotalCharacters}");
        sb.AppendLine($"Replaced Words: {statistics.ReplacedWordsCount}");
        sb.AppendLine($"Sentences: {statistics.SentenceCount}");
        sb.AppendLine($"Paragraphs: {statistics.ParagraphCount}");

        if (statistics.ReplacementFrequency.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Top 5 Replaced Words:");
            foreach (var word in statistics.ReplacementFrequency.OrderByDescending(x => x.Value).Take(5))
            {
                sb.AppendLine($"- {word.Key}: {word.Value} times");
            }
        }

        return sb.ToString();
    }

    private static string GenerateHtmlContent(string processedText, TextStatistics statistics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<title>Cleaned Text</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }");
        sb.AppendLine("h1, h2 { color: #333; }");
        sb.AppendLine(".stats { background: #f5f5f5; padding: 15px; border-radius: 5px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("<h1>Cleaned Text</h1>");
        // Encode the processed text before inserting into HTML to prevent errors
        var encodedProcessedText = WebUtility.HtmlEncode(processedText);
        sb.AppendLine($"<div style='white-space: pre-wrap;'>{encodedProcessedText}</div>");

        sb.AppendLine("<h2>Statistics</h2>");
        sb.AppendLine("<div class='stats'>");
        sb.AppendLine($"<p>Total Words: {statistics.TotalWords}</p>");
        sb.AppendLine($"<p>Total Characters: {statistics.TotalCharacters}</p>");
        sb.AppendLine($"<p>Replaced Words: {statistics.ReplacedWordsCount}</p>");
        sb.AppendLine($"<p>Sentences: {statistics.SentenceCount}</p>");
        sb.AppendLine($"<p>Paragraphs: {statistics.ParagraphCount}</p>");

        if (statistics.ReplacementFrequency.Any())
        {
            sb.AppendLine("<h3>Top 5 Replaced Words</h3>");
            sb.AppendLine("<ul>");
            foreach (var word in statistics.ReplacementFrequency.OrderByDescending(x => x.Value).Take(5))
            {
                sb.AppendLine($"<li>{word.Key}: {word.Value} times</li>");
            }
            sb.AppendLine("</ul>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string GenerateJsonContent(string processedText, TextStatistics statistics)
    {
        var exportObject = new
        {
            processedText,
            statistics = new
            {
                totalWords = statistics.TotalWords,
                totalCharacters = statistics.TotalCharacters,
                replacedWords = statistics.ReplacedWordsCount,
                sentences = statistics.SentenceCount,
                paragraphs = statistics.ParagraphCount,
                topReplacements = statistics.ReplacementFrequency
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .Select(x => new { word = x.Key, count = x.Value })
                    .ToList(),
                graphData = statistics.GraphData
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(exportObject, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
