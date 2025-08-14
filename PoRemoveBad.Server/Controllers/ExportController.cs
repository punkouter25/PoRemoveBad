using Microsoft.AspNetCore.Mvc;
using PoRemoveBad.Core.Models;
using PoRemoveBad.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace PoRemoveBad.Server.Controllers;

/// <summary>
/// Controller for exporting processed text in various formats.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(IExportService exportService, ILogger<ExportController> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the list of supported export formats.
    /// </summary>
    /// <returns>List of supported export formats with metadata.</returns>
    [HttpGet("formats")]
    [ProducesResponseType(typeof(IEnumerable<ExportFormatMetadata>), StatusCodes.Status200OK)]
    public IActionResult GetSupportedFormats()
    {
        try
        {
            var formats = _exportService.GetSupportedFormats()
                .Select(format => _exportService.GetFormatMetadata(format))
                .ToList();

            return Ok(formats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported export formats");
            return StatusCode(500, new { error = "Failed to retrieve supported formats" });
        }
    }

    /// <summary>
    /// Exports processed text to a specific format.
    /// </summary>
    /// <param name="request">Export request containing text, statistics, and format.</param>
    /// <returns>File download with the exported content.</returns>
    [HttpPost("single")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportSingle([FromBody] ExportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var fileBytes = await _exportService.ExportToFileAsync(
                request.ProcessedText, 
                request.Statistics, 
                request.Format);

            var fileName = _exportService.GetFormattedFileName(request.Format, request.CustomName);
            var metadata = _exportService.GetFormatMetadata(request.Format);

            return File(fileBytes, metadata.MimeType, fileName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid export format: {Format}", request.Format);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting file in format: {Format}", request.Format);
            return StatusCode(500, new { error = "Failed to export file" });
        }
    }

    /// <summary>
    /// Exports multiple texts to multiple formats as a ZIP file.
    /// </summary>
    /// <param name="request">Batch export request.</param>
    /// <returns>ZIP file containing all exported formats.</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportBatch([FromBody] BatchExportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!request.Exports.Any())
        {
            return BadRequest(new { error = "At least one export item is required" });
        }

        if (!request.Formats.Any())
        {
            return BadRequest(new { error = "At least one format is required" });
        }

        try
        {
            var exports = request.Exports.Select(e => (e.ProcessedText, e.Statistics, e.Name)).ToList();
            var zipBytes = await _exportService.ExportBatchToZipAsync(exports, request.Formats);

            var fileName = $"batch_export_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            return File(zipBytes, "application/zip", fileName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid export request: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch export");
            return StatusCode(500, new { error = "Failed to create batch export" });
        }
    }

    /// <summary>
    /// Generates a download link for a specific export (placeholder for future blob storage integration).
    /// </summary>
    /// <param name="request">Export request.</param>
    /// <returns>Download URL information.</returns>
    [HttpPost("link")]
    [ProducesResponseType(typeof(ExportLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GenerateExportLink([FromBody] ExportRequest request)
    {
        // This is a placeholder for future blob storage integration
        // For now, return a direct export
        
        try
        {
            var fileBytes = await _exportService.ExportToFileAsync(
                request.ProcessedText, 
                request.Statistics, 
                request.Format);

            var fileName = _exportService.GetFormattedFileName(request.Format, request.CustomName);
            var base64Data = Convert.ToBase64String(fileBytes);
            var metadata = _exportService.GetFormatMetadata(request.Format);

            return Ok(new ExportLinkResponse
            {
                FileName = fileName,
                MimeType = metadata.MimeType,
                DataUrl = $"data:{metadata.MimeType};base64,{base64Data}",
                ExpiresAt = DateTime.UtcNow.AddHours(1), // 1 hour expiry for data URLs
                IsTemporary = true
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid export format: {Format}", request.Format);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating export link for format: {Format}", request.Format);
            return StatusCode(500, new { error = "Failed to generate export link" });
        }
    }
}

/// <summary>
/// Request model for single file export.
/// </summary>
public class ExportRequest
{
    /// <summary>
    /// The processed text to export.
    /// </summary>
    [Required]
    public string ProcessedText { get; set; } = string.Empty;

    /// <summary>
    /// Text processing statistics.
    /// </summary>
    [Required]
    public TextStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Export format (txt, html, json, csv, xml, md, pdf).
    /// </summary>
    [Required]
    public string Format { get; set; } = "txt";

    /// <summary>
    /// Custom name prefix for the exported file (optional).
    /// </summary>
    public string? CustomName { get; set; }
}

/// <summary>
/// Request model for batch export.
/// </summary>
public class BatchExportRequest
{
    /// <summary>
    /// List of export items.
    /// </summary>
    [Required]
    public List<ExportItem> Exports { get; set; } = new();

    /// <summary>
    /// List of formats to export (txt, html, json, csv, xml, md, pdf).
    /// </summary>
    [Required]
    public List<string> Formats { get; set; } = new();
}

/// <summary>
/// Individual export item for batch processing.
/// </summary>
public class ExportItem
{
    /// <summary>
    /// The processed text to export.
    /// </summary>
    [Required]
    public string ProcessedText { get; set; } = string.Empty;

    /// <summary>
    /// Text processing statistics.
    /// </summary>
    [Required]
    public TextStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Name for this export item.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Response model for export link generation.
/// </summary>
public class ExportLinkResponse
{
    /// <summary>
    /// File name for the export.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the exported file.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Download URL or data URL for the export.
    /// </summary>
    public string DataUrl { get; set; } = string.Empty;

    /// <summary>
    /// Expiration time for the download link.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indicates if this is a temporary link.
    /// </summary>
    public bool IsTemporary { get; set; }
}
