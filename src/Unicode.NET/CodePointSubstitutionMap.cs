using System.Collections.ObjectModel;

namespace Unicode.NET;

/// <summary>
/// Performs one-pass substitution of Unicode scalar values in UTF-16 text.
/// </summary>
/// <remarks>
/// Mappings are copied at construction and cannot be changed afterward. Replacement text is
/// appended unchanged and is not recursively remapped. Unmapped input scalars are preserved.
/// </remarks>
public sealed class CodePointSubstitutionMap
{
    private readonly IReadOnlyDictionary<CodePoint, string> _replacements;

    /// <summary>Creates a substitution map from Unicode scalar values to replacement strings.</summary>
    /// <param name="replacements">The mappings to copy into this immutable map.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="replacements"/> is null, or contains a null replacement.</exception>
    /// <exception cref="ArgumentException">Thrown when a mapping key is a UTF-16 surrogate.</exception>
    public CodePointSubstitutionMap(IReadOnlyDictionary<CodePoint, string> replacements)
    {
        ArgumentNullException.ThrowIfNull(replacements);

        var copy = new Dictionary<CodePoint, string>(replacements.Count);
        foreach (var pair in replacements)
        {
            if (pair.Key.IsSurrogate)
                throw new ArgumentException("Mapping keys must be Unicode scalar values, not UTF-16 surrogates.", nameof(replacements));

            ArgumentNullException.ThrowIfNull(pair.Value, nameof(replacements));
            copy.Add(pair.Key, pair.Value);
        }

        _replacements = new ReadOnlyDictionary<CodePoint, string>(copy);
    }

    /// <summary>Gets read-only mappings held by this instance.</summary>
    public IReadOnlyDictionary<CodePoint, string> Replacements => _replacements;

    /// <summary>Tries to get the replacement for a scalar without modifying replacement text.</summary>
    public bool TryGetReplacement(CodePoint value, out string? replacement)
    {
        if (value.IsSurrogate)
            throw new ArgumentException("Value must be a Unicode scalar value, not a UTF-16 surrogate.", nameof(value));

        return _replacements.TryGetValue(value, out replacement);
    }

    /// <summary>
    /// Replaces each mapped scalar in <paramref name="value">value</paramref> once.
    /// </summary>
    /// <param name="value">Well-formed UTF-16 input text.</param>
    /// <returns>Text with mapped scalars replaced; unmapped scalars remain unchanged.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> contains an unpaired UTF-16 surrogate.</exception>
    public string Replace(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new System.Text.StringBuilder(value.Length);
        foreach (var codePoint in value.EnumerateCodePoints())
        {
            if (codePoint.IsSurrogate)
                throw new ArgumentException("Input contains an unpaired UTF-16 surrogate.", nameof(value));

            if (_replacements.TryGetValue(codePoint, out string? replacement))
                result.Append(replacement);
            else
                result.Append(Utf16.Encode(codePoint));
        }

        return result.ToString();
    }
}
