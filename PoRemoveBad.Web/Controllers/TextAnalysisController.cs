using Microsoft.AspNetCore.Mvc;
using PoRemoveBad.Core.Models;
using PoRemoveBad.Core.Services;

namespace PoRemoveBad.Web.Controllers;

/// <summary>
/// API controller for text analysis endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TextAnalysisController : ControllerBase
{
    private readonly IAdvancedTextAnalysisService _analysisService;
    private readonly ILogger<TextAnalysisController> _logger;

    public TextAnalysisController(
        IAdvancedTextAnalysisService analysisService,
        ILogger<TextAnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes text for sentiment, grammar, vocabulary, and target audience.
    /// </summary>
    /// <param name="request">The text analysis request.</param>
    /// <returns>The analysis results.</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<AdvancedTextAnalysis>> AnalyzeText([FromBody] TextAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Received text analysis request");
            var result = await _analysisService.AnalyzeTextAsync(request.Text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing text");
            return StatusCode(500, "An error occurred while analyzing the text");
        }
    }

    /// <summary>
    /// Analyzes text sentiment.
    /// </summary>
    /// <param name="request">The text analysis request.</param>
    /// <returns>The sentiment analysis results.</returns>
    [HttpPost("sentiment")]
    public async Task<ActionResult<SentimentAnalysis>> AnalyzeSentiment([FromBody] TextAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Received sentiment analysis request");
            var result = await _analysisService.AnalyzeSentimentAsync(request.Text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return StatusCode(500, "An error occurred while analyzing sentiment");
        }
    }

    /// <summary>
    /// Analyzes text grammar.
    /// </summary>
    /// <param name="request">The text analysis request.</param>
    /// <returns>The grammar analysis results.</returns>
    [HttpPost("grammar")]
    public async Task<ActionResult<List<GrammarIssue>>> AnalyzeGrammar([FromBody] TextAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Received grammar analysis request");
            var result = await _analysisService.AnalyzeGrammarAsync(request.Text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing grammar");
            return StatusCode(500, "An error occurred while analyzing grammar");
        }
    }

    /// <summary>
    /// Analyzes text vocabulary.
    /// </summary>
    /// <param name="request">The text analysis request.</param>
    /// <returns>The vocabulary analysis results.</returns>
    [HttpPost("vocabulary")]
    public async Task<ActionResult<VocabularyAnalysis>> AnalyzeVocabulary([FromBody] TextAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Received vocabulary analysis request");
            var result = await _analysisService.AnalyzeVocabularyAsync(request.Text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing vocabulary");
            return StatusCode(500, "An error occurred while analyzing vocabulary");
        }
    }

    /// <summary>
    /// Determines the target audience for the text.
    /// </summary>
    /// <param name="request">The text analysis request.</param>
    /// <returns>The target audience recommendations.</returns>
    [HttpPost("target-audience")]
    public async Task<ActionResult<TargetAudienceRecommendation>> DetermineTargetAudience([FromBody] TextAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Received target audience analysis request");
            var result = await _analysisService.DetermineTargetAudienceAsync(request.Text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining target audience");
            return StatusCode(500, "An error occurred while determining target audience");
        }
    }
}

/// <summary>
/// Represents a text analysis request.
/// </summary>
public class TextAnalysisRequest
{
    /// <summary>
    /// Gets or sets the text to analyze.
    /// </summary>
    public string Text { get; set; } = string.Empty;
} 