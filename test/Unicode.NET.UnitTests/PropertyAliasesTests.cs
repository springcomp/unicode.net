using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

public class PropertyAliasesTests
{
    private static readonly UnicodeVersion V = UnicodeVersion.V15_1_0;

    // ── TryResolveCategory ───────────────────────────────────────────────────────

    [Fact]
    public void TryResolveCategory_ShortAlias_Lu_Succeeds()
    {
        bool ok = UnicodeData.TryResolveCategory("Lu", V, out var cat);
        Assert.True(ok);
        Assert.Equal(GeneralCategory.Lu, cat);
    }

    [Fact]
    public void TryResolveCategory_LongName_Succeeds()
    {
        bool ok = UnicodeData.TryResolveCategory("Uppercase_Letter", V, out var cat);
        Assert.True(ok);
        Assert.Equal(GeneralCategory.Lu, cat);
    }

    [Fact]
    public void TryResolveCategory_LongName_SameAs_ShortAlias()
    {
        UnicodeData.TryResolveCategory("Lu", V, out var fromShort);
        UnicodeData.TryResolveCategory("Uppercase_Letter", V, out var fromLong);
        Assert.Equal(fromShort, fromLong);
    }

    [Fact]
    public void TryResolveCategory_CompoundSyntax_gcEqLu_Succeeds()
    {
        bool ok = UnicodeData.TryResolveCategory("gc=Lu", V, out var cat);
        Assert.True(ok);
        Assert.Equal(GeneralCategory.Lu, cat);
    }

    [Fact]
    public void TryResolveCategory_CompoundSyntax_GeneralCategoryEqUppercaseLetter_Succeeds()
    {
        bool ok = UnicodeData.TryResolveCategory("General_Category=Uppercase_Letter", V, out var cat);
        Assert.True(ok);
        Assert.Equal(GeneralCategory.Lu, cat);
    }

    [Fact]
    public void TryResolveCategory_CaseInsensitive()
    {
        bool ok = UnicodeData.TryResolveCategory("uppercase_letter", V, out var cat);
        Assert.True(ok);
        Assert.Equal(GeneralCategory.Lu, cat);
    }

    [Fact]
    public void TryResolveCategory_Unknown_ReturnsFalse()
    {
        bool ok = UnicodeData.TryResolveCategory("Unknown", V, out _);
        Assert.False(ok);
    }

    [Fact]
    public void TryResolveCategory_MajorCategory_L_ReturnsFalse()
    {
        // 'L' expands to a union — TryResolveCategory returns false; use GetCategorySet instead.
        bool ok = UnicodeData.TryResolveCategory("L", V, out _);
        Assert.False(ok);
    }

    // ── GetCategorySet(string) ────────────────────────────────────────────────────

    [Fact]
    public void GetCategorySet_ByShortAlias_ReturnsNonEmpty()
    {
        var set = UnicodeData.GetCategorySet("Lu", V);
        Assert.False(set.IsEmpty);
    }

    [Fact]
    public void GetCategorySet_ByAlias_MatchesEnumOverload()
    {
        var byAlias = UnicodeData.GetCategorySet("Lu", V);
        var byEnum  = UnicodeData.GetCategorySet(GeneralCategory.Lu, V);
        Assert.Equal(byEnum, byAlias);
    }

    [Fact]
    public void GetCategorySet_MajorCategory_L_ReturnsUnionOfLetterSubcategories()
    {
        var letterSet = UnicodeData.GetCategorySet("L", V);

        var expected = CodePointSet.Empty;
        foreach (var cat in new[] { GeneralCategory.Lu, GeneralCategory.Ll, GeneralCategory.Lt, GeneralCategory.Lm, GeneralCategory.Lo })
            expected = expected.Union(UnicodeData.GetCategorySet(cat, V));

        Assert.Equal(expected, letterSet);
    }

    [Fact]
    public void GetCategorySet_MajorCategory_Letter_SameAs_L()
    {
        var byShort = UnicodeData.GetCategorySet("L", V);
        var byLong  = UnicodeData.GetCategorySet("Letter", V);
        Assert.Equal(byShort, byLong);
    }

    [Fact]
    public void GetCategorySet_Unknown_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            UnicodeData.GetCategorySet("DefinitelyNotACategory", V));
    }

    // ── Spot checks for other categories ─────────────────────────────────────────

    [Theory]
    [InlineData("Ll")]
    [InlineData("Lowercase_Letter")]
    [InlineData("Nd")]
    [InlineData("Decimal_Number")]
    [InlineData("digit")]      // additional alias from PropertyValueAliases.txt
    [InlineData("Zs")]
    [InlineData("Space_Separator")]
    public void TryResolveCategory_VariousAliases_Succeed(string alias)
    {
        bool ok = UnicodeData.TryResolveCategory(alias, V, out _);
        Assert.True(ok, $"Expected alias '{alias}' to resolve successfully.");
    }

    [Theory]
    [InlineData("M")]   // Mark union
    [InlineData("N")]   // Number union
    [InlineData("P")]   // Punctuation union
    [InlineData("S")]   // Symbol union
    [InlineData("Z")]   // Separator union
    [InlineData("C")]   // Other union
    [InlineData("LC")]  // Cased_Letter union
    public void GetCategorySet_AllMajorCategories_NonEmpty(string alias)
    {
        var set = UnicodeData.GetCategorySet(alias, V);
        Assert.False(set.IsEmpty, $"Expected major category '{alias}' to return a non-empty set.");
    }
}
