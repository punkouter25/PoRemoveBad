using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PoRemoveBad.Core.Models;
using PoRemoveBad.Core.Services;
using PoRemoveBad.Web.Controllers;
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
        await Assert.ThrowsAsync<ArgumentException>(() => exportService.ExportToFileAsync("hello", stats, "xml"));

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
}
