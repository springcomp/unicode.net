using System.Collections.Generic;
using Xunit;
using Unicode.NET;

public class XPathShortcutsTests
{
    public static IEnumerable<object[]> SupportedVersions =>
        new[]
        {
            new object[] { UnicodeVersion.V15_1_0 },
            new object[] { UnicodeVersion.V16_0_0 }
        };

    [Theory]
    [MemberData(nameof(SupportedVersions))]
    public void Digit_MatchesCategorySet_ForEachVersion(UnicodeVersion version)
    {
        Assert.Equal(
            UnicodeData.GetCategorySet("Nd", version),
            XPathShortcuts.Digit(version));
    }

    [Theory]
    [MemberData(nameof(SupportedVersions))]
    public void Word_MatchesExpectedCategoryComplement_ForEachVersion(UnicodeVersion version)
    {
        var punctuation = UnicodeData.GetCategorySet("P", version);
        var separators = UnicodeData.GetCategorySet("Z", version);
        var other = UnicodeData.GetCategorySet("C", version);
        var expected = CodePointSet.All.Subtract(punctuation.Union(separators).Union(other));

        Assert.Equal(expected, XPathShortcuts.Word(version));
    }

    [Fact]
    public void Word_DiffersBetweenUnicodeVersions()
    {
        // Unicode 16.0 assigns code points that were unassigned in 15.1.
        Assert.NotEqual(
            XPathShortcuts.Word(UnicodeVersion.V15_1_0),
            XPathShortcuts.Word(UnicodeVersion.V16_0_0));
    }

    [Fact]
    public void Digit_Contains_Expected()
    {
        var d = XPathShortcuts.Digit();
        Assert.True(d.Contains(CodePoint.Create('0')));
        Assert.True(d.Contains(CodePoint.Create('9')));
        Assert.False(d.Contains(CodePoint.Create('A')));
        Assert.True(d.Contains(CodePoint.Create('\u0660'))); // Arabic-Indic digit (Nd)
        Assert.True(d.RangeCount > 1);
        Assert.Equal(UnicodeData.GetCategorySet("Nd", UnicodeVersion.V15_1_0), d);
    }

    [Fact]
    public void Space_Contains_Expected()
    {
        var s = XPathShortcuts.Space();
        Assert.True(s.Contains(CodePoint.Create(' ')));
        Assert.True(s.Contains(CodePoint.Create('\t')));
        Assert.True(s.Contains(CodePoint.Create('\r')));
        Assert.True(s.Contains(CodePoint.Create('\n')));
        // Per XSD/XPath spec \s == [#x20\t\n\r]: ASCII-only, NOT Unicode White_Space.
        Assert.False(s.Contains(CodePoint.Create('\u00A0'))); // NBSP excluded
        Assert.False(s.Contains(CodePoint.Create('\u0085'))); // NEL excluded
        Assert.False(s.Contains(CodePoint.Create('\u2028'))); // line separator excluded
        Assert.False(s.Contains(CodePoint.Create('\u200B'))); // ZWSP excluded
        Assert.Equal(4, s.Count);
    }

    [Fact]
    public void Cache_ReturnsStableInstances()
    {
        Assert.Same(
            XPathShortcuts.Digit(UnicodeVersion.V15_1_0),
            XPathShortcuts.Digit(UnicodeVersion.V15_1_0));
        Assert.Same(
            XPathShortcuts.Word(UnicodeVersion.V16_0_0),
            XPathShortcuts.Word(UnicodeVersion.V16_0_0));
        Assert.Same(XPathShortcuts.Space(), XPathShortcuts.Space());
    }

    [Fact]
    public void Digit_DefaultVersionMatchesCurrent()
    {
        Assert.Equal(
            XPathShortcuts.Digit(UnicodeVersion.Current),
            XPathShortcuts.Digit());
    }

    [Fact]
    public void Word_Contains_Expected()
    {
        var w = XPathShortcuts.Word();
        Assert.True(w.Contains(CodePoint.Create('A')));
        Assert.True(w.Contains(CodePoint.Create('z')));
        Assert.True(w.Contains(CodePoint.Create('0')));
        Assert.False(w.Contains(CodePoint.Create('_'))); // '_' is Pc (Punctuation), excluded
        Assert.False(w.Contains(CodePoint.Create('-')));
        Assert.True(w.Contains(CodePoint.Create('\u03B1'))); // Greek alpha is a Letter
    }
}
