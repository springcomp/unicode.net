using Xunit;
using Unicode.NET;

public class XPathShortcutsTests
{
    [Fact]
    public void Digit_Contains_Expected()
    {
        var d = XPathShortcuts.Digit;
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
        var s = XPathShortcuts.Space;
        Assert.True(s.Contains(CodePoint.Create(' ')));
        Assert.True(s.Contains(CodePoint.Create('\t')));
        Assert.True(s.Contains(CodePoint.Create('\r')));
        Assert.True(s.Contains(CodePoint.Create('\n')));
        Assert.True(s.Contains(CodePoint.Create('\u00A0'))); // NBSP is White_Space
        Assert.False(s.Contains(CodePoint.Create('\u200B'))); // ZWSP is not White_Space
        Assert.Equal(UnicodeBinaryProperties.GetPropertySet(BinaryProperty.White_Space, UnicodeVersion.V15_1_0), s);
    }

    [Fact]
    public void Word_Contains_Expected()
    {
        var w = XPathShortcuts.Word;
        Assert.True(w.Contains(CodePoint.Create('A')));
        Assert.True(w.Contains(CodePoint.Create('z')));
        Assert.True(w.Contains(CodePoint.Create('0')));
        Assert.False(w.Contains(CodePoint.Create('_'))); // '_' is Pc (Punctuation), excluded
        Assert.False(w.Contains(CodePoint.Create('-')));
        Assert.True(w.Contains(CodePoint.Create('\u03B1'))); // Greek alpha is a Letter
    }
}

