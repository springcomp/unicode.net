using Xunit;

namespace Unicode.NET.UnitTests;

public class CodePointSetTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CodePointSet Set(params (int start, int end)[] ranges)
    {
        var b = new CodePointSetBuilder();
        foreach (var (s, e) in ranges)
            b.Add(CodePointRange.Create(s, e));
        return b.Build();
    }

    private static CodePointSet Set(params int[] singles)
    {
        var b = new CodePointSetBuilder();
        foreach (var v in singles)
            b.Add(CodePoint.Create(v));
        return b.Build();
    }

    // ── Empty / All ──────────────────────────────────────────────────────────

    [Fact]
    public void Empty_ContainsNothing()
    {
        Assert.False(CodePointSet.Empty.Contains(CodePoint.Create(0)));
        Assert.False(CodePointSet.Empty.Contains(CodePoint.Create(0x10FFFF)));
        Assert.Equal(0, CodePointSet.Empty.RangeCount);
        Assert.True(CodePointSet.Empty.IsEmpty);
    }

    [Fact]
    public void All_ContainsEveryBoundary()
    {
        Assert.True(CodePointSet.All.Contains(CodePoint.Create(0)));
        Assert.True(CodePointSet.All.Contains(CodePoint.Create(0xD800)));
        Assert.True(CodePointSet.All.Contains(CodePoint.Create(0xDFFF)));
        Assert.True(CodePointSet.All.Contains(CodePoint.Create(0xE000)));
        Assert.True(CodePointSet.All.Contains(CodePoint.Create(0x10FFFF)));
        Assert.Equal(1, CodePointSet.All.RangeCount);
    }

    // ── Builder coalescing ───────────────────────────────────────────────────

    [Fact]
    public void Builder_AdjacentRanges_Coalesced()
    {
        var s = Set((0, 5), (6, 10));
        Assert.Equal(1, s.RangeCount);
        Assert.Equal(0, s.Ranges.First().Start.Value);
        Assert.Equal(10, s.Ranges.First().End.Value);
    }

    [Fact]
    public void Builder_OverlappingRanges_Coalesced()
    {
        var s = Set((0, 5), (3, 10));
        Assert.Equal(1, s.RangeCount);
        Assert.Equal(0, s.Ranges.First().Start.Value);
        Assert.Equal(10, s.Ranges.First().End.Value);
    }

    [Fact]
    public void Builder_UnsortedInput_Sorted()
    {
        var s = Set((100, 200), (0, 50));
        var ranges = s.Ranges.ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(0, ranges[0].Start.Value);
        Assert.Equal(100, ranges[1].Start.Value);
    }

    [Fact]
    public void Builder_DuplicateRanges_Harmless()
    {
        var s = Set((10, 20), (10, 20));
        Assert.Equal(1, s.RangeCount);
    }

    [Fact]
    public void Builder_NoInput_ReturnsEmpty()
    {
        var b = new CodePointSetBuilder();
        Assert.Equal(CodePointSet.Empty, b.Build());
    }

    [Fact]
    public void Builder_ReuseAfterBuild_DoesNotMutatePreviousResult()
    {
        var b = new CodePointSetBuilder();
        b.Add(CodePointRange.Create(0x41, 0x5A));
        var first = b.Build();

        b.Add(CodePointRange.Create(0x61, 0x7A));
        var second = b.Build();

        // first must still have only one range
        Assert.Equal(1, first.RangeCount);
        Assert.Equal(2, second.RangeCount);
    }

    // ── Contains / membership ────────────────────────────────────────────────

    [Fact]
    public void Contains_Boundaries()
    {
        var s = Set((0x41, 0x5A));
        Assert.True(s.Contains(CodePoint.Create(0x41)));
        Assert.True(s.Contains(CodePoint.Create(0x5A)));
        Assert.False(s.Contains(CodePoint.Create(0x40)));
        Assert.False(s.Contains(CodePoint.Create(0x5B)));
    }

    [Fact]
    public void Contains_SurrogateRange()
    {
        var s = Set((0xD800, 0xDFFF));
        Assert.True(s.Contains(CodePoint.Create(0xD800)));
        Assert.True(s.Contains(CodePoint.Create(0xDFFF)));
        Assert.False(s.Contains(CodePoint.Create(0xD7FF)));
        Assert.False(s.Contains(CodePoint.Create(0xE000)));
    }

    // ── Union ────────────────────────────────────────────────────────────────

    [Fact]
    public void Union_WithEmpty_ReturnsSelf()
    {
        var a = Set((10, 20));
        Assert.Equal(a, a.Union(CodePointSet.Empty));
        Assert.Equal(a, CodePointSet.Empty.Union(a));
    }

    [Fact]
    public void Union_DisjointSets_BothRanges()
    {
        var a = Set((0, 5));
        var b = Set((10, 15));
        var u = a.Union(b);
        Assert.Equal(2, u.RangeCount);
    }

    [Fact]
    public void Union_OverlappingSets_Merged()
    {
        var a = Set((0, 10));
        var b = Set((5, 20));
        var u = a.Union(b);
        Assert.Equal(1, u.RangeCount);
        Assert.Equal(0, u.Ranges.First().Start.Value);
        Assert.Equal(20, u.Ranges.First().End.Value);
    }

    [Fact]
    public void Union_Commutativity()
    {
        var a = Set((0, 10), (30, 40));
        var b = Set((5, 35));
        Assert.Equal(a.Union(b), b.Union(a));
    }

    [Fact]
    public void Union_WithComplement_EqualsAll()
    {
        var a = Set((0x41, 0x5A));
        Assert.Equal(CodePointSet.All, a.Union(a.Complement()));
    }

    // ── Intersect ────────────────────────────────────────────────────────────

    [Fact]
    public void Intersect_WithEmpty_ReturnsEmpty()
    {
        var a = Set((10, 20));
        Assert.Equal(CodePointSet.Empty, a.Intersect(CodePointSet.Empty));
    }

    [Fact]
    public void Intersect_WithAll_ReturnsSelf()
    {
        var a = Set((10, 20));
        Assert.Equal(a, a.Intersect(CodePointSet.All));
    }

    [Fact]
    public void Intersect_SelfWithSelf_ReturnsSelf()
    {
        var a = Set((10, 20), (30, 40));
        Assert.Equal(a, a.Intersect(a));
    }

    [Fact]
    public void Intersect_Disjoint_ReturnsEmpty()
    {
        var a = Set((0, 5));
        var b = Set((10, 15));
        Assert.Equal(CodePointSet.Empty, a.Intersect(b));
    }

    [Fact]
    public void Intersect_Commutativity()
    {
        var a = Set((0, 20));
        var b = Set((10, 30));
        Assert.Equal(a.Intersect(b), b.Intersect(a));
    }

    // ── Subtract ─────────────────────────────────────────────────────────────

    [Fact]
    public void Subtract_Empty_ReturnsSelf()
    {
        var a = Set((10, 20));
        Assert.Equal(a, a.Subtract(CodePointSet.Empty));
    }

    [Fact]
    public void Subtract_Self_ReturnsEmpty()
    {
        var a = Set((10, 20));
        Assert.Equal(CodePointSet.Empty, a.Subtract(a));
    }

    [Fact]
    public void Subtract_All_ReturnsEmpty()
    {
        var a = Set((10, 20));
        Assert.Equal(CodePointSet.Empty, a.Subtract(CodePointSet.All));
    }

    [Fact]
    public void Subtract_SplitRange()
    {
        // A = [10..20], B = [12..15] union [18..25]
        // A \ B = [10..11] union [16..17]
        var a = Set((10, 20));
        var b = Set((12, 15), (18, 25));
        var result = a.Subtract(b);
        var ranges = result.Ranges.ToArray();
        Assert.Equal(2, ranges.Length);
        Assert.Equal(10, ranges[0].Start.Value);
        Assert.Equal(11, ranges[0].End.Value);
        Assert.Equal(16, ranges[1].Start.Value);
        Assert.Equal(17, ranges[1].End.Value);
    }

    [Fact]
    public void Subtract_ResultContainsNoMemberOfSubtrahend()
    {
        var a = Set((0, 100));
        var b = Set((20, 30), (50, 60));
        var result = a.Subtract(b);
        for (int v = 20; v <= 30; v++)
            Assert.False(result.Contains(CodePoint.Create(v)));
        for (int v = 50; v <= 60; v++)
            Assert.False(result.Contains(CodePoint.Create(v)));
    }

    // ── Complement ───────────────────────────────────────────────────────────

    [Fact]
    public void Complement_Empty_EqualsAll()
    {
        Assert.Equal(CodePointSet.All, CodePointSet.Empty.Complement());
    }

    [Fact]
    public void Complement_All_EqualsEmpty()
    {
        Assert.Equal(CodePointSet.Empty, CodePointSet.All.Complement());
    }

    [Fact]
    public void Complement_DoubleComplement_ReturnsSelf()
    {
        var a = Set((0x41, 0x5A), (0x61, 0x7A));
        Assert.Equal(a, a.Complement().Complement());
    }

    [Fact]
    public void Complement_CorrectBoundaries()
    {
        // complement of [0x0001..0x10FFFE] should be {0x0000} union {0x10FFFF}
        var a = Set((0x0001, 0x10FFFE));
        var c = a.Complement();
        Assert.True(c.Contains(CodePoint.Create(0)));
        Assert.True(c.Contains(CodePoint.Create(0x10FFFF)));
        Assert.False(c.Contains(CodePoint.Create(1)));
        Assert.False(c.Contains(CodePoint.Create(0x10FFFE)));
    }

    // ── Structural equality / hashing ────────────────────────────────────────

    [Fact]
    public void Equality_InsertionOrderIndependent()
    {
        var b1 = new CodePointSetBuilder();
        b1.Add(CodePointRange.Create(0x41, 0x5A));
        b1.Add(CodePointRange.Create(0x61, 0x7A));
        var s1 = b1.Build();

        var b2 = new CodePointSetBuilder();
        b2.Add(CodePointRange.Create(0x61, 0x7A));
        b2.Add(CodePointRange.Create(0x41, 0x5A));
        var s2 = b2.Build();

        Assert.Equal(s1, s2);
        Assert.Equal(s1.GetHashCode(), s2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentSets_NotEqual()
    {
        var a = Set((0x41, 0x5A));
        var b = Set((0x61, 0x7A));
        Assert.NotEqual(a, b);
    }

    // ── Enumeration ──────────────────────────────────────────────────────────

    [Fact]
    public void Enumeration_AscendingOrder()
    {
        var s = Set((0x61, 0x65), (0x41, 0x45));
        var values = s.ToList();
        for (int i = 1; i < values.Count; i++)
            Assert.True(values[i] > values[i - 1]);
    }

    [Fact]
    public void Enumeration_Empty_YieldsNothing()
    {
        Assert.Empty(CodePointSet.Empty);
    }

    [Fact]
    public void Enumeration_AllStartsAndEndsAtBoundaries()
    {
        var first = CodePointSet.All.First();
        var last  = CodePointSet.All.Last();
        Assert.Equal(0, first.Value);
        Assert.Equal(CodePoint.MaxValue, last.Value);
    }

    [Fact]
    public void Enumeration_All_IsLazy_NoEagerAllocation()
    {
        // Taking just the first element must succeed immediately without iterating all ~1.1M.
        var first = CodePointSet.All.Take(1).FirstOrDefault();
        Assert.Equal(0, first.Value);
    }

    [Fact]
    public void Enumeration_MultipleEnumerators_AreIndependent()
    {
        var s = Set((0x41, 0x45));
        using var e1 = s.GetEnumerator();
        using var e2 = s.GetEnumerator();
        e1.MoveNext();
        e1.MoveNext();
        e2.MoveNext();
        // e2 should still be at start
        Assert.Equal(0x41, e2.Current.Value);
        Assert.Equal(0x42, e1.Current.Value);
    }

    // ── Count ────────────────────────────────────────────────────────────────

    [Fact]
    public void Count_CorrectForKnownSets()
    {
        var s = Set((0x41, 0x5A)); // 26 letters
        Assert.Equal(26, s.Count);

        Assert.Empty(CodePointSet.Empty);
    }

    // ── De Morgan / distributive ─────────────────────────────────────────────

    [Fact]
    public void DeMorgan_UnionOfComplements()
    {
        // ¬(A ∪ B) = ¬A ∩ ¬B
        var a = Set((0, 50));
        var b = Set((30, 80));
        var lhs = a.Union(b).Complement();
        var rhs = a.Complement().Intersect(b.Complement());
        Assert.Equal(lhs, rhs);
    }

    [Fact]
    public void DeMorgan_IntersectionOfComplements()
    {
        // ¬(A ∩ B) = ¬A ∪ ¬B
        var a = Set((0, 50));
        var b = Set((30, 80));
        var lhs = a.Intersect(b).Complement();
        var rhs = a.Complement().Union(b.Complement());
        Assert.Equal(lhs, rhs);
    }

    // ── Real boundaries ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(0x000000)]
    [InlineData(0x00D7FF)]
    [InlineData(0x00D800)]
    [InlineData(0x00DFFF)]
    [InlineData(0x00E000)]
    [InlineData(0x10FFFF)]
    public void Contains_RealBoundaries(int value)
    {
        Assert.True(CodePointSet.All.Contains(CodePoint.Create(value)));
        Assert.False(CodePointSet.Empty.Contains(CodePoint.Create(value)));
    }
}
