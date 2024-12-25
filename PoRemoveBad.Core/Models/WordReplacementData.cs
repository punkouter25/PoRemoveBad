using System.Text.Json.Serialization;

namespace PoRemoveBad.Core.Models;

public class WordReplacementData
{
    [JsonPropertyName("words")]
    public List<WordReplacementEntry> Words { get; set; } = new();
}

public class WordReplacementEntry
{
    [JsonPropertyName("originalWord")]
    public string OriginalWord { get; set; } = string.Empty;

    [JsonPropertyName("replacementOptions")]
    public string[] ReplacementOptions { get; set; } = Array.Empty<string>();

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WordCategory Category { get; set; }

    [JsonPropertyName("partOfSpeech")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartOfSpeech PartOfSpeech { get; set; }

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