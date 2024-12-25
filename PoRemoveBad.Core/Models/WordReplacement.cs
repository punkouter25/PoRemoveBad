namespace PoRemoveBad.Core.Models;

public class WordReplacement
{
    public string OriginalWord { get; set; } = string.Empty;
    public string[] ReplacementOptions { get; set; } = Array.Empty<string>();
    public WordCategory Category { get; set; }
    public PartOfSpeech PartOfSpeech { get; set; }
} 