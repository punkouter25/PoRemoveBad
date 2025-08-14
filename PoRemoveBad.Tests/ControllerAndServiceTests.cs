using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PoRemoveBad.Core.Models;
using PoRemoveBad.Core.Services;
using PoRemoveBad.Web.Controllers;
using PoRemoveBad.Server.Controllers;
using Xunit;
using Microsoft.Extensions.Logging;

namespace PoRemoveBad.Tests;

public class ControllerAndServiceTests
{
    [Fact]
    public async Task TextAnalysisController_AnalyzeText_ServiceThrows_Returns500()
    {
        // Arrange
        var mockAnalysisService = new Mock<IAdvancedTextAnalysisService>();
        mockAnalysisService
            .Setup(s => s.AnalyzeTextAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("boom"));

        var controller = new TextAnalysisController(mockAnalysisService.Object, new NullLogger<TextAnalysisController>());

        // Act
        var result = await controller.AnalyzeText(new TextAnalysisRequest { Text = "test input" });

        // Assert
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TextProcessingService_ProcessTextAsync_WithoutInitialize_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new TextProcessingService(new NullLogger<TextProcessingService>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessTextAsync("some text"));
    }

    [Fact]
    public async Task ExportService_ExportToFileAsync_UnsupportedAndSupportedFormats_BehaveAsExpected()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());

        var stats = new TextStatistics
        {
            TotalWords = 3,
            TotalCharacters = 14,
            ReplacedWordsCount = 1,
            SentenceCount = 1,
            ParagraphCount = 1
        };

        // Unsupported format -> ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(() => exportService.ExportToFileAsync("hello", stats, "unsupported"));

        // TXT format -> contains marker
        var txtBytes = await exportService.ExportToFileAsync("cleaned text", stats, "txt");
        var txtContent = Encoding.UTF8.GetString(txtBytes);
        txtContent.Should().Contain("=== Cleaned Text ===");

        // HTML format -> contains DOCTYPE and encoded content
        var htmlBytes = await exportService.ExportToFileAsync("<b>cleaned</b>", stats, "html");
        var htmlContent = Encoding.UTF8.GetString(htmlBytes);
        htmlContent.Should().Contain("<!DOCTYPE html>");
        // Expect HTML-encoded content produced by ExportService
        htmlContent.Should().Contain("&lt;b&gt;cleaned&lt;/b&gt;");

        // JSON format -> valid JSON string and contains processedText property
        var jsonBytes = await exportService.ExportToFileAsync("cleaned text", stats, "json");
        var jsonContent = Encoding.UTF8.GetString(jsonBytes);
        jsonContent.Should().Contain("\"processedText\"");
        jsonContent.Should().Contain("cleaned text");
    }

    [Fact]
    public async Task ExportService_NewFormats_GenerateCorrectContent()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());
        var stats = new TextStatistics
        {
            TotalWords = 5,
            TotalCharacters = 20,
            ReplacedWordsCount = 2,
            SentenceCount = 1,
            ParagraphCount = 1,
            ReadingTimeMinutes = 0.5,
            ReadabilityScore = 8.5
        };
        stats.ReplacementFrequency.TryAdd("bad", 1);
        stats.ReplacementFrequency.TryAdd("terrible", 1);

        // CSV format -> contains header and data
        var csvBytes = await exportService.ExportToFileAsync("cleaned text", stats, "csv");
        var csvContent = Encoding.UTF8.GetString(csvBytes);
        csvContent.Should().Contain("Metric,Value");
        csvContent.Should().Contain("\"Total Words\",5");
        csvContent.Should().Contain("Replaced Word,Frequency");

        // XML format -> contains XML declaration and CDATA
        var xmlBytes = await exportService.ExportToFileAsync("cleaned text", stats, "xml");
        var xmlContent = Encoding.UTF8.GetString(xmlBytes);
        xmlContent.Should().Contain("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xmlContent.Should().Contain("<textAnalysis>");
        xmlContent.Should().Contain("<![CDATA[");

        // Markdown format -> contains headers and tables
        var mdBytes = await exportService.ExportToFileAsync("cleaned text", stats, "md");
        var mdContent = Encoding.UTF8.GetString(mdBytes);
        mdContent.Should().Contain("# Text Analysis Report");
        mdContent.Should().Contain("| Metric | Value |");
        mdContent.Should().Contain("| Total Words | 5 |");

        // PDF format (HTML-based) -> contains professional styling
        var pdfBytes = await exportService.ExportToFileAsync("cleaned text", stats, "pdf");
        var pdfContent = Encoding.UTF8.GetString(pdfBytes);
        pdfContent.Should().Contain("<!DOCTYPE html>");
        pdfContent.Should().Contain("Text Analysis Report");
        pdfContent.Should().Contain("font-family: 'Segoe UI'");
    }

    [Fact]
    public void ExportService_GetSupportedFormats_IncludesAllFormats()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());

        // Act
        var formats = exportService.GetSupportedFormats().ToList();

        // Assert
        formats.Should().Contain("txt");
        formats.Should().Contain("html");
        formats.Should().Contain("json");
        formats.Should().Contain("csv");
        formats.Should().Contain("xml");
        formats.Should().Contain("md");
        formats.Should().Contain("pdf");
        formats.Should().HaveCount(7);
    }

    [Fact]
    public void ExportService_GetFormattedFileName_GeneratesCorrectNames()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());

        // Act & Assert
        var fileName1 = exportService.GetFormattedFileName("pdf");
        fileName1.Should().StartWith("cleaned_text_");
        fileName1.Should().EndWith(".pdf");

        var fileName2 = exportService.GetFormattedFileName("csv", "custom_name");
        fileName2.Should().StartWith("custom_name_");
        fileName2.Should().EndWith(".csv");
    }

    [Fact]
    public void ExportService_GetFormatMetadata_ReturnsCorrectMetadata()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());

        // Act
        var metadata = exportService.GetFormatMetadata("pdf");

        // Assert
        metadata.Format.Should().Be("pdf");
        metadata.DisplayName.Should().Be("PDF Document");
        metadata.MimeType.Should().Be("application/pdf");
        metadata.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportService_BatchExport_CreatesZipWithAllFormats()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());
        var exports = new List<(string, TextStatistics, string)>
        {
            ("first text", new TextStatistics { TotalWords = 2, TotalCharacters = 10, ReplacedWordsCount = 0, SentenceCount = 1, ParagraphCount = 1 }, "first"),
            ("second text", new TextStatistics { TotalWords = 3, TotalCharacters = 12, ReplacedWordsCount = 1, SentenceCount = 1, ParagraphCount = 1 }, "second")
        };
        var formats = new[] { "txt", "json" };

        // Act
        var zipBytes = await exportService.ExportBatchToZipAsync(exports, formats);

        // Assert
        zipBytes.Should().NotBeNull();
        zipBytes.Length.Should().BeGreaterThan(0);
        
        // Verify it's a valid zip file by attempting to read it
        using var stream = new MemoryStream(zipBytes);
        using var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read);
        archive.Entries.Should().HaveCount(4); // 2 exports × 2 formats
    }

    [Fact]
    public void ExportController_GetSupportedFormats_ReturnsAllFormats()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());
        var controller = new PoRemoveBad.Server.Controllers.ExportController(exportService, new NullLogger<PoRemoveBad.Server.Controllers.ExportController>());

        // Act
        var result = controller.GetSupportedFormats();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var formats = okResult?.Value as List<ExportFormatMetadata>;
        formats.Should().NotBeNull();
        formats.Should().HaveCount(7);
        formats.Should().Contain(f => f.Format == "pdf");
        formats.Should().Contain(f => f.Format == "csv");
    }

    [Fact]
    public async Task ExportController_ExportSingle_ReturnsFileResult()
    {
        // Arrange
        var exportService = new ExportService(new NullLogger<ExportService>());
        var controller = new PoRemoveBad.Server.Controllers.ExportController(exportService, new NullLogger<PoRemoveBad.Server.Controllers.ExportController>());
        
        var request = new PoRemoveBad.Server.Controllers.ExportRequest
        {
            ProcessedText = "test text",
            Statistics = new TextStatistics { TotalWords = 2, TotalCharacters = 9, ReplacedWordsCount = 0, SentenceCount = 1, ParagraphCount = 1 },
            Format = "txt"
        };

        // Act
        var result = await controller.ExportSingle(request);

        // Assert
        result.Should().BeOfType<FileContentResult>();
        var fileResult = result as FileContentResult;
        fileResult?.ContentType.Should().Be("text/plain");
        fileResult?.FileDownloadName.Should().Contain("cleaned_text_");
        fileResult?.FileDownloadName.Should().EndWith(".txt");
    }
}
