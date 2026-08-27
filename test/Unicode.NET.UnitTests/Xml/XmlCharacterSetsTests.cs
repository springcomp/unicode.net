using Unicode.NET.Xml;
using Xunit;

namespace Unicode.NET.UnitTests.Xml;

public class XmlCharacterSetsTests
{
    [Fact]
    public void NameStartChar_MatchesSpecBoundaries()
    {
        int[] included =
        {
            0x003A,
            0x0041,
            0x005A,
            0x005F,
            0x0061,
            0x007A,
            0x00C0,
            0x00D6,
            0x00D8,
            0x00F6,
            0x00F8,
            0x02FF,
            0x0370,
            0x037D,
            0x037F,
            0x1FFF,
            0x200C,
            0x200D,
            0x2070,
            0x218F,
            0x2C00,
            0x2FEF,
            0x3001,
            0xD7FF,
            0xF900,
            0xFDCF,
            0xFDF0,
            0xFFFD,
            0x10000,
            0xEFFFF,
        };

        foreach (var value in included)
        {
            Assert.True(XmlCharacterSets.NameStartChar.Contains(CodePoint.Create(value)), $"Expected NameStartChar to include U+{value:X}");
        }

        int[] excluded =
        {
            0x002D,
            0x002E,
            0x0030,
            0x0039,
            0x00B7,
            0x0300,
            0x037E,
            0x200B,
            0x200E,
            0x2190,
            0x2FF0,
            0x3000,
            0xD800,
            0xE000,
            0xFDD0,
            0xFFFE,
            0xFFFF,
        };

        foreach (var value in excluded)
        {
            Assert.False(XmlCharacterSets.NameStartChar.Contains(CodePoint.Create(value)), $"Expected NameStartChar to exclude U+{value:X}");
        }
    }

    [Fact]
    public void NameChar_ExtendsNameStartCharWithDigitsHyphenDotMiddleDotAndCombiningMarks()
    {
        foreach (var range in XmlCharacterSets.NameStartChar.Ranges)
        {
            Assert.True(XmlCharacterSets.NameChar.Contains(range.Start), $"NameChar should include start U+{range.Start:X}");
            Assert.True(XmlCharacterSets.NameChar.Contains(range.End), $"NameChar should include end U+{range.End:X}");
        }

        int[] included =
        {
            0x002D,
            0x002E,
            0x0030,
            0x0039,
            0x00B7,
            0x0300,
            0x036F,
            0x203F,
            0x2040,
        };

        foreach (var value in included)
        {
            Assert.True(XmlCharacterSets.NameChar.Contains(CodePoint.Create(value)), $"Expected NameChar to include U+{value:X}");
        }

        int[] excluded =
        {
            0x002F,
            0x00B6,
            0x037E,
            0x200B,
        };

        foreach (var value in excluded)
        {
            Assert.False(XmlCharacterSets.NameChar.Contains(CodePoint.Create(value)), $"Expected NameChar to exclude U+{value:X}");
        }
    }

    [Fact]
    public void Char_IncludesAndExcludesExpectedControlCharacters()
    {
        int[] included = { 0x0009, 0x000A, 0x000D, 0x0020 };
        foreach (var value in included)
        {
            Assert.True(XmlCharacterSets.Char.Contains(CodePoint.Create(value)), $"Char should include U+{value:X}");
        }

        int[] excluded = { 0x0000, 0x0001, 0x0008, 0x000B, 0x000C, 0x000E, 0x001F };
        foreach (var value in excluded)
        {
            Assert.False(XmlCharacterSets.Char.Contains(CodePoint.Create(value)), $"Char should exclude U+{value:X}");
        }
    }

    [Fact]
    public void Char_ExcludesSurrogatesAndNonCharactersAndIncludesRangeBoundaries()
    {
        int[] included = { 0xD7FF, 0xE000, 0xFFFD, 0x10000, 0x10FFFF };
        foreach (var value in included)
        {
            Assert.True(XmlCharacterSets.Char.Contains(CodePoint.Create(value)), $"Char should include U+{value:X}");
        }

        int[] excluded = { 0xD800, 0xDFFF, 0xFFFE, 0xFFFF };
        foreach (var value in excluded)
        {
            Assert.False(XmlCharacterSets.Char.Contains(CodePoint.Create(value)), $"Char should exclude U+{value:X}");
        }
    }

    [Fact]
    public void PredicateHelpers_DelegateToXmlSets()
    {
        var values = new[] { 0x003A, 0x0030, 0x000A, 0x1F600 };
        foreach (var value in values)
        {
            var codePoint = CodePoint.Create(value);
            Assert.Equal(XmlCharacterSets.Char.Contains(codePoint), XmlCharacterSets.IsChar(codePoint));
            Assert.Equal(XmlCharacterSets.NameStartChar.Contains(codePoint), XmlCharacterSets.IsNameStartChar(codePoint));
            Assert.Equal(XmlCharacterSets.NameChar.Contains(codePoint), XmlCharacterSets.IsNameChar(codePoint));
            Assert.Equal(XmlCharacterSets.Whitespace.Contains(codePoint), XmlCharacterSets.IsWhitespace(codePoint));
        }
    }

    [Fact]
    public void Whitespace_IsExactSet()
    {
        var whitespace = XmlCharacterSets.Whitespace;
        int[] expected = { 0x9, 0xA, 0xD, 0x20 };

        foreach (var value in expected)
            Assert.True(whitespace.Contains(CodePoint.Create(value)), $"Whitespace should include U+{value:X}");

        // Spot-check neighbors
        Assert.False(whitespace.Contains(CodePoint.Create(0x0008)));
        Assert.False(whitespace.Contains(CodePoint.Create(0x000B)));
        Assert.False(whitespace.Contains(CodePoint.Create(0x000C)));
        Assert.False(whitespace.Contains(CodePoint.Create(0x000E)));
        Assert.False(whitespace.Contains(CodePoint.Create(0x001F)));
        Assert.False(whitespace.Contains(CodePoint.Create(0x0021)));
        Assert.Equal(4, whitespace.Count);
    }

    [Fact]
    public void XmlSets_AreSharedInstances()
    {
        Assert.Same(XmlCharacterSets.Char, XmlCharacterSets.Char);
        Assert.Same(XmlCharacterSets.NameStartChar, XmlCharacterSets.NameStartChar);
        Assert.Same(XmlCharacterSets.NameChar, XmlCharacterSets.NameChar);
        Assert.Same(XmlCharacterSets.Whitespace, XmlCharacterSets.Whitespace);
    }

    [Fact]
    public void XmlSets_RejectEverySurrogate()
    {
        for (int value = CodePoint.HighSurrogateStart; value <= CodePoint.LowSurrogateEnd; value++)
        {
            var codePoint = CodePoint.Create(value);
            Assert.False(XmlCharacterSets.IsChar(codePoint));
            Assert.False(XmlCharacterSets.IsNameStartChar(codePoint));
            Assert.False(XmlCharacterSets.IsNameChar(codePoint));
        }
    }
}
