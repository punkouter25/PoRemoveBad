using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Core.Services;

public partial class TextProcessingService : ITextProcessingService
{
    private readonly ConcurrentDictionary<string, WordReplacement> _wordDictionary = new(StringComparer.OrdinalIgnoreCase);
    private bool _isInitialized;
    private static readonly Random _random = new();

    public bool IsInitialized => _isInitialized;

    [GeneratedRegex(@"\b\w+\b")]
    private static partial Regex WordRegex();

    public async Task InitializeDictionaryAsync()
    {
        if (_isInitialized) return;

        await Task.Run(async () =>
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("PoRemoveBad.Core.Resources.word_replacements.json");
                
                if (stream == null)
                    throw new InvalidOperationException("Could not find embedded word replacements resource.");

                var data = await JsonSerializer.DeserializeAsync<WordReplacementData>(stream);
                
                if (data == null)
                    throw new InvalidOperationException("Failed to deserialize word replacements data.");

                foreach (var entry in data.Words)
                {
                    _wordDictionary.TryAdd(entry.OriginalWord, entry.ToWordReplacement());
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize word replacements.", ex);
            }
        });
    }

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
                return replacement.ReplacementOptions[index];
            }

            return word;
        }));

        statistics.GraphData = segments;
        return (processedText, statistics);
    }
} 