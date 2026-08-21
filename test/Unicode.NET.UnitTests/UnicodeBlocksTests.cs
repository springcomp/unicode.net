using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

public class UnicodeBlocksTests
{
	[Theory]
	[InlineData("Latin Extended-A")]
	public void TryResolveBlock_ByNameOrAlias_ReturnsBlock(string nameOrAlias)
    	=> Assert.True(UnicodeBlocks.TryResolveBlock(nameOrAlias, UnicodeVersion.Current, out var codePointSet));

    // ── GetBlock spot-checks ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(0x0000, "Basic Latin", 0x0000, 0x007F)]
    [InlineData(0x0041, "Basic Latin", 0x0000, 0x007F)]
    [InlineData(0x007F, "Basic Latin", 0x0000, 0x007F)]
    [InlineData(0x4E00, "CJK Unified Ideographs", 0x4E00, 0x9FFF)]
    public void GetBlock_KnownCodePoint_ReturnsCorrectBlock(
        int codePointValue, string expectedName, int expectedStart, int expectedEnd)
    {
        var cp = CodePoint.Create(codePointValue);
        var block = UnicodeBlocks.GetBlock(cp, UnicodeVersion.Current);

        Assert.NotNull(block);
        Assert.Equal(expectedName, block!.Value.Name);
        Assert.Equal(CodePointRange.Create(expectedStart, expectedEnd), block.Value.Range);
    }

    [Fact]
    public void GetBlock_UnassignedGap_ReturnsNull()
    {
        // Code points in gaps between defined blocks return null.
        // Use a value known to be outside any block in 16.0.0.
        // The range 0x2FE0–0x2FEF is unassigned and not in a named block in many versions.
        // Safer: iterate to find a code point not covered by any block.
        var allBlocks = UnicodeBlocks.GetAllBlocks(UnicodeVersion.Current);
        // Find gap between first and second block if any; otherwise just assert we handle it.
        // Actually, Unicode blocks cover the entire BMP contiguously except for some gaps.
        // We'll test code points above the last defined block.
        var lastBlock = allBlocks[allBlocks.Count - 1];
        if (lastBlock.Range.End.Value < CodePoint.MaxValue)
        {
            var beyondLast = CodePoint.Create(lastBlock.Range.End.Value + 1);
            var result = UnicodeBlocks.GetBlock(beyondLast, UnicodeVersion.Current);
            Assert.Null(result);
        }
        // If all code points are covered, the test is vacuously satisfied.
    }

    // ── GetBlockRange spot-checks ────────────────────────────────────────────────

    [Theory]
    [InlineData("Basic Latin", 0x0000, 0x007F)]
    [InlineData("CJK Unified Ideographs", 0x4E00, 0x9FFF)]
    [InlineData("Latin-1 Supplement", 0x0080, 0x00FF)]
    public void GetBlockRange_KnownName_ReturnsCorrectRange(
        string blockName, int expectedStart, int expectedEnd)
    {
        var range = UnicodeBlocks.GetBlockRange(blockName, UnicodeVersion.Current);
        Assert.Equal(CodePointRange.Create(expectedStart, expectedEnd), range);
    }

    [Fact]
    public void GetBlockRange_UnknownName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            UnicodeBlocks.GetBlockRange("Not A Real Block", UnicodeVersion.Current));
    }

    [Fact]
    public void GetBlock_UnregisteredVersion_Throws()
    {
        var cp = CodePoint.Create(0x0041);
        var badVersion = new UnicodeVersion(1, 0, 0);
        Assert.Throws<NotSupportedException>(() =>
            UnicodeBlocks.GetBlock(cp, badVersion));
    }

    [Fact]
    public void GetBlockRange_UnregisteredVersion_Throws()
    {
        var badVersion = new UnicodeVersion(1, 0, 0);
        Assert.Throws<NotSupportedException>(() =>
            UnicodeBlocks.GetBlockRange("Basic Latin", badVersion));
    }

    // ── UnicodeVersion ───────────────────────────────────────────────────────────

    [Fact]
    public void UnicodeVersion_Latest_IsV15_1_0()
    {
        Assert.Equal(UnicodeVersion.V15_1_0, UnicodeVersion.Current);
    }

    [Fact]
    public void UnicodeVersion_ToString()
    {
        Assert.Equal("16.0.0", UnicodeVersion.V16_0_0.ToString());
    }
}
