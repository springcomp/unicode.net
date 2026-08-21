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
    /// \s — Unicode White_Space binary property (includes NBSP, U+0085, etc., not just ASCII).
    /// </summary>
    public static CodePointSet Space { get; } =
        UnicodeBinaryProperties.GetPropertySet(BinaryProperty.White_Space, Version);

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
