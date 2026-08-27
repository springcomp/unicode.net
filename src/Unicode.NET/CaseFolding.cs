using System.Collections.Frozen;
using Unicode.NET.Generated;

namespace Unicode.NET;

/// <summary>
/// Unicode case-folding facade. Provides scalar-level folding using UCD <c>CaseFolding.txt</c> data.
/// </summary>
/// <remarks>
/// <para>
/// Simple folding (C+S records) and full folding (C+F records, 1:N mappings) are both implemented.
/// Turkic locale is designed-in but reserved for future implementation.
/// </para>
/// <para>
/// Case closure (<see cref="CaseClosure"/>) operates on scalar sets only and does not expand
/// 1:N full-folding mappings. Callers needing full-fold string expansion must use
/// <see cref="Fold"/> with <see cref="CaseFoldingMode.Full"/> directly.
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
    /// Full mode (when implemented) may return one or more elements.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="locale"/> is
    /// <see cref="CaseFoldingLocale.Turkic"/> (reserved for future implementation),
    /// or when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static IReadOnlyList<CodePoint> Fold(
        CodePoint codePoint,
        CaseFoldingMode mode = CaseFoldingMode.Simple,
        CaseFoldingLocale locale = CaseFoldingLocale.Default,
        UnicodeVersion? version = null)
    {
        var ver = version ?? UnicodeVersion.Current;
        UnicodeVersion.GetTablesOrThrow(ver);

        if (locale == CaseFoldingLocale.Turkic)
            throw new NotSupportedException(
                "CaseFoldingLocale.Turkic is reserved for future implementation. " +
                "Turkic (T) case-folding map generation has not yet been added.");

        return mode switch
        {
            CaseFoldingMode.Simple => FoldSimple(codePoint, ver),
            CaseFoldingMode.Full => FoldFull(codePoint, ver),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown CaseFoldingMode.")
        };
    }

    private static IReadOnlyList<CodePoint> FoldSimple(CodePoint codePoint, UnicodeVersion version)
    {
        var map = GetSimpleMap(version);
        int value = codePoint.Value;
        if (map.TryGetValue(value, out int mapped))
            return [CodePoint.CreateScalar(mapped)];
        return [codePoint];
    }

    private static IReadOnlyList<CodePoint> FoldFull(CodePoint codePoint, UnicodeVersion version)
    {
        var fullMap = GetFullMap(version);
        int value = codePoint.Value;
        if (fullMap.TryGetValue(value, out int[]? mapped))
            return Array.ConvertAll(mapped, CodePoint.CreateScalar);
        // Fallback: use simple fold
        return FoldSimple(codePoint, version);
    }

    /// <summary>
    /// Returns the simple case-folding map for the given Unicode version.
    /// </summary>
    internal static FrozenDictionary<int, int> GetSimpleMap(UnicodeVersion version)
    {
        if (version == UnicodeVersion.V15_1_0)
            return CaseFolding_15_1_0.SimpleMap;
        if (version == UnicodeVersion.V16_0_0)
            return CaseFolding_16_0_0.SimpleMap;
        throw new NotSupportedException(
            $"Simple case-folding data is not available for Unicode {version}. " +
            $"Registered versions: 15.1.0, 16.0.0.");
    }

    /// <summary>
    /// Returns the full case-folding map for the given Unicode version.
    /// </summary>
    internal static FrozenDictionary<int, int[]> GetFullMap(UnicodeVersion version)
    {
        if (version == UnicodeVersion.V15_1_0)
            return CaseFolding_15_1_0.FullMap;
        if (version == UnicodeVersion.V16_0_0)
            return CaseFolding_16_0_0.FullMap;
        throw new NotSupportedException(
            $"Full case-folding data is not available for Unicode {version}. " +
            $"Registered versions: 15.1.0, 16.0.0.");
    }
}
