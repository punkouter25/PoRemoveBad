namespace PoRemoveBad.Core.Models;

/// <summary>
/// Represents the category of a word.
/// </summary>
public enum WordCategory
{
    /// <summary>
    /// Profanity category.
    /// </summary>
    Profanity,

    /// <summary>
    /// Slur category.
    /// </summary>
    Slur,

    /// <summary>
    /// Inappropriate category.
    /// </summary>
    Inappropriate,

    /// <summary>
    /// Offensive category.
    /// </summary>
    Offensive,

    /// <summary>
    /// Mild category.
    /// </summary>
    Mild
}
