namespace PoRemoveBad.Core.Models;

/// <summary>
/// Represents a word replacement entry.
/// </summary>
public class WordReplacement
{
    /// <summary>
    /// Gets or sets the original word.
    /// </summary>
    public string OriginalWord { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the replacement options for the original word.
    /// </summary>
    public string[] ReplacementOptions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the category of the word.
    /// </summary>
    public WordCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the part of speech of the word.
    /// </summary>
    public PartOfSpeech PartOfSpeech { get; set; }
}
