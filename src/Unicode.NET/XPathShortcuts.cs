namespace Unicode.NET;

/// <summary>
/// Pre-built character sets for XSD/XPath shortcut escapes, derived from real Unicode
/// properties (general categories and binary properties), not .NET Regex/ASCII semantics.
/// </summary>
public static class XPathShortcuts
{
    private static readonly UnicodeVersion Version = UnicodeVersion.V15_1_0;

    /// <summary>
    /// \d — \p{Nd} (Unicode Decimal_Number category).
    /// </summary>
    public static CodePointSet Digit { get; } =
        UnicodeData.GetCategorySet("Nd", Version);

    /// <summary>
    /// \s — [#x20\t\n\r]. Per the XSD/XPath spec this is ASCII-only
    /// (space, tab, LF, CR); it is NOT the Unicode White_Space property.
    /// </summary>
    public static CodePointSet Space { get; } = BuildAsciiSpace();

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
    public static CodePointSet Word { get; } =
        CodePointSet.All.Subtract(
            UnicodeData.GetCategorySet("P", Version)
                .Union(UnicodeData.GetCategorySet("Z", Version))
                .Union(UnicodeData.GetCategorySet("C", Version)));
}
