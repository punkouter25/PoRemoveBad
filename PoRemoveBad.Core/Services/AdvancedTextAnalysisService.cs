using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PoRemoveBad.Core.Models;
using System.Text.Json;

namespace PoRemoveBad.Core.Services;

/// <summary>
/// Implementation of the advanced text analysis service.
/// </summary>
public partial class AdvancedTextAnalysisService : IAdvancedTextAnalysisService
{
    private readonly ILogger<AdvancedTextAnalysisService> _logger;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdvancedTextAnalysisService(ILogger<AdvancedTextAnalysisService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<AdvancedTextAnalysis> AnalyzeTextAsync(string text)
    {
        _logger.LogInformation("Starting text analysis for {TextLength} characters", text.Length);

        var analysis = new AdvancedTextAnalysis();

        try
        {
            // Run all analyses sequentially with enhanced performance
            analysis.Sentiment = await AnalyzeSentimentAsync(text);
            analysis.GrammarIssues = await AnalyzeGrammarAsync(text);
            analysis.Vocabulary = await AnalyzeVocabularyAsync(text);
            analysis.TargetAudience = await DetermineTargetAudienceAsync(text);

            _logger.LogInformation("Completed comprehensive text analysis with {GrammarIssuesCount} grammar issues",
                analysis.GrammarIssues.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during text analysis for {TextLength} characters", text.Length);
            throw;
        }

        return analysis;
    }

    public Task<SentimentAnalysis> AnalyzeSentimentAsync(string text)
    {
        _logger.LogInformation("Starting sentiment analysis for {TextLength} characters", text.Length);

        // Enhanced sentiment word lists
        var positiveWords = new[] { 
            // General positive
            "good", "great", "excellent", "wonderful", "amazing", "fantastic", "outstanding",
            "brilliant", "superb", "perfect", "awesome", "incredible", "remarkable", "exceptional",
            // Success related
            "success", "achievement", "accomplish", "win", "victory", "triumph", "excel",
            // Emotional positive
            "happy", "joy", "delighted", "pleased", "glad", "cheerful", "excited", "thrilled",
            // Quality related
            "efficient", "effective", "reliable", "innovative", "premium", "superior", "valuable",
            // Progress related
            "improve", "progress", "advance", "grow", "enhance", "upgrade", "optimize",
            // Support related
            "helpful", "supportive", "beneficial", "useful", "practical", "convenient"
        };

        var negativeWords = new[] {
            // General negative
            "bad", "terrible", "awful", "horrible", "poor", "disappointing", "inferior",
            "mediocre", "unsatisfactory", "unacceptable", "inadequate", "deficient", "flawed",
            // Problem related
            "problem", "issue", "defect", "bug", "error", "failure", "malfunction",
            // Emotional negative
            "sad", "unhappy", "frustrated", "angry", "upset", "annoyed", "irritated",
            // Quality related
            "inefficient", "ineffective", "unreliable", "outdated", "substandard", "worthless",
            // Decline related
            "decline", "decrease", "deteriorate", "worsen", "degrade", "fail", "break",
            // Opposition related
            "against", "oppose", "reject", "refuse", "deny", "prevent", "block"
        };

        var words = text.ToLower().Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '(', ')', '[', ']', '{', '}', '"', '\'', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        var positiveCount = words.Count(w => positiveWords.Contains(w));
        var negativeCount = words.Count(w => negativeWords.Contains(w));
        var totalWords = words.Length;

        // Enhanced scoring algorithm
        var score = totalWords > 0 ? (positiveCount - negativeCount) / (double)totalWords : 0;

        // More nuanced sentiment labeling
        string label;
        if (score > 0.2) label = "Very Positive";
        else if (score > 0.1) label = "Positive";
        else if (score > -0.1) label = "Neutral";
        else if (score > -0.2) label = "Negative";
        else label = "Very Negative";

        // Calculate confidence based on sample size and sentiment strength
        var confidence = Math.Min(1.0, Math.Max(0.3,
            (totalWords >= 10 ? 0.7 : totalWords / 14.0) + // Sample size factor
            (Math.Abs(score) * 0.3))); // Sentiment strength factor

        _logger.LogInformation("Completed sentiment analysis: {Label} with confidence {Confidence:P2}", label, confidence);

        return Task.FromResult(new SentimentAnalysis
        {
            Score = score,
            Label = label,
            Confidence = confidence,
            Details = new Dictionary<string, int>
            {
                { "Positive Words", positiveCount },
                { "Negative Words", negativeCount },
                { "Total Words", totalWords }
            }
        });
    }

    public Task<List<GrammarIssue>> AnalyzeGrammarAsync(string text)
    {
        _logger.LogInformation("Starting grammar analysis for {TextLength} characters", text.Length);

        var issues = new List<GrammarIssue>();

        // Using source-generated regex for better performance
        var firstPersonMatches = FirstPersonRegex().Matches(text);
        var weakAdverbMatches = WeakAdverbRegex().Matches(text);
        var conjunctionMatches = ConjunctionRepetitionRegex().Matches(text);

        foreach (Match match in firstPersonMatches)
        {
            issues.Add(new GrammarIssue
            {
                IssueType = "First Person",
                Description = "Consider using more formal language",
                Suggestion = "Consider rephrasing",
                Position = match.Index,
                Length = match.Length
            });
        }

        foreach (Match match in weakAdverbMatches)
        {
            issues.Add(new GrammarIssue
            {
                IssueType = "Weak Adverb",
                Description = "Consider using stronger language",
                Suggestion = "Consider rephrasing",
                Position = match.Index,
                Length = match.Length
            });
        }

        foreach (Match match in conjunctionMatches)
        {
            issues.Add(new GrammarIssue
            {
                IssueType = "Conjunction Repetition",
                Description = "Avoid repeating conjunctions",
                Suggestion = "Consider rephrasing",
                Position = match.Index,
                Length = match.Length
            });
        }

        _logger.LogInformation("Completed grammar analysis: found {Count} issues", issues.Count);
        return Task.FromResult(issues);
    }

    // Source-generated regex methods for better performance
    [GeneratedRegex(@"\b(i|me|my|mine)\b", RegexOptions.IgnoreCase)]
    private static partial Regex FirstPersonRegex();

    [GeneratedRegex(@"\b(very|really|quite)\b", RegexOptions.IgnoreCase)]
    private static partial Regex WeakAdverbRegex();

    [GeneratedRegex(@"\b(and|but|or)\b\s+\b(and|but|or)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ConjunctionRepetitionRegex();

    public Task<VocabularyAnalysis> AnalyzeVocabularyAsync(string text)
    {
        _logger.LogInformation("Starting vocabulary analysis for {TextLength} characters", text.Length);

        var words = text.ToLower()
            .Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToList();

        var wordFrequency = words
            .GroupBy(w => w)
            .ToDictionary(g => g.Key, g => g.Count());

        var suggestions = new List<VocabularySuggestion>();

        var overusedWords = wordFrequency
            .Where(kvp => kvp.Value > 5)
            .Select(kvp => new VocabularySuggestion
            {
                Word = kvp.Key,
                Suggestion = "Consider using synonyms",
                Reason = "Word appears frequently in the text"
            });

        suggestions.AddRange(overusedWords);

        _logger.LogInformation("Completed vocabulary analysis: found {Count} unique words", wordFrequency.Count);

        return Task.FromResult(new VocabularyAnalysis
        {
            UniqueWordCount = wordFrequency.Count,
            WordFrequency = wordFrequency,
            Suggestions = suggestions
        });
    }

    public Task<TargetAudienceRecommendation> DetermineTargetAudienceAsync(string text)
    {
        _logger.LogInformation("Starting target audience analysis for {TextLength} characters", text.Length);

        var words = text.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var avgWordLength = words.Average(w => w.Length);
        var sentenceCount = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var avgSentenceLength = words.Length / (double)sentenceCount;

        string readingLevel;
        string ageGroup;
        string educationLevel;
        double confidence;

        if (avgWordLength < 4 && avgSentenceLength < 10)
        {
            readingLevel = "Elementary";
            ageGroup = "8-12";
            educationLevel = "Elementary School";
            confidence = 0.8;
        }
        else if (avgWordLength < 5 && avgSentenceLength < 15)
        {
            readingLevel = "Middle School";
            ageGroup = "12-15";
            educationLevel = "Middle School";
            confidence = 0.7;
        }
        else if (avgWordLength < 6 && avgSentenceLength < 20)
        {
            readingLevel = "High School";
            ageGroup = "15-18";
            educationLevel = "High School";
            confidence = 0.75;
        }
        else
        {
            readingLevel = "College";
            ageGroup = "18+";
            educationLevel = "College";
            confidence = 0.85;
        }

        _logger.LogInformation("Completed target audience analysis: {ReadingLevel} level", readingLevel);

        return Task.FromResult(new TargetAudienceRecommendation
        {
            ReadingLevel = readingLevel,
            AgeGroup = ageGroup,
            EducationLevel = educationLevel,
            Confidence = confidence
        });
    }
}