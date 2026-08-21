using Xunit;

namespace Unicode.NET.UnitTests;

public class CodePointRangeTests
{
    // ── Construction ────────────────────────────────────────────────────────

    [Fact]
    public void Ctor_SingletonRange_StartEqualsEnd()
    {
        var r = new CodePointRange(CodePoint.Create(0x41));
        Assert.Equal(0x41, r.Start.Value);
        Assert.Equal(0x41, r.End.Value);
    }

    [Fact]
    public void Ctor_ValidRange_Succeeds()
    {
        var r = CodePointRange.Create(0x41, 0x5A);
        Assert.Equal(0x41, r.Start.Value);
        Assert.Equal(0x5A, r.End.Value);
    }

    [Fact]
    public void Ctor_EndBeforeStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => CodePointRange.Create(0x5A, 0x41));
    }

    [Fact]
    public void Ctor_MaxBoundary_Succeeds()
    {
        var r = CodePointRange.Create(0, CodePoint.MaxValue);
        Assert.Equal(CodePoint.MaxValue, r.End.Value);
    }

    // ── Contains ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0x41, true)]
    [InlineData(0x5A, true)]
    [InlineData(0x4D, true)]
    [InlineData(0x40, false)]
    [InlineData(0x5B, false)]
    public void Contains_InclusiveBoundaries(int value, bool expected)
    {
        var r = CodePointRange.Create(0x41, 0x5A);
        Assert.Equal(expected, r.Contains(CodePoint.Create(value)));
    }

    [Fact]
    public void Contains_Singleton_OnlyItsValue()
    {
        var r = new CodePointRange(CodePoint.Create(0x41));
        Assert.True(r.Contains(CodePoint.Create(0x41)));
        Assert.False(r.Contains(CodePoint.Create(0x40)));
        Assert.False(r.Contains(CodePoint.Create(0x42)));
    }

    // ── Overlaps ────────────────────────────────────────────────────────────

    [Fact]
    public void Overlaps_SharedCodePoint_True()
    {
        var a = CodePointRange.Create(0x00, 0x10);
        var b = CodePointRange.Create(0x10, 0x20);
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_Disjoint_False()
    {
        var a = CodePointRange.Create(0x00, 0x05);
        var b = CodePointRange.Create(0x07, 0x0F);
        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_Adjacent_False()
    {
        var a = CodePointRange.Create(0x00, 0x05);
        var b = CodePointRange.Create(0x06, 0x0F);
        Assert.False(a.Overlaps(b));
    }

    // ── IsAdjacentTo ────────────────────────────────────────────────────────

    [Fact]
    public void IsAdjacentTo_DirectlyAdjacent_True()
    {
        var a = CodePointRange.Create(0x00, 0x05);
        var b = CodePointRange.Create(0x06, 0x0F);
        Assert.True(a.IsAdjacentTo(b));
        Assert.True(b.IsAdjacentTo(a));
    }

    [Fact]
    public void IsAdjacentTo_GapBetween_False()
    {
        var a = CodePointRange.Create(0x00, 0x05);
        var b = CodePointRange.Create(0x07, 0x0F);
        Assert.False(a.IsAdjacentTo(b));
    }

    [Fact]
    public void IsAdjacentTo_MaxBoundary_False()
    {
        // U+10FFFF has no successor; range ending at max cannot be adjacent to anything
        var a = CodePointRange.Create(0x10FFFE, CodePoint.MaxValue);
        var b = CodePointRange.Create(CodePoint.MaxValue, CodePoint.MaxValue);
        // b overlaps a; adjacency beyond max is impossible
        Assert.False(a.IsAdjacentTo(b));
    }

    [Fact]
    public void IsAdjacentTo_RangeEndingAtMaxMinusOne()
    {
        var a = CodePointRange.Create(0x10FFFE, 0x10FFFE);
        var b = CodePointRange.Create(CodePoint.MaxValue, CodePoint.MaxValue);
        Assert.True(a.IsAdjacentTo(b));
    }

    // ── Equality / hashing ──────────────────────────────────────────────────

    [Fact]
    public void Equality_SameRange_Equal()
    {
        var a = CodePointRange.Create(0x41, 0x5A);
        var b = CodePointRange.Create(0x41, 0x5A);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentRange_NotEqual()
    {
        var a = CodePointRange.Create(0x41, 0x5A);
        var b = CodePointRange.Create(0x61, 0x7A);
        Assert.NotEqual(a, b);
    }

    // ── ToString ────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_MultiValue_DotDotFormat()
    {
        var r = CodePointRange.Create(0x41, 0x5A);
        Assert.Equal("U+0041..U+005A", r.ToString());
    }

    [Fact]
    public void ToString_Singleton_SingleValue()
    {
        var r = new CodePointRange(CodePoint.Create(0x41));
        Assert.Equal("U+0041", r.ToString());
    }
}
