using System.Text.Json.Serialization;

namespace PoRemoveBad.Core.Models;

/// <summary>
/// Represents the data structure for word replacements.
/// </summary>
public class WordReplacementData
{
    /// <summary>
    /// Gets or sets the list of word replacement entries.
    /// </summary>
    [JsonPropertyName("words")]
    public List<WordReplacementEntry> Words { get; set; } = new();
}

/// <summary>
/// Represents a single word replacement entry.
/// </summary>
public class WordReplacementEntry
{
    /// <summary>
    /// Gets or sets the original word.
    /// </summary>
    [JsonPropertyName("originalWord")]
    public string OriginalWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the replacement options for the original word.
    /// </summary>
    [JsonPropertyName("replacementOptions")]
    public string[] ReplacementOptions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the category of the word.
    /// </summary>
    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WordCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the part of speech of the word.
    /// </summary>
    [JsonPropertyName("partOfSpeech")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartOfSpeech PartOfSpeech { get; set; }

    /// <summary>
    /// Converts the entry to a <see cref="WordReplacement"/> object.
    /// </summary>
    /// <returns>A <see cref="WordReplacement"/> object.</returns>
    public WordReplacement ToWordReplacement()
    {
        return new WordReplacement
        {
            OriginalWord = OriginalWord,
            ReplacementOptions = ReplacementOptions,
            Category = Category,
            PartOfSpeech = PartOfSpeech
        };
    }
}
