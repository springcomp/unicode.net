using Unicode.NET;

namespace Unicode.NET.Xml;

/// <summary>
/// XML 1.0 fifth edition character classes, hand-authored from the spec productions.
/// </summary>
public static class XmlCharacterSets
{
    /// <summary>
    /// XML 1.0 fifth edition <c>Char</c> production:
    /// <c>Char ::= #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]</c>.
    /// </summary>
    public static CodePointSet Char { get; } = BuildChar();

    /// <summary>
    /// XML 1.0 fifth edition <c>NameStartChar</c> production:
    /// <c>NameStartChar ::= ":" | [A-Z] | "_" | [a-z] | [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] |
    /// [#x370-#x37D] | [#x37F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] | [#x2C00-#x2FEF] |
    /// [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]</c>.
    /// </summary>
    public static CodePointSet NameStartChar { get; } = BuildNameStartChar();

    /// <summary>
    /// XML 1.0 fifth edition <c>NameChar</c> production:
    /// <c>NameChar ::= NameStartChar | "-" | "." | [0-9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]</c>.
    /// </summary>
    public static CodePointSet NameChar { get; } = BuildNameChar();

    /// <summary>
    /// XML 1.0 fifth edition whitespace (<c>S</c>) production: <c>S ::= (#x20 | #x9 | #xD | #xA)</c>.
    /// </summary>
    public static CodePointSet Whitespace { get; } = BuildWhitespace();

    private static CodePointSet BuildChar()
    {
        var builder = new CodePointSetBuilder();
        builder.Add(CodePoint.Create(0x0009));
        builder.Add(CodePoint.Create(0x000A));
        builder.Add(CodePoint.Create(0x000D));
        builder.Add(CodePointRange.Create(0x0020, 0xD7FF));
        builder.Add(CodePointRange.Create(0xE000, 0xFFFD));
        builder.Add(CodePointRange.Create(0x10000, 0x10FFFF));
        return builder.Build();
    }

    private static CodePointSet BuildNameStartChar()
    {
        var builder = new CodePointSetBuilder();
        builder.Add(CodePoint.Create(0x003A)); // :
        builder.Add(CodePointRange.Create(0x0041, 0x005A)); // A-Z
        builder.Add(CodePoint.Create(0x005F)); // _
        builder.Add(CodePointRange.Create(0x0061, 0x007A)); // a-z
        builder.Add(CodePointRange.Create(0x00C0, 0x00D6));
        builder.Add(CodePointRange.Create(0x00D8, 0x00F6));
        builder.Add(CodePointRange.Create(0x00F8, 0x02FF));
        builder.Add(CodePointRange.Create(0x0370, 0x037D));
        builder.Add(CodePointRange.Create(0x037F, 0x1FFF));
        builder.Add(CodePointRange.Create(0x200C, 0x200D));
        builder.Add(CodePointRange.Create(0x2070, 0x218F));
        builder.Add(CodePointRange.Create(0x2C00, 0x2FEF));
        builder.Add(CodePointRange.Create(0x3001, 0xD7FF));
        builder.Add(CodePointRange.Create(0xF900, 0xFDCF));
        builder.Add(CodePointRange.Create(0xFDF0, 0xFFFD));
        builder.Add(CodePointRange.Create(0x10000, 0xEFFFF));
        return builder.Build();
    }

    private static CodePointSet BuildNameChar()
    {
        var builder = new CodePointSetBuilder();
        builder.AddRange(NameStartChar.Ranges);
        builder.Add(CodePoint.Create(0x002D)); // -
        builder.Add(CodePoint.Create(0x002E)); // .
        builder.Add(CodePointRange.Create(0x0030, 0x0039)); // 0-9
        builder.Add(CodePoint.Create(0x00B7));
        builder.Add(CodePointRange.Create(0x0300, 0x036F));
        builder.Add(CodePointRange.Create(0x203F, 0x2040));
        return builder.Build();
    }

    private static CodePointSet BuildWhitespace()
    {
        var builder = new CodePointSetBuilder();
        builder.Add(CodePoint.Create(0x0009));
        builder.Add(CodePoint.Create(0x000A));
        builder.Add(CodePoint.Create(0x000D));
        builder.Add(CodePoint.Create(0x0020));
        return builder.Build();
    }
}
