using System.ComponentModel;
using System.Runtime.CompilerServices;
using PoRemoveBad.Core.Models;

namespace PoRemoveBad.Mobile.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private bool _hasProcessedText;
    private bool _hasStatistics;
    private int _totalWords;
    private int _totalCharacters;
    private int _replacedWordsCount;
    private int _sentenceCount;
    private int _paragraphCount;

    public bool HasProcessedText
    {
        get => _hasProcessedText;
        set => SetProperty(ref _hasProcessedText, value);
    }

    public bool HasStatistics
    {
        get => _hasStatistics;
        set => SetProperty(ref _hasStatistics, value);
    }

    public int TotalWords
    {
        get => _totalWords;
        set => SetProperty(ref _totalWords, value);
    }

    public int TotalCharacters
    {
        get => _totalCharacters;
        set => SetProperty(ref _totalCharacters, value);
    }

    public int ReplacedWordsCount
    {
        get => _replacedWordsCount;
        set => SetProperty(ref _replacedWordsCount, value);
    }

    public int SentenceCount
    {
        get => _sentenceCount;
        set => SetProperty(ref _sentenceCount, value);
    }

    public int ParagraphCount
    {
        get => _paragraphCount;
        set => SetProperty(ref _paragraphCount, value);
    }

    public void UpdateStatistics(TextStatistics statistics)
    {
        TotalWords = statistics.TotalWords;
        TotalCharacters = statistics.TotalCharacters;
        ReplacedWordsCount = statistics.ReplacedWordsCount;
        SentenceCount = statistics.SentenceCount;
        ParagraphCount = statistics.ParagraphCount;
        HasStatistics = true;
    }

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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
} 