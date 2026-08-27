using Unicode.NET.Generated;

namespace Unicode.NET;

/// <summary>
/// Applies Unicode default whole-string lower- and upper-casing.
/// </summary>
/// <remarks>
/// Case mapping is distinct from case folding: mapping preserves lower- or upper-case
/// distinctions and can use contextual rules, while folding produces a comparison form.
/// Mapping does not normalize input or inspect the current culture.
/// </remarks>
public static class CaseMapping
{
    /// <summary>Maps <paramref name="input"/> to Unicode default lowercase.</summary>
    /// <param name="input">The UTF-16 string to map.</param>
    /// <param name="version">The Unicode version; defaults to <see cref="UnicodeVersion.Current"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="input"/> contains a lone surrogate.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="version"/> is not registered.</exception>
    public static string ToLower(string? input, UnicodeVersion? version = null)
        => Map(input, version ?? UnicodeVersion.Current, lowercase: true);

    /// <summary>Maps <paramref name="input"/> to Unicode default uppercase.</summary>
    /// <param name="input">The UTF-16 string to map.</param>
    /// <param name="version">The Unicode version; defaults to <see cref="UnicodeVersion.Current"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="input"/> contains a lone surrogate.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="version"/> is not registered.</exception>
    public static string ToUpper(string? input, UnicodeVersion? version = null)
        => Map(input, version ?? UnicodeVersion.Current, lowercase: false);

    private static string Map(string? input, UnicodeVersion version, bool lowercase)
    {
        ArgumentNullException.ThrowIfNull(input);
        var tables = UnicodeVersion.GetTablesOrThrow(version).GetCaseMappingData();
        var codePoints = new List<CodePoint>();

        foreach (var codePoint in input.EnumerateCodePoints())
        {
            if (codePoint.IsSurrogate)
                throw new ArgumentException("Input contains an unpaired UTF-16 surrogate.", nameof(input));
            codePoints.Add(codePoint);
        }

        var result = new System.Text.StringBuilder(input.Length);
        for (int index = 0; index < codePoints.Count; index++)
        {
            CodePoint codePoint = codePoints[index];
            int[]? mapping = null;

            if (lowercase)
            {
                foreach (var rule in tables.ContextualRules)
                {
                    if (rule.Source == codePoint.Value && rule.Predicate == "Final_Sigma" &&
                        IsFinalSigma(codePoints, index, tables))
                    {
                        mapping = rule.LowercaseMapping;
                        break;
                    }
                }

                mapping ??= tables.FullLowercaseMap.GetValueOrDefault(codePoint.Value);
                if (mapping is null && tables.SimpleLowercaseMap.TryGetValue(codePoint.Value, out int simple))
                    mapping = [simple];
            }
            else
            {
                mapping = tables.FullUppercaseMap.GetValueOrDefault(codePoint.Value);
                if (mapping is null && tables.SimpleUppercaseMap.TryGetValue(codePoint.Value, out int simple))
                    mapping = [simple];
            }

            if (mapping is null)
            {
                result.Append(Utf16.Encode(codePoint));
                continue;
            }

            foreach (int mapped in mapping)
                result.Append(Utf16.Encode(CodePoint.CreateScalar(mapped)));
        }

        return result.ToString();
    }

    private static bool IsFinalSigma(
        IReadOnlyList<CodePoint> codePoints,
        int index,
        CaseMappingData tables)
    {
        bool precedingCased = false;
        for (int i = index - 1; i >= 0; i--)
        {
            if (Contains(tables.CaseIgnorable, codePoints[i]))
                continue;
            precedingCased = Contains(tables.Cased, codePoints[i]);
            break;
        }

        if (!precedingCased)
            return false;

        for (int i = index + 1; i < codePoints.Count; i++)
        {
            if (Contains(tables.CaseIgnorable, codePoints[i]))
                continue;
            return !Contains(tables.Cased, codePoints[i]);
        }

        return true;
    }

    private static bool Contains(IReadOnlyList<CodePointRange> ranges, CodePoint value)
    {
        foreach (var range in ranges)
        {
            if (range.Contains(value))
                return true;
        }

        return false;
    }
}
