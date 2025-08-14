using System.Net; // Added for WebUtility
using System.Text;
using System.IO.Compression;
using System.Text.Json;
using PoRemoveBad.Core.Models;
using Microsoft.Extensions.Logging;

namespace PoRemoveBad.Core.Services;

/// <summary>
/// Service for exporting processed text and statistics to various file formats.
/// </summary>
public class ExportService : IExportService
{
    private static readonly string[] SupportedFormats = { "txt", "html", "json", "csv", "xml", "md", "pdf" };
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
    /// <param name="format">The format for the export (optional).</param>
    /// <returns>The default file name.</returns>
    public string GetDefaultFileName(string format = "txt") =>
        $"cleaned_text_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToLower()}";

    /// <summary>
    /// Gets the file name for a specific format with timestamp.
    /// </summary>
    /// <param name="format">The format for the export.</param>
    /// <param name="customName">Custom name prefix (optional).</param>
    /// <returns>The formatted file name.</returns>
    public string GetFormattedFileName(string format, string? customName = null)
    {
        var prefix = !string.IsNullOrWhiteSpace(customName) ? customName : "cleaned_text";
        return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToLower()}";
    }

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
                    "csv" => GenerateCsvContent(processedText, statistics),
                    "xml" => GenerateXmlContent(processedText, statistics),
                    "md" => GenerateMarkdownContent(processedText, statistics),
                    "pdf" => GeneratePdfContent(processedText, statistics),
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

    private static string GenerateCsvContent(string processedText, TextStatistics statistics)
    {
        var sb = new StringBuilder();
        
        // CSV Header
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"\"Total Words\",{statistics.TotalWords}");
        sb.AppendLine($"\"Total Characters\",{statistics.TotalCharacters}");
        sb.AppendLine($"\"Replaced Words\",{statistics.ReplacedWordsCount}");
        sb.AppendLine($"\"Sentences\",{statistics.SentenceCount}");
        sb.AppendLine($"\"Paragraphs\",{statistics.ParagraphCount}");
        sb.AppendLine($"\"Reading Time (minutes)\",{statistics.ReadingTimeMinutes:F1}");
        sb.AppendLine($"\"Readability Score\",{statistics.ReadabilityScore:F1}");
        
        if (statistics.ReplacementFrequency.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Replaced Word,Frequency");
            foreach (var word in statistics.ReplacementFrequency.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"\"{word.Key}\",{word.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Processed Text");
        // Escape quotes in CSV content
        var escapedText = processedText.Replace("\"", "\"\"");
        sb.AppendLine($"\"{escapedText}\"");

        return sb.ToString();
    }

    private static string GenerateXmlContent(string processedText, TextStatistics statistics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<textAnalysis>");
        
        sb.AppendLine("  <processedText><![CDATA[");
        sb.AppendLine(processedText);
        sb.AppendLine("  ]]></processedText>");
        
        sb.AppendLine("  <statistics>");
        sb.AppendLine($"    <totalWords>{statistics.TotalWords}</totalWords>");
        sb.AppendLine($"    <totalCharacters>{statistics.TotalCharacters}</totalCharacters>");
        sb.AppendLine($"    <replacedWords>{statistics.ReplacedWordsCount}</replacedWords>");
        sb.AppendLine($"    <sentences>{statistics.SentenceCount}</sentences>");
        sb.AppendLine($"    <paragraphs>{statistics.ParagraphCount}</paragraphs>");
        sb.AppendLine($"    <readingTimeMinutes>{statistics.ReadingTimeMinutes:F1}</readingTimeMinutes>");
        sb.AppendLine($"    <readabilityScore>{statistics.ReadabilityScore:F1}</readabilityScore>");
        
        if (statistics.ReplacementFrequency.Any())
        {
            sb.AppendLine("    <replacements>");
            foreach (var word in statistics.ReplacementFrequency.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"      <replacement word=\"{WebUtility.HtmlEncode(word.Key)}\" frequency=\"{word.Value}\" />");
            }
            sb.AppendLine("    </replacements>");
        }
        
        sb.AppendLine("  </statistics>");
        sb.AppendLine("</textAnalysis>");

        return sb.ToString();
    }

    private static string GenerateMarkdownContent(string processedText, TextStatistics statistics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Text Analysis Report");
        sb.AppendLine();
        sb.AppendLine($"*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
        sb.AppendLine();
        
        sb.AppendLine("## Processed Text");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine(processedText);
        sb.AppendLine("```");
        sb.AppendLine();
        
        sb.AppendLine("## Statistics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Total Words | {statistics.TotalWords} |");
        sb.AppendLine($"| Total Characters | {statistics.TotalCharacters} |");
        sb.AppendLine($"| Replaced Words | {statistics.ReplacedWordsCount} |");
        sb.AppendLine($"| Sentences | {statistics.SentenceCount} |");
        sb.AppendLine($"| Paragraphs | {statistics.ParagraphCount} |");
        sb.AppendLine($"| Reading Time | {statistics.ReadingTimeMinutes:F1} minutes |");
        sb.AppendLine($"| Readability Score | {statistics.ReadabilityScore:F1} |");
        sb.AppendLine();

        if (statistics.ReplacementFrequency.Any())
        {
            sb.AppendLine("## Top Replaced Words");
            sb.AppendLine();
            sb.AppendLine("| Word | Frequency |");
            sb.AppendLine("|------|-----------|");
            foreach (var word in statistics.ReplacementFrequency.OrderByDescending(x => x.Value).Take(10))
            {
                sb.AppendLine($"| {word.Key} | {word.Value} |");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GeneratePdfContent(string processedText, TextStatistics statistics)
    {
        // For PDF generation, we'll return HTML content that can be converted to PDF
        // In a real implementation, you might use a library like iTextSharp or PuppeteerSharp
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<title>Text Analysis Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 40px 20px; line-height: 1.6; }");
        sb.AppendLine("h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }");
        sb.AppendLine("h2 { color: #34495e; margin-top: 30px; }");
        sb.AppendLine(".stats { background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }");
        sb.AppendLine(".stats table { width: 100%; border-collapse: collapse; }");
        sb.AppendLine(".stats th, .stats td { padding: 8px 12px; text-align: left; border-bottom: 1px solid #dee2e6; }");
        sb.AppendLine(".stats th { background-color: #e9ecef; font-weight: bold; }");
        sb.AppendLine(".processed-text { background: #ffffff; border: 1px solid #dee2e6; border-radius: 5px; padding: 20px; margin: 20px 0; white-space: pre-wrap; font-family: 'Consolas', monospace; }");
        sb.AppendLine("@media print { body { margin: 0; padding: 20px; } }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("<h1>Text Analysis Report</h1>");
        sb.AppendLine($"<p><em>Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</em></p>");

        sb.AppendLine("<h2>Processed Text</h2>");
        var encodedProcessedText = WebUtility.HtmlEncode(processedText);
        sb.AppendLine($"<div class='processed-text'>{encodedProcessedText}</div>");

        sb.AppendLine("<h2>Analysis Statistics</h2>");
        sb.AppendLine("<div class='stats'>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Metric</th><th>Value</th></tr>");
        sb.AppendLine($"<tr><td>Total Words</td><td>{statistics.TotalWords}</td></tr>");
        sb.AppendLine($"<tr><td>Total Characters</td><td>{statistics.TotalCharacters}</td></tr>");
        sb.AppendLine($"<tr><td>Replaced Words</td><td>{statistics.ReplacedWordsCount}</td></tr>");
        sb.AppendLine($"<tr><td>Sentences</td><td>{statistics.SentenceCount}</td></tr>");
        sb.AppendLine($"<tr><td>Paragraphs</td><td>{statistics.ParagraphCount}</td></tr>");
        sb.AppendLine($"<tr><td>Reading Time</td><td>{statistics.ReadingTimeMinutes:F1} minutes</td></tr>");
        sb.AppendLine($"<tr><td>Readability Score</td><td>{statistics.ReadabilityScore:F1}</td></tr>");
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");

        if (statistics.ReplacementFrequency.Any())
        {
            sb.AppendLine("<h2>Replacement Summary</h2>");
            sb.AppendLine("<div class='stats'>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Replaced Word</th><th>Frequency</th></tr>");
            foreach (var word in statistics.ReplacementFrequency.OrderByDescending(x => x.Value).Take(10))
            {
                sb.AppendLine($"<tr><td>{WebUtility.HtmlEncode(word.Key)}</td><td>{word.Value}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Exports multiple texts to a zip file containing all formats.
    /// </summary>
    /// <param name="exports">List of export data containing text and statistics.</param>
    /// <param name="formats">Formats to include in the batch export.</param>
    /// <returns>A zip file as byte array.</returns>
    public async Task<byte[]> ExportBatchToZipAsync(List<(string ProcessedText, TextStatistics Statistics, string Name)> exports, IEnumerable<string> formats)
    {
        _logger.LogInformation("Starting batch export for {ExportCount} texts in {FormatCount} formats", exports.Count, formats.Count());

        try
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var export in exports)
                {
                    foreach (var format in formats)
                    {
                        if (!SupportedFormats.Contains(format.ToLower()))
                        {
                            _logger.LogWarning("Skipping unsupported format: {Format}", format);
                            continue;
                        }

                        var content = await ExportToFileAsync(export.ProcessedText, export.Statistics, format);
                        var fileName = $"{export.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToLower()}";
                        
                        var entry = archive.CreateEntry(fileName);
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(content);
                    }
                }
            }

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create batch export zip file");
            throw new InvalidOperationException("Failed to create batch export zip file", ex);
        }
    }

    /// <summary>
    /// Gets metadata about an export format.
    /// </summary>
    /// <param name="format">The format to get metadata for.</param>
    /// <returns>Export format metadata.</returns>
    public ExportFormatMetadata GetFormatMetadata(string format)
    {
        return format.ToLower() switch
        {
            "txt" => new ExportFormatMetadata { Format = "txt", DisplayName = "Plain Text", MimeType = "text/plain", Description = "Simple text format with basic formatting" },
            "html" => new ExportFormatMetadata { Format = "html", DisplayName = "HTML Document", MimeType = "text/html", Description = "Styled HTML document with statistics" },
            "json" => new ExportFormatMetadata { Format = "json", DisplayName = "JSON Data", MimeType = "application/json", Description = "Structured JSON data for API integration" },
            "csv" => new ExportFormatMetadata { Format = "csv", DisplayName = "CSV Spreadsheet", MimeType = "text/csv", Description = "Comma-separated values for spreadsheet applications" },
            "xml" => new ExportFormatMetadata { Format = "xml", DisplayName = "XML Document", MimeType = "application/xml", Description = "Structured XML format for data exchange" },
            "md" => new ExportFormatMetadata { Format = "md", DisplayName = "Markdown", MimeType = "text/markdown", Description = "Markdown format with tables and formatting" },
            "pdf" => new ExportFormatMetadata { Format = "pdf", DisplayName = "PDF Document", MimeType = "application/pdf", Description = "Professional PDF report (HTML-based)" },
            _ => throw new ArgumentException($"Unknown format: {format}")
        };
    }
}

/// <summary>
/// Metadata about an export format.
/// </summary>
public class ExportFormatMetadata
{
    public string Format { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
