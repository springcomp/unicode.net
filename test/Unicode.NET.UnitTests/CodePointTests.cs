using Xunit;

namespace Unicode.NET.UnitTests;

public class CodePointTests
{
    [Theory]
    [InlineData(0x0000)]
    [InlineData(0x10FFFF)]
    [InlineData(0x0041)]
    [InlineData(0xD7FF)]
    [InlineData(0xD800)]
    [InlineData(0xDBFF)]
    [InlineData(0xDC00)]
    [InlineData(0xDFFF)]
    [InlineData(0xE000)]
    public void Create_AcceptsValuesInRange(int value)
    {
        var codePoint = CodePoint.Create(value);
        Assert.Equal(value, codePoint.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0x110000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Create_ThrowsForValuesOutOfRange(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CodePoint.Create(value));
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0x0000, true)]
    [InlineData(0x10FFFF, true)]
    [InlineData(0x110000, false)]
    public void TryCreate_ReturnsExpectedResult(int value, bool expected)
    {
        Assert.Equal(expected, CodePoint.TryCreate(value, out _));
    }

    [Theory]
    [InlineData(0xD7FF, false, false, false)]
    [InlineData(0xD800, true, true, false)]
    [InlineData(0xDBFF, true, true, false)]
    [InlineData(0xDC00, true, false, true)]
    [InlineData(0xDFFF, true, false, true)]
    [InlineData(0xE000, false, false, false)]
    public void SurrogateClassification_IsCorrectAtBoundaries(int value, bool isSurrogate, bool isHigh, bool isLow)
    {
        var codePoint = CodePoint.Create(value);

        Assert.Equal(isSurrogate, codePoint.IsSurrogate);
        Assert.Equal(isHigh, codePoint.IsHighSurrogate);
        Assert.Equal(isLow, codePoint.IsLowSurrogate);
        Assert.Equal(!isSurrogate, codePoint.IsScalarValue);
    }

    [Theory]
    [InlineData(0xD800)]
    [InlineData(0xDFFF)]
    public void CreateScalar_ThrowsForSurrogateValues(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CodePoint.CreateScalar(value));
    }

    [Theory]
    [InlineData(0x0041)]
    [InlineData(0x10FFFF)]
    public void CreateScalar_AcceptsNonSurrogateValues(int value)
    {
        var codePoint = CodePoint.CreateScalar(value);
        Assert.Equal(value, codePoint.Value);
    }

    [Fact]
    public void Equality_IsStructural()
    {
        var a = CodePoint.Create(0x1F600);
        var b = CodePoint.Create(0x1F600);
        var c = CodePoint.Create(0x0041);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.False(a.Equals(c));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void CompareTo_OrdersByValue()
    {
        var low = CodePoint.Create(0x0041);
        var high = CodePoint.Create(0x1F600);

        Assert.True(low.CompareTo(high) < 0);
        Assert.True(high.CompareTo(low) > 0);
        Assert.Equal(0, low.CompareTo(low));
        Assert.True(low < high);
        Assert.True(high > low);
    }

    [Fact]
    public void ToString_FormatsAsUPlusHex()
    {
        Assert.Equal("U+0041", CodePoint.Create(0x0041).ToString());
        Assert.Equal("U+1F600", CodePoint.Create(0x1F600).ToString());
    }
}
