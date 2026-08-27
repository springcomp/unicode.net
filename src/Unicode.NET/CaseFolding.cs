using Unicode.NET.Generated;

namespace Unicode.NET;

/// <summary>
/// Unicode case-folding facade. Provides scalar-level folding using UCD <c>CaseFolding.txt</c> data.
/// </summary>
/// <remarks>
/// <para>
/// Simple folding (C+S records) and full folding (C+F records, 1:N mappings) are both implemented.
/// String folding rejects malformed UTF-16 and allocates its returned string. Turkic locale is
/// designed-in but reserved for future implementation.
/// </para>
/// <para>
/// Case closure (<see cref="CaseClosure"/>) operates on scalar sets only and does not expand
/// 1:N full-folding mappings. Callers needing full-fold string expansion must use
/// the string <see cref="Fold(string, CaseFoldingMode, CaseFoldingLocale, UnicodeVersion?)"/> with <see cref="CaseFoldingMode.Full"/> directly.
/// </para>
/// <para>
/// Case folding is not lowercasing and does not imply normalization.
/// Do not use <see cref="char.ToLower(char)"/> or culture-aware APIs as a substitute.
/// </para>
/// </remarks>
public static class CaseFolding
{
    /// <summary>
    /// Folds a single code point according to the specified mode, locale, and Unicode version.
    /// </summary>
    /// <param name="codePoint">The code point to fold.</param>
    /// <param name="mode">
    /// The folding mode. Both <see cref="CaseFoldingMode.Simple"/> and
    /// <see cref="CaseFoldingMode.Full"/> are implemented.
    /// </param>
    /// <param name="locale">
    /// The locale policy. <see cref="CaseFoldingLocale.Default"/> is implemented;
    /// <see cref="CaseFoldingLocale.Turkic"/> throws <see cref="NotSupportedException"/>.
    /// </param>
    /// <param name="version">
    /// The Unicode version. Defaults to <see cref="UnicodeVersion.Current"/>.
    /// </param>
    /// <returns>
    /// A non-empty read-only list of code points. Simple mode always returns exactly one element.
    /// Full mode may return one or more elements.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="locale"/> is
    /// <see cref="CaseFoldingLocale.Turkic"/> (reserved for future implementation),
    /// or when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static IReadOnlyList<CodePoint> Fold(
        CodePoint codePoint,
        CaseFoldingMode mode = CaseFoldingMode.Full,
        CaseFoldingLocale locale = CaseFoldingLocale.Default,
        UnicodeVersion? version = null)
    {
        var ver = version ?? UnicodeVersion.Current;
        var tables = UnicodeVersion.GetTablesOrThrow(ver);

        if (locale == CaseFoldingLocale.Turkic)
            throw new NotSupportedException(
                "CaseFoldingLocale.Turkic is reserved for future implementation. " +
                "Turkic (T) case-folding map generation has not yet been added.");

        return mode switch
        {
            CaseFoldingMode.Simple => FoldSimple(codePoint, tables.GetCaseFoldingData()),
            CaseFoldingMode.Full => FoldFull(codePoint, tables.GetCaseFoldingData()),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown CaseFoldingMode.")
        };
    }

    private static IReadOnlyList<CodePoint> FoldSimple(CodePoint codePoint, CaseFoldingData data)
    {
        var map = data.SimpleMap;
        int value = codePoint.Value;
        if (map.TryGetValue(value, out int mapped))
            return [CodePoint.CreateScalar(mapped)];
        return [codePoint];
    }

    private static IReadOnlyList<CodePoint> FoldFull(CodePoint codePoint, CaseFoldingData data)
    {
        var fullMap = data.FullMap;
        int value = codePoint.Value;
        if (fullMap.TryGetValue(value, out int[]? mapped))
            return Array.ConvertAll(mapped, CodePoint.CreateScalar);
        // Fallback: use simple fold
        return FoldSimple(codePoint, data);
    }

    /// <summary>
    /// Folds all Unicode scalar values in a UTF-16 string. The result is not normalized.
    /// </summary>
    /// <remarks>
    /// The returned string is newly allocated, including when the input needs no mapping.
    /// Unpaired UTF-16 surrogates are rejected with <see cref="ArgumentException"/>.
    /// </remarks>
    public static string Fold(
        string value,
        CaseFoldingMode mode = CaseFoldingMode.Full,
        CaseFoldingLocale locale = CaseFoldingLocale.Default,
        UnicodeVersion? version = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        var ver = version ?? UnicodeVersion.Current;
        var tables = UnicodeVersion.GetTablesOrThrow(ver);
        if (locale == CaseFoldingLocale.Turkic)
            throw new NotSupportedException(
                "CaseFoldingLocale.Turkic is reserved for future implementation. " +
                "Turkic (T) case-folding map generation has not yet been added.");

        if (mode is not (CaseFoldingMode.Simple or CaseFoldingMode.Full))
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown CaseFoldingMode.");

        var data = tables.GetCaseFoldingData();
        var builder = new System.Text.StringBuilder(value.Length);
        for (int index = 0; index < value.Length;)
        {
            char first = value[index];
            if (Utf16.IsHighSurrogate(first))
            {
                if (index + 1 >= value.Length || !Utf16.IsLowSurrogate(value[index + 1]))
                    throw new ArgumentException("Value contains an unpaired UTF-16 surrogate.", nameof(value));
            }
            else if (Utf16.IsLowSurrogate(first))
            {
                throw new ArgumentException("Value contains an unpaired UTF-16 surrogate.", nameof(value));
            }

            Utf16.Decode(value.AsSpan(index), out var codePoint, out int consumed);
            var folded = mode == CaseFoldingMode.Simple
                ? FoldSimple(codePoint, data)
                : FoldFull(codePoint, data);
            foreach (var item in folded)
                builder.Append(Utf16.Encode(item));
            index += consumed;
        }
        return builder.ToString();
    }

    /// <summary>
    /// Compares two UTF-16 strings after Unicode case folding.
    /// </summary>
    /// <remarks>Both folded strings are allocated; this gives the comparison the same semantics as <see cref="Fold(string, CaseFoldingMode, CaseFoldingLocale, UnicodeVersion?)"/>.</remarks>
    public static bool CaselessEquals(
        ReadOnlySpan<char> left,
        ReadOnlySpan<char> right,
        CaseFoldingMode mode = CaseFoldingMode.Full,
        CaseFoldingLocale locale = CaseFoldingLocale.Default,
        UnicodeVersion? version = null)
    {
        // Strings are required internally so malformed UTF-16 validation and version policy stay identical.
        return string.Equals(Fold(left.ToString(), mode, locale, version),
            Fold(right.ToString(), mode, locale, version), StringComparison.Ordinal);
    }
}
