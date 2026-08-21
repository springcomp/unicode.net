namespace Unicode.NET;

/// <summary>
/// Thrown when a Unicode property name or alias cannot be resolved.
/// Includes closest-match suggestions to help callers recover.
/// </summary>
public class UnknownPropertyException : Exception
{
    /// <summary>The unrecognised property name that was queried.</summary>
    public string PropertyName { get; }

    /// <summary>Suggested property names sorted by Levenshtein distance from <see cref="PropertyName"/>.</summary>
    public IReadOnlyList<string> Suggestions { get; }

    /// <param name="propertyName">The name that could not be resolved.</param>
    /// <param name="suggestions">Closest-match candidates (may be empty).</param>
    public UnknownPropertyException(string propertyName, IEnumerable<string> suggestions)
        : base(BuildMessage(propertyName, suggestions))
    {
        PropertyName = propertyName;
        Suggestions = suggestions.ToList();
    }

    private static string BuildMessage(string name, IEnumerable<string> suggestions)
    {
        var list = suggestions.ToList();
        return list.Count > 0
            ? $"Unknown Unicode property: '{name}'. Did you mean: {string.Join(", ", list)}?"
            : $"Unknown Unicode property: '{name}'.";
    }
}
