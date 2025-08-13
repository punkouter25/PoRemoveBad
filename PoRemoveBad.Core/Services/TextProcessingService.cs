using System.Collections.Concurrent;
using System.Linq; // Added for LINQ methods
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Core.Services;

/// <summary>
/// Service for processing text and replacing inappropriate words.
/// </summary>
public partial class TextProcessingService : ITextProcessingService
{
    private readonly ConcurrentDictionary<string, WordReplacement> _wordDictionary = new(StringComparer.OrdinalIgnoreCase);
    private bool _isInitialized;
    private static readonly Random _random = new();
    private string _currentDictionaryType = "default";
    private readonly ILogger<TextProcessingService> _logger;

    public TextProcessingService(ILogger<TextProcessingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets a value indicating whether the service is initialized.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Gets the current dictionary type.
    /// </summary>
    public string CurrentDictionaryType => _currentDictionaryType;

    [GeneratedRegex(@"\b\w+\b")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"[aeiouy]+", RegexOptions.IgnoreCase)]
    private static partial Regex VowelGroupRegex(); // Regex for vowel groups

    /// <summary>
    /// Estimates the number of syllables in a word using a simple heuristic.
    /// </summary>
    /// <param name="word">The word to analyze.</param>
    /// <returns>An estimated syllable count.</returns>
    private static int EstimateSyllables(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return 0;

        word = word.ToLowerInvariant().Trim('\'', '"', '.', ',', '!', '?', ':', ';'); // Basic cleanup

        if (word.Length <= 3) return 1; // Short words often have 1 syllable

        var vowelGroups = VowelGroupRegex().Matches(word);
        var syllableCount = vowelGroups.Count;

        // Adjust for silent 'e' at the end
        if (word.EndsWith("e") && !word.EndsWith("le") && syllableCount > 1 && !VowelGroupRegex().IsMatch(word[^2..^1]))
        {
            // Check if the second to last char is not a vowel before decrementing
            if (!"aeiouy".Contains(word[^2]))
            {
                syllableCount--;
            }
        }
        // Adjust for 'le' ending if preceded by a consonant
        else if (word.EndsWith("le") && word.Length > 2 && !"aeiouy".Contains(word[^3]))
        {
            // Already counted by vowel group regex, no change needed unless previous logic reduced it
        }


        // Ensure at least one syllable
        return Math.Max(1, syllableCount);
    }


    /// <summary>
    /// Initializes the word replacement dictionary asynchronously.
    /// </summary>
    /// <param name="dictionaryType">The type of dictionary to initialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeDictionaryAsync(string dictionaryType = "default")
    {
        await Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("Initializing dictionary of type {DictionaryType}", dictionaryType);
                _wordDictionary.Clear();
                _isInitialized = false;

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = dictionaryType.ToLower() switch
                {
                    "buzzwords" => "PoRemoveBad.Core.Resources.word_replacements_buzzwords.json",
                    _ => "PoRemoveBad.Core.Resources.word_replacements.json"
                };

                // Add detailed logging for debugging
                var resources = assembly.GetManifestResourceNames();
                _logger.LogInformation("Available embedded resources:");
                foreach (var resource in resources)
                {
                    _logger.LogInformation("- {Resource}", resource);
                }
                _logger.LogInformation("Attempting to load resource: {ResourceName}", resourceName);

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    _logger.LogError("Resource not found: {ResourceName}", resourceName);
                    throw new FileNotFoundException($"Resource {resourceName} not found");
                }

                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();
                _logger.LogInformation("Raw JSON content: {Content}", jsonContent);
                stream.Position = 0; // Reset stream position for deserialization

                _logger.LogInformation("Resource found, attempting to deserialize");
                var data = await JsonSerializer.DeserializeAsync<WordReplacementData>(stream);

                if (data == null)
                {
                    _logger.LogError("Deserialization resulted in null data");
                    throw new InvalidOperationException("Failed to deserialize word replacements data.");
                }

                _logger.LogInformation("Successfully deserialized data with {WordCount} words", data.Words?.Count ?? 0);

                if (data.Words == null || data.Words.Count == 0)
                {
                    _logger.LogError("No words found in the deserialized data");
                    throw new InvalidOperationException("No words found in the dictionary file.");
                }

                foreach (var entry in data.Words)
                {
                    if (!_wordDictionary.TryAdd(entry.OriginalWord, entry.ToWordReplacement()))
                    {
                        _logger.LogWarning("Failed to add word: {Word}", entry.OriginalWord);
                    }
                }

                _currentDictionaryType = dictionaryType;
                _isInitialized = true;
                _logger.LogInformation("Dictionary initialized with {Count} words", _wordDictionary.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize dictionary of type {DictionaryType}. Error details: {ErrorMessage}",
                    dictionaryType, ex.ToString());
                throw;
            }
        });
    }

    /// <summary>
    /// Processes the input text asynchronously, replacing inappropriate words.
    /// </summary>
    /// <param name="inputText">The input text to process.</param>
    /// <param name="progress">An optional progress reporter.</param>
    /// <returns>A task representing the asynchronous operation, with a tuple containing the processed text and statistics.</returns>
    public async Task<(string ProcessedText, TextStatistics Statistics)> ProcessTextAsync(string inputText, IProgress<double>? progress = null)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Service must be initialized before processing text");

        var statistics = new TextStatistics
        {
            TotalCharacters = inputText.Length,
            ParagraphCount = inputText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None).Length,
            SentenceCount = inputText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length
        };

        var words = WordRegex().Matches(inputText);
        statistics.TotalWords = words.Count;

        var segments = new List<TextStatistics.GraphDataPoint>();
        var segmentSize = Math.Max(1, statistics.TotalWords / 10);
        var currentSegment = new TextStatistics.GraphDataPoint();
        var segmentIndex = 0;
        var wordIndex = 0;

        var processedText = await Task.Run(() => WordRegex().Replace(inputText, match =>
        {
            var word = match.Value;
            wordIndex++;

            if (wordIndex % segmentSize == 0 || wordIndex == statistics.TotalWords)
            {
                currentSegment.SegmentIndex = segmentIndex++;
                currentSegment.PercentageComplete = (double)wordIndex / statistics.TotalWords * 100;
                segments.Add(currentSegment);
                currentSegment = new TextStatistics.GraphDataPoint();
            }

            progress?.Report((double)wordIndex / statistics.TotalWords);

            if (_wordDictionary.TryGetValue(word, out var replacement))
            {
                statistics.ReplacedWordsCount++;
                currentSegment.InappropriateWordCount++;

                statistics.ReplacementFrequency.AddOrUpdate(
                    word,
                    1,
                    (_, count) => count + 1);

                // Randomly select a replacement option
                var index = _random.Next(replacement.ReplacementOptions.Length);
                var replacementWord = replacement.ReplacementOptions[index];
                return $"[HIGHLIGHT]{replacementWord}[/HIGHLIGHT]";
            }

            return word;
        }));

        // Replace the highlight markers with actual HTML
        processedText = processedText
            .Replace("[HIGHLIGHT]", "<mark style=\"background-color: yellow; border-radius: 3px; padding: 0 2px;\">")
            .Replace("[/HIGHLIGHT]", "</mark>");

        statistics.GraphData = segments;

        // --- Calculate Enhanced Statistics ---
        _logger.LogInformation("Calculating enhanced statistics...");

        // 1. Calculate Total Syllables
        long totalSyllables = 0;
        foreach (Match wordMatch in words)
        {
            totalSyllables += EstimateSyllables(wordMatch.Value);
        }
        _logger.LogInformation("Total estimated syllables: {TotalSyllables}", totalSyllables);


        // 2. Calculate Reading Time (Average 225 WPM)
        const double wordsPerMinute = 225.0;
        statistics.ReadingTimeMinutes = statistics.TotalWords > 0 ? statistics.TotalWords / wordsPerMinute : 0;
        _logger.LogInformation("Estimated reading time: {ReadingTimeMinutes} minutes", statistics.ReadingTimeMinutes);


        // 3. Calculate Readability Score (Flesch-Kincaid)
        // Formula: 206.835 - 1.015 * (TotalWords / TotalSentences) - 84.6 * (TotalSyllables / TotalWords)
        if (statistics.SentenceCount > 0 && statistics.TotalWords > 0)
        {
            try
            {
                // Use double for calculations to maintain precision
                double wordsPerSentence = (double)statistics.TotalWords / statistics.SentenceCount;
                double syllablesPerWord = (double)totalSyllables / statistics.TotalWords;

                statistics.ReadabilityScore = 206.835 - (1.015 * wordsPerSentence) - (84.6 * syllablesPerWord);
                _logger.LogInformation("Calculated Readability Score (FK): {ReadabilityScore}", statistics.ReadabilityScore);
            }
            catch (OverflowException ex)
            {
                _logger.LogError(ex, "Overflow calculating readability score.");
                statistics.ReadabilityScore = 0; // Default on error
            }
        }
        else
        {
            statistics.ReadabilityScore = 0; // Cannot calculate if no sentences or words
            _logger.LogWarning("Cannot calculate readability score: Sentences={SentenceCount}, Words={WordCount}", statistics.SentenceCount, statistics.TotalWords);
        }
        // --- End Enhanced Statistics Calculation ---


        return (processedText, statistics);
    }
}
