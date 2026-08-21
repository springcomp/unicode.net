using Xunit;

namespace Unicode.NET.UnitTests;

public class Utf16Tests
{
    [Theory]
    [InlineData(0x0041, 1)]
    [InlineData(0xFFFF, 1)]
    [InlineData(0x10000, 2)]
    [InlineData(0x10FFFF, 2)]
    public void Utf16CodeUnitCount_ReturnsExpectedCount(int value, int expected)
    {
        Assert.Equal(expected, Utf16.Utf16CodeUnitCount(CodePoint.Create(value)));
    }

    [Fact]
    public void TryEncode_BmpCodePoint_WritesSingleChar()
    {
        Span<char> destination = stackalloc char[2];
        bool result = Utf16.TryEncode(CodePoint.Create(0x0041), destination, out int charsWritten);

        Assert.True(result);
        Assert.Equal(1, charsWritten);
        Assert.Equal('A', destination[0]);
    }

    [Fact]
    public void TryEncode_SupplementaryCodePoint_WritesSurrogatePair()
    {
        // U+1F600 GRINNING FACE => surrogate pair D83D DE00
        Span<char> destination = stackalloc char[2];
        bool result = Utf16.TryEncode(CodePoint.Create(0x1F600), destination, out int charsWritten);

        Assert.True(result);
        Assert.Equal(2, charsWritten);
        Assert.Equal(0xD83D, destination[0]);
        Assert.Equal(0xDE00, destination[1]);
    }

    [Fact]
    public void TryEncode_DestinationTooSmall_ReturnsFalse()
    {
        Span<char> destination = stackalloc char[1];
        bool result = Utf16.TryEncode(CodePoint.Create(0x1F600), destination, out int charsWritten);

        Assert.False(result);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void Encode_RoundTripsThroughString()
    {
        string s = Utf16.Encode(CodePoint.Create(0x1F600));
        Assert.Equal(2, s.Length);
        Assert.Equal("\uD83D\uDE00", s);
    }

    [Fact]
    public void Decode_BmpChar_ConsumesOneChar()
    {
        Utf16.Decode("A", out var value, out int charsConsumed);

        Assert.Equal(1, charsConsumed);
        Assert.Equal(0x0041, value.Value);
    }

    [Fact]
    public void Decode_SurrogatePair_ConsumesTwoCharsAndDecodesSupplementaryCodePoint()
    {
        Utf16.Decode("\uD83D\uDE00", out var value, out int charsConsumed);

        Assert.Equal(2, charsConsumed);
        Assert.Equal(0x1F600, value.Value);
    }

    [Fact]
    public void Decode_LoneHighSurrogate_DecodesPermissivelyAsSurrogateCodePoint()
    {
        Utf16.Decode("\uD83D", out var value, out int charsConsumed);

        Assert.Equal(1, charsConsumed);
        Assert.Equal(0xD83D, value.Value);
        Assert.True(value.IsHighSurrogate);
    }

    [Fact]
    public void Decode_LoneLowSurrogate_DecodesPermissivelyAsSurrogateCodePoint()
    {
        Utf16.Decode("\uDE00", out var value, out int charsConsumed);

        Assert.Equal(1, charsConsumed);
        Assert.Equal(0xDE00, value.Value);
        Assert.True(value.IsLowSurrogate);
    }

    [Fact]
    public void Decode_EmptySpan_Throws()
    {
        Assert.Throws<ArgumentException>(() => Utf16.Decode(ReadOnlySpan<char>.Empty, out _, out _));
    }

    [Theory]
    [InlineData(0x0000)]
    [InlineData(0x0041)]
    [InlineData(0xFFFF)]
    [InlineData(0x10000)]
    [InlineData(0x1F600)]
    [InlineData(0x10FFFF)]
    public void EncodeThenDecode_RoundTrips(int value)
    {
        var original = CodePoint.Create(value);
        string encoded = Utf16.Encode(original);

        Utf16.Decode(encoded, out var decoded, out int charsConsumed);

        Assert.Equal(original, decoded);
        Assert.Equal(encoded.Length, charsConsumed);
    }

    [Fact]
    public void EnumerateCodePoints_WalksMixedAsciiBmpAndSupplementaryString()
    {
        // "A" (ASCII), "é" (BMP U+00E9), "𝄞" (supplementary U+1D11E MUSICAL SYMBOL G CLEF)
        string s = "A\u00E9\uD834\uDD1E";

        var results = new List<int>();
        foreach (var codePoint in s.EnumerateCodePoints())
        {
            results.Add(codePoint.Value);
        }

        Assert.Equal([0x0041, 0x00E9, 0x1D11E], results);
    }

    [Fact]
    public void EnumerateCodePoints_LoneSurrogateInString_DoesNotThrow()
    {
        string s = "A\uD800B";

        var results = new List<int>();
        foreach (var codePoint in s.EnumerateCodePoints())
        {
            results.Add(codePoint.Value);
        }

        Assert.Equal([0x0041, 0xD800, 0x0042], results);
    }

    [Fact]
    public void EnumerateCodePoints_EmptyString_YieldsNothing()
    {
        var results = new List<int>();
        foreach (var codePoint in "".EnumerateCodePoints())
        {
            results.Add(codePoint.Value);
        }

        Assert.Empty(results);
    }
}
