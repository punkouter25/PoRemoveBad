using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Core.Services;

/// <summary>
/// Interface for advanced text analysis services.
/// </summary>
public interface IAdvancedTextAnalysisService
{
    /// <summary>
    /// Analyzes the text for sentiment, grammar, vocabulary, and target audience.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A task representing the asynchronous operation, with the analysis results.</returns>
    Task<AdvancedTextAnalysis> AnalyzeTextAsync(string text);

    /// <summary>
    /// Analyzes the sentiment of the text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A task representing the asynchronous operation, with the sentiment analysis results.</returns>
    Task<SentimentAnalysis> AnalyzeSentimentAsync(string text);

    /// <summary>
    /// Analyzes the grammar of the text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A task representing the asynchronous operation, with the grammar analysis results.</returns>
    Task<List<GrammarIssue>> AnalyzeGrammarAsync(string text);

    /// <summary>
    /// Analyzes the vocabulary of the text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A task representing the asynchronous operation, with the vocabulary analysis results.</returns>
    Task<VocabularyAnalysis> AnalyzeVocabularyAsync(string text);

    /// <summary>
    /// Determines the target audience for the text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A task representing the asynchronous operation, with the target audience recommendations.</returns>
    Task<TargetAudienceRecommendation> DetermineTargetAudienceAsync(string text);
}