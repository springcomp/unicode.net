using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

public class GeneralCategoryTests
{
    // ── Spot-checks ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0x0041, GeneralCategory.Lu)]   // 'A'
    [InlineData(0x0061, GeneralCategory.Ll)]   // 'a'
    [InlineData(0x0030, GeneralCategory.Nd)]   // '0'
    [InlineData(0x0020, GeneralCategory.Zs)]   // space
    [InlineData(0xE000, GeneralCategory.Co)]   // private-use
    [InlineData(0x0378, GeneralCategory.Cn)]   // unassigned (U+0378 is unassigned in 16.0.0)
    public void GetGeneralCategory_KnownCodePoints(int codePointValue, GeneralCategory expected)
    {
        var cp = CodePoint.Create(codePointValue);
        var actual = UnicodeData.GetGeneralCategory(cp, UnicodeVersion.Current);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetGeneralCategory_UnregisteredVersion_Throws()
    {
        var cp = CodePoint.Create(0x0041);
        var badVersion = new UnicodeVersion(1, 0, 0);
        Assert.Throws<NotSupportedException>(() =>
            UnicodeData.GetGeneralCategory(cp, badVersion));
    }

    [Fact]
    public void GetCategorySet_UnregisteredVersion_Throws()
    {
        var badVersion = new UnicodeVersion(1, 0, 0);
        Assert.Throws<NotSupportedException>(() =>
            UnicodeData.GetCategorySet(GeneralCategory.Lu, badVersion));
    }

    // ── Coverage: union of all categories == All, no overlaps ────────────────────

    [Fact]
    public void AllCategories_UnionEqualsAll_NoOverlaps()
    {
        var categories = Enum.GetValues<GeneralCategory>();
        var sets = categories
            .Select(c => UnicodeData.GetCategorySet(c, UnicodeVersion.Current))
            .ToArray();

        // Check no overlaps between any two distinct categories.
        for (int i = 0; i < sets.Length; i++)
        {
            for (int j = i + 1; j < sets.Length; j++)
            {
                var intersection = sets[i].Intersect(sets[j]);
                if (!intersection.IsEmpty)
                {
                    var first = intersection.First();
                    Assert.Fail(
                        $"Categories {categories[i]} and {categories[j]} overlap " +
                        $"(first shared: U+{first.Value:X4})");
                }
            }
        }

        // Build union and verify == CodePointSet.All.
        var union = CodePointSet.Empty;
        foreach (var s in sets)
            union = union.Union(s);

        Assert.Equal(CodePointSet.All, union);
    }
}
