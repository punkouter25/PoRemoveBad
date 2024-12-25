using System.Collections.Concurrent;

namespace PoRemoveBad.Core.Models;

public class TextStatistics
{
    public int TotalWords { get; set; }
    public int TotalCharacters { get; set; }
    public int ReplacedWordsCount { get; set; }
    public ConcurrentDictionary<string, int> ReplacementFrequency { get; set; } = new();
    public List<GraphDataPoint> GraphData { get; set; } = new();
    public int SentenceCount { get; set; }
    public int ParagraphCount { get; set; }

    public class GraphDataPoint
    {
        public int SegmentIndex { get; set; }
        public int InappropriateWordCount { get; set; }
        public double PercentageComplete { get; set; }
    }
} 