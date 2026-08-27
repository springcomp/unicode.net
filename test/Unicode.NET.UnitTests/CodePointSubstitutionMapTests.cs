using Xunit;

namespace Unicode.NET.UnitTests;

public class CodePointSubstitutionMapTests
{
    [Fact]
    public void Replace_MapsBmpAndPreservesUnmappedInput()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('a')] = "XY"
        });

        Assert.Equal("XYbc", map.Replace("abc"));
    }

    [Fact]
    public void Replace_MapsSupplementaryScalarAsOneKey()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create(0x1F600)] = "grin"
        });

        Assert.Equal("grin!", map.Replace("\U0001F600!"));
    }

    [Fact]
    public void Replace_AllowsEmptyAndOneToManyReplacements()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('x')] = "",
            [CodePoint.Create('y')] = "one-to-many"
        });

        Assert.Equal("one-to-manyz", map.Replace("xyz"));
    }

    [Fact]
    public void Replace_DoesNotRecursivelyRemapReplacementText()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('a')] = "b",
            [CodePoint.Create('b')] = "c"
        });

        Assert.Equal("b", map.Replace("a"));
    }

    [Fact]
    public void Constructor_CopiesMappings()
    {
        var mappings = new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('a')] = "first"
        };
        var map = new CodePointSubstitutionMap(mappings);
        mappings[CodePoint.Create('a')] = "changed";

        Assert.Equal("first", map.Replace("a"));
    }

    [Fact]
    public void TryGetReplacement_ReturnsMapping()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('a')] = "A"
        });

        Assert.True(map.TryGetReplacement(CodePoint.Create('a'), out var replacement));
        Assert.Equal("A", replacement);
        Assert.False(map.TryGetReplacement(CodePoint.Create('b'), out _));
    }

    [Fact]
    public void Replace_LoneHighSurrogate_Throws()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>());
        Assert.Throws<ArgumentException>(() => map.Replace("\uD800"));
    }

    [Fact]
    public void Replace_LoneLowSurrogate_Throws()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>());
        Assert.Throws<ArgumentException>(() => map.Replace("\uDC00"));
    }

    [Fact]
    public void Replace_UnpairedSurrogateBetweenScalars_Throws()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>());
        Assert.Throws<ArgumentException>(() => map.Replace("a\uD800b"));
    }

    [Fact]
    public void Constructor_SurrogateKey_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CodePointSubstitutionMap(
            new Dictionary<CodePoint, string> { [CodePoint.Create(0xD800)] = "x" }));
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new CodePointSubstitutionMap(null!));
        Assert.Throws<ArgumentNullException>(() => new CodePointSubstitutionMap(
            new Dictionary<CodePoint, string> { [CodePoint.Create('a')] = null! }));
    }

    [Fact]
    public void Replace_NullInput_Throws()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>());

        Assert.Throws<ArgumentNullException>(() => map.Replace(null!));
    }

    [Theory]
    [InlineData(0xD800)]
    [InlineData(0xDBFF)]
    [InlineData(0xDC00)]
    [InlineData(0xDFFF)]
    public void TryGetReplacement_SurrogateInput_Throws(int value)
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>());

        Assert.Throws<ArgumentException>(() => map.TryGetReplacement(CodePoint.Create(value), out _));
    }

    [Fact]
    public void Replace_PreservesReplacementTextWithoutRecursiveMapping()
    {
        var replacement = "a\uD800";
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('x')] = replacement,
            [CodePoint.Create('a')] = "changed"
        });

        Assert.Equal(replacement, map.Replace("x"));
    }

    [Fact]
    public void Replace_EmptyInput_ReturnsEmptyString()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('a')] = "X"
        });

        Assert.Equal("", map.Replace(""));
    }

    [Fact]
    public void Replace_AllMappedToEmpty_ReturnsEmptyString()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('a')] = "",
            [CodePoint.Create('b')] = ""
        });

        Assert.Equal("", map.Replace("ab"));
    }

    [Theory]
    [InlineData(0x10000)]  // first supplementary scalar
    [InlineData(0x1F600)]  // emoji, mid-range
    [InlineData(0x1D11E)]  // musical symbol
    [InlineData(0x10FFFF)] // highest valid scalar
    public void Replace_SupplementaryScalars_AreMappedAsSingleKeysNeverSplitIntoSurrogateHalves(int codePoint)
    {
        var scalar = CodePoint.Create(codePoint);
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [scalar] = "MAPPED"
        });

        string input = Utf16.Encode(scalar);
        Assert.Equal("MAPPED", map.Replace(input));

        // Sanity: a map keyed only by one surrogate half must not match the supplementary scalar.
        var highOnlyMap = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>());
        Assert.False(highOnlyMap.TryGetReplacement(scalar, out _) && scalar.IsSurrogate);
    }

    [Fact]
    public void Replace_SupplementaryToSupplementaryMapping_PreservesSurrogatePairing()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create(0x10400)] = Utf16.Encode(CodePoint.Create(0x1F600))
        });

        string result = map.Replace(Utf16.Encode(CodePoint.Create(0x10400)));
        Assert.Equal(Utf16.Encode(CodePoint.Create(0x1F600)), result);

        // The result must decode back to exactly one supplementary scalar, not stray surrogates.
        var decoded = new List<int>();
        foreach (var cp in result.EnumerateCodePoints())
            decoded.Add(cp.Value);
        Assert.Equal(new[] { 0x1F600 }, decoded);
    }

    [Fact]
    public void Replace_WellFormedSurrogatePair_ForUnmappedScalar_IsPreservedUnsplit()
    {
        var map = new CodePointSubstitutionMap(new Dictionary<CodePoint, string>
        {
            [CodePoint.Create('a')] = "X"
        });

        string input = "a" + Utf16.Encode(CodePoint.Create(0x1F600));
        string result = map.Replace(input);
        Assert.Equal("X" + Utf16.Encode(CodePoint.Create(0x1F600)), result);
    }
}
