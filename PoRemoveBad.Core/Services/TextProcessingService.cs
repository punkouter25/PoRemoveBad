using System.Collections.Concurrent;
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
        return (processedText, statistics);
    }
}
