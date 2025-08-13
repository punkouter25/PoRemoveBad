using System.Collections.Generic;

namespace PoRemoveBad.Core.Models;

/// <summary>
/// Represents advanced text analysis results including sentiment, grammar, and readability metrics.
/// </summary>
public class AdvancedTextAnalysis
{
    /// <summary>
    /// Gets or sets the sentiment analysis results.
    /// </summary>
    public SentimentAnalysis Sentiment { get; set; } = new();

    /// <summary>
    /// Gets or sets the grammar analysis results.
    /// </summary>
    public List<GrammarIssue> GrammarIssues { get; set; } = new();

    /// <summary>
    /// Gets or sets the vocabulary analysis results.
    /// </summary>
    public VocabularyAnalysis Vocabulary { get; set; } = new();

    /// <summary>
    /// Gets or sets the target audience recommendations.
    /// </summary>
    public TargetAudienceRecommendation TargetAudience { get; set; } = new();
}

/// <summary>
/// Represents sentiment analysis results.
/// </summary>
public class SentimentAnalysis
{
    /// <summary>
    /// Gets or sets the overall sentiment score (-1 to 1).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Gets or sets the sentiment label (Positive, Negative, Neutral).
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score for the sentiment analysis.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Gets or sets additional details about the sentiment analysis.
    /// </summary>
    public Dictionary<string, int> Details { get; set; } = new();
}

/// <summary>
/// Represents a grammar issue found in the text.
/// </summary>
public class GrammarIssue
{
    /// <summary>
    /// Gets or sets the type of grammar issue.
    /// </summary>
    public string IssueType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggested correction.
    /// </summary>
    public string Suggestion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position in the text where the issue occurs.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the length of the text affected by the issue.
    /// </summary>
    public int Length { get; set; }
}

/// <summary>
/// Represents vocabulary analysis results.
/// </summary>
public class VocabularyAnalysis
{
    /// <summary>
    /// Gets or sets the unique word count.
    /// </summary>
    public int UniqueWordCount { get; set; }

    /// <summary>
    /// Gets or sets the word frequency analysis.
    /// </summary>
    public Dictionary<string, int> WordFrequency { get; set; } = new();

    /// <summary>
    /// Gets or sets the suggested vocabulary improvements.
    /// </summary>
    public List<VocabularySuggestion> Suggestions { get; set; } = new();
}

/// <summary>
/// Represents a vocabulary suggestion.
/// </summary>
public class VocabularySuggestion
{
    /// <summary>
    /// Gets or sets the word to be replaced.
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggested replacement.
    /// </summary>
    public string Suggestion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for the suggestion.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Represents target audience recommendations.
/// </summary>
public class TargetAudienceRecommendation
{
    /// <summary>
    /// Gets or sets the recommended reading level.
    /// </summary>
    public string ReadingLevel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommended age group.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommended education level.
    /// </summary>
    public string EducationLevel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score for the recommendations.
    /// </summary>
    public double Confidence { get; set; }
}