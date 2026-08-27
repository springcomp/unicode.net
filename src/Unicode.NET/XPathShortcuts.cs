using System.Collections.Concurrent;

namespace Unicode.NET;

/// <summary>
/// Pre-built character sets for XSD/XPath shortcut escapes, derived from real Unicode
/// properties (general categories and binary properties), not .NET Regex/ASCII semantics.
/// </summary>
public static class XPathShortcuts
{
    private static readonly ConcurrentDictionary<UnicodeVersion, CodePointSet> s_digitCache = new();
    private static readonly ConcurrentDictionary<UnicodeVersion, CodePointSet> s_wordCache = new();
    private static readonly CodePointSet s_space = BuildAsciiSpace();

    /// <summary>
    /// \d — \p{Nd} (Unicode Decimal_Number category).
    /// </summary>
    /// <param name="version">The Unicode version; defaults to <see cref="UnicodeVersion.Current"/>.</param>
    public static CodePointSet Digit(UnicodeVersion? version = null) =>
        s_digitCache.GetOrAdd(version ?? UnicodeVersion.Current,
            static v => UnicodeData.GetCategorySet("Nd", v));

    /// <summary>
    /// \s — [#x20\t\n\r]. Per the XSD/XPath spec this is ASCII-only
    /// (space, tab, LF, CR); it is NOT the Unicode White_Space property.
    /// </summary>
    public static CodePointSet Space() => s_space;

    /// <summary>Tests whether <paramref name="value"/> belongs to the XPath/XSD <c>\d</c> set.</summary>
    /// <param name="value">Code point to test.</param>
    /// <param name="version">The Unicode version; defaults to <see cref="UnicodeVersion.Current"/>.</param>
    public static bool IsDigit(CodePoint value, UnicodeVersion? version = null) =>
        Digit(version).Contains(value);

    /// <summary>Tests whether <paramref name="value"/> belongs to the XPath/XSD <c>\s</c> set.</summary>
    /// <param name="value">Code point to test.</param>
    public static bool IsSpace(CodePoint value) => s_space.Contains(value);

    /// <summary>Tests whether <paramref name="value"/> belongs to the XPath/XSD <c>\w</c> set.</summary>
    /// <param name="value">Code point to test.</param>
    /// <param name="version">The Unicode version; defaults to <see cref="UnicodeVersion.Current"/>.</param>
    public static bool IsWord(CodePoint value, UnicodeVersion? version = null) =>
        Word(version).Contains(value);

    private static CodePointSet BuildAsciiSpace()
    {
        var builder = new CodePointSetBuilder();
        builder.Add(CodePointRange.Create(0x09, 0x0A)); // \t, \n
        builder.Add(CodePointRange.Create(0x0D, 0x0D)); // \r
        builder.Add(CodePointRange.Create(0x20, 0x20)); // space
        return builder.Build();
    }

    /// <summary>
    /// \w — [#x0000-#x10FFFF]-[\p{P}\p{Z}\p{C}]: every character except
    /// Punctuation, Separator, and Other categories (e.g. excludes '_', which is Pc).
    /// </summary>
    /// <param name="version">The Unicode version; defaults to <see cref="UnicodeVersion.Current"/>.</param>
    public static CodePointSet Word(UnicodeVersion? version = null) =>
        s_wordCache.GetOrAdd(version ?? UnicodeVersion.Current, static v =>
            CodePointSet.All.Subtract(
                UnicodeData.GetCategorySet("P", v)
                    .Union(UnicodeData.GetCategorySet("Z", v))
                    .Union(UnicodeData.GetCategorySet("C", v))));
}
