using System.ComponentModel;
using System.Runtime.CompilerServices;
using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Mobile.ViewModels;

/// <summary>
/// ViewModel for the MainPage, providing properties and methods for text processing statistics.
/// </summary>
public class MainPageViewModel : INotifyPropertyChanged
{
    private bool _hasProcessedText;
    private bool _hasStatistics;
    private int _totalWords;
    private int _totalCharacters;
    private int _replacedWordsCount;
    private int _sentenceCount;
    private int _paragraphCount;

    /// <summary>
    /// Gets or sets a value indicating whether the text has been processed.
    /// </summary>
    public bool HasProcessedText
    {
        get => _hasProcessedText;
        set => SetProperty(ref _hasProcessedText, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether statistics are available.
    /// </summary>
    public bool HasStatistics
    {
        get => _hasStatistics;
        set => SetProperty(ref _hasStatistics, value);
    }

    /// <summary>
    /// Gets or sets the total number of words in the text.
    /// </summary>
    public int TotalWords
    {
        get => _totalWords;
        set => SetProperty(ref _totalWords, value);
    }

    /// <summary>
    /// Gets or sets the total number of characters in the text.
    /// </summary>
    public int TotalCharacters
    {
        get => _totalCharacters;
        set => SetProperty(ref _totalCharacters, value);
    }

    /// <summary>
    /// Gets or sets the total number of replaced words in the text.
    /// </summary>
    public int ReplacedWordsCount
    {
        get => _replacedWordsCount;
        set => SetProperty(ref _replacedWordsCount, value);
    }

    /// <summary>
    /// Gets or sets the total number of sentences in the text.
    /// </summary>
    public int SentenceCount
    {
        get => _sentenceCount;
        set => SetProperty(ref _sentenceCount, value);
    }

    /// <summary>
    /// Gets or sets the total number of paragraphs in the text.
    /// </summary>
    public int ParagraphCount
    {
        get => _paragraphCount;
        set => SetProperty(ref _paragraphCount, value);
    }

    /// <summary>
    /// Updates the statistics properties with the provided <see cref="TextStatistics"/> object.
    /// </summary>
    /// <param name="statistics">The text statistics to update.</param>
    public void UpdateStatistics(TextStatistics statistics)
    {
        TotalWords = statistics.TotalWords;
        TotalCharacters = statistics.TotalCharacters;
        ReplacedWordsCount = statistics.ReplacedWordsCount;
        SentenceCount = statistics.SentenceCount;
        ParagraphCount = statistics.ParagraphCount;
        HasStatistics = true;
    }

    /// <summary>
    /// Resets the ViewModel properties to their default values.
    /// </summary>
    public void Reset()
    {
        HasProcessedText = false;
        HasStatistics = false;
        TotalWords = 0;
        TotalCharacters = 0;
        ReplacedWordsCount = 0;
        SentenceCount = 0;
        ParagraphCount = 0;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the specified property to the given value and raises the <see cref="PropertyChanged"/> event if the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The field to set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property (optional).</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
