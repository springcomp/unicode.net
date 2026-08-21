using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

public class UnicodeBinaryPropertiesTests
{
    // ── TryResolveProperty ────────────────────────────────────────────────────

    [Fact]
    public void TryResolveProperty_Alphabetic_Success()
    {
        var result = UnicodeBinaryProperties.TryResolveProperty("Alphabetic", out var prop);
        Assert.True(result);
        Assert.Equal(BinaryProperty.Alphabetic, prop);
    }

    [Fact]
    public void TryResolveProperty_White_Space_Success()
    {
        var result = UnicodeBinaryProperties.TryResolveProperty("White_Space", out var prop);
        Assert.True(result);
        Assert.Equal(BinaryProperty.White_Space, prop);
    }

    [Fact]
    public void TryResolveProperty_Hex_Digit_Success()
    {
        var result = UnicodeBinaryProperties.TryResolveProperty("Hex_Digit", out var prop);
        Assert.True(result);
        Assert.Equal(BinaryProperty.Hex_Digit, prop);
    }

    [Fact]
    public void TryResolveProperty_Default_Ignorable_Code_Point_Success()
    {
        var result = UnicodeBinaryProperties.TryResolveProperty("Default_Ignorable_Code_Point", out var prop);
        Assert.True(result);
        Assert.Equal(BinaryProperty.Default_Ignorable_Code_Point, prop);
    }

    [Fact]
    public void TryResolveProperty_Noncharacter_Code_Point_Success()
    {
        var result = UnicodeBinaryProperties.TryResolveProperty("Noncharacter_Code_Point", out var prop);
        Assert.True(result);
        Assert.Equal(BinaryProperty.Noncharacter_Code_Point, prop);
    }

    // ── Alias variations (case, underscores, hyphens) ─────────────────────────

    [Theory]
    [InlineData("White_Space")]
    [InlineData("whitespace")]
    [InlineData("white-space")]
    [InlineData("WHITE_SPACE")]
    [InlineData("white_space")]
    public void TryResolveProperty_WhiteSpace_Aliases(string alias)
    {
        var result = UnicodeBinaryProperties.TryResolveProperty(alias, out var prop);
        Assert.True(result, $"alias '{alias}' failed to resolve");
        Assert.Equal(BinaryProperty.White_Space, prop);
    }

    [Theory]
    [InlineData("Alphabetic")]
    [InlineData("alphabetic")]
    [InlineData("ALPHABETIC")]
    public void TryResolveProperty_Alphabetic_CaseInsensitive(string alias)
    {
        var result = UnicodeBinaryProperties.TryResolveProperty(alias, out var prop);
        Assert.True(result, $"alias '{alias}' failed to resolve");
        Assert.Equal(BinaryProperty.Alphabetic, prop);
    }

    [Fact]
    public void TryResolveProperty_Empty_ReturnsFalse()
    {
        Assert.False(UnicodeBinaryProperties.TryResolveProperty("", out _));
    }

    [Fact]
    public void TryResolveProperty_Unknown_ReturnsFalse()
    {
        Assert.False(UnicodeBinaryProperties.TryResolveProperty("Foobar_Property", out _));
    }

    // ── GetPropertySet — spot-checks ──────────────────────────────────────────

    [Fact]
    public void GetPropertySet_Alphabetic_ContainsLatinLetters()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Alphabetic, UnicodeVersion.V15_1_0);

        // A-Z
        for (int cp = 0x41; cp <= 0x5A; cp++)
            Assert.True(set.Contains(CodePoint.Create(cp)), $"U+{cp:X4} not in Alphabetic");

        // a-z
        for (int cp = 0x61; cp <= 0x7A; cp++)
            Assert.True(set.Contains(CodePoint.Create(cp)), $"U+{cp:X4} not in Alphabetic");
    }

    [Fact]
    public void GetPropertySet_Alphabetic_ContainsGreek()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Alphabetic, UnicodeVersion.V15_1_0);

        // U+0370 GREEK CAPITAL LETTER HETA
        Assert.True(set.Contains(CodePoint.Create(0x0370)));
        // U+03A9 GREEK CAPITAL LETTER OMEGA
        Assert.True(set.Contains(CodePoint.Create(0x03A9)));
    }

    [Fact]
    public void GetPropertySet_Alphabetic_ContainsCyrillic()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Alphabetic, UnicodeVersion.V15_1_0);

        // U+0410 CYRILLIC CAPITAL LETTER A
        Assert.True(set.Contains(CodePoint.Create(0x0410)));
    }

    [Fact]
    public void GetPropertySet_WhiteSpace_ContainsCoreChars()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.White_Space, UnicodeVersion.V15_1_0);

        Assert.True(set.Contains(CodePoint.Create(0x0020)), "SPACE");
        Assert.True(set.Contains(CodePoint.Create(0x0009)), "HT");
        Assert.True(set.Contains(CodePoint.Create(0x000A)), "LF");
        Assert.True(set.Contains(CodePoint.Create(0x000D)), "CR");
        Assert.True(set.Contains(CodePoint.Create(0x00A0)), "NBSP");
    }

    [Fact]
    public void GetPropertySet_WhiteSpace_NotContainsNonWhitespace()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.White_Space, UnicodeVersion.V15_1_0);

        Assert.False(set.Contains(CodePoint.Create(0x0041)), "LATIN CAPITAL LETTER A should not be whitespace");
        Assert.False(set.Contains(CodePoint.Create(0x0030)), "DIGIT ZERO should not be whitespace");
    }

    [Fact]
    public void GetPropertySet_HexDigit_Contains42CodePoints()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Hex_Digit, UnicodeVersion.V15_1_0);

        // 0-9, A-F, a-f (ASCII = 22) + fullwidth variants (22 more) = 44
        Assert.Equal(44, set.Count);
    }

    [Fact]
    public void GetPropertySet_HexDigit_ContainsAsciiHexChars()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Hex_Digit, UnicodeVersion.V15_1_0);

        // ASCII digits
        for (int cp = 0x30; cp <= 0x39; cp++)
            Assert.True(set.Contains(CodePoint.Create(cp)), $"U+{cp:X4}");

        // A-F
        for (int cp = 0x41; cp <= 0x46; cp++)
            Assert.True(set.Contains(CodePoint.Create(cp)), $"U+{cp:X4}");

        // a-f
        for (int cp = 0x61; cp <= 0x66; cp++)
            Assert.True(set.Contains(CodePoint.Create(cp)), $"U+{cp:X4}");
    }

    [Fact]
    public void GetPropertySet_NoncharacterCodePoint_ContainsFDD0Range()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Noncharacter_Code_Point, UnicodeVersion.V15_1_0);

        // U+FDD0..U+FDEF
        for (int cp = 0xFDD0; cp <= 0xFDEF; cp++)
            Assert.True(set.Contains(CodePoint.Create(cp)), $"U+{cp:X4}");

        // U+FFFE, U+FFFF
        Assert.True(set.Contains(CodePoint.Create(0xFFFE)));
        Assert.True(set.Contains(CodePoint.Create(0xFFFF)));
    }

    [Fact]
    public void GetPropertySet_NoncharacterCodePoint_ContainsSuppPlane()
    {
        var set = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Noncharacter_Code_Point, UnicodeVersion.V15_1_0);

        // Each supplementary plane ends with FFFE and FFFF noncharacters
        Assert.True(set.Contains(CodePoint.Create(0x1FFFE)));
        Assert.True(set.Contains(CodePoint.Create(0x1FFFF)));
        Assert.True(set.Contains(CodePoint.Create(0x10FFFE)));
        Assert.True(set.Contains(CodePoint.Create(0x10FFFF)));
    }

    // ── GetPropertySet by string ───────────────────────────────────────────────

    [Fact]
    public void GetPropertySet_ByString_Alphabetic_Works()
    {
        var set1 = UnicodeBinaryProperties.GetPropertySet("Alphabetic", UnicodeVersion.V15_1_0);
        var set2 = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.Alphabetic, UnicodeVersion.V15_1_0);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void GetPropertySet_ByString_WhiteSpace_Alias_Works()
    {
        var set1 = UnicodeBinaryProperties.GetPropertySet("whitespace", UnicodeVersion.V15_1_0);
        var set2 = UnicodeBinaryProperties.GetPropertySet(BinaryProperty.White_Space, UnicodeVersion.V15_1_0);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void GetPropertySet_ByString_Unknown_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            UnicodeBinaryProperties.GetPropertySet("NoSuchProperty", UnicodeVersion.V15_1_0));
    }
}
