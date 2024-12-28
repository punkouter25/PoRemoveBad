using System.Collections.Concurrent;

namespace PoRemoveBad.Core.Models;

/// <summary>
/// Represents statistics related to text processing.
/// </summary>
public class TextStatistics
{
    /// <summary>
    /// Gets or sets the total number of words in the text.
    /// </summary>
    public int TotalWords { get; set; }

    /// <summary>
    /// Gets or sets the total number of characters in the text.
    /// </summary>
    public int TotalCharacters { get; set; }

    /// <summary>
    /// Gets or sets the total number of replaced words in the text.
    /// </summary>
    public int ReplacedWordsCount { get; set; }

    /// <summary>
    /// Gets or sets the frequency of each replacement word.
    /// </summary>
    public ConcurrentDictionary<string, int> ReplacementFrequency { get; set; } = new();

    /// <summary>
    /// Gets or sets the graph data points for visualizing text processing statistics.
    /// </summary>
    public List<GraphDataPoint> GraphData { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of sentences in the text.
    /// </summary>
    public int SentenceCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of paragraphs in the text.
    /// </summary>
    public int ParagraphCount { get; set; }

    /// <summary>
    /// Represents a data point for graphing text processing statistics.
    /// </summary>
    public class GraphDataPoint
    {
        /// <summary>
        /// Gets or sets the index of the segment.
        /// </summary>
        public int SegmentIndex { get; set; }

        /// <summary>
        /// Gets or sets the count of inappropriate words in the segment.
        /// </summary>
        public int InappropriateWordCount { get; set; }

        /// <summary>
        /// Gets or sets the percentage of text processed in the segment.
        /// </summary>
        public double PercentageComplete { get; set; }
    }
}
