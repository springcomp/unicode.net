using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

public class UnicodePropertiesTests
{
    private static readonly UnicodeVersion V = UnicodeVersion.Current;

    // ── Category resolution ────────────────────────────────────────────────

    [Fact]
    public void TryResolve_Category_ShortAlias_Lu()
    {
        var ok = UnicodeProperties.TryResolve("Lu", V, out var set);
        Assert.True(ok);
        Assert.True(set.Contains(CodePoint.Create(0x0041))); // 'A'
    }

    [Fact]
    public void TryResolve_Category_LongName_Uppercase_Letter()
    {
        var ok1 = UnicodeProperties.TryResolve("Lu", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("Uppercase_Letter", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void TryResolve_Category_CompoundSyntax_gc_Lu()
    {
        var ok1 = UnicodeProperties.TryResolve("Lu", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("gc=Lu", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void TryResolve_Category_MajorCategory_L_Union()
    {
        // 'L' resolves to the Letter union (Lu+Ll+Lt+Lm+Lo)
        var ok = UnicodeProperties.TryResolve("L", V, out var set);
        Assert.True(ok);
        Assert.True(set.Contains(CodePoint.Create(0x0041))); // 'A' (Lu)
        Assert.True(set.Contains(CodePoint.Create(0x0061))); // 'a' (Ll)
    }

    // ── Block resolution ────────────────────────────────────────────────────

    [Fact]
    public void TryResolve_Block_ExactName_BasicLatin()
    {
        var ok = UnicodeProperties.TryResolve("Basic Latin", V, out var set);
        Assert.True(ok);
        Assert.True(set.Contains(CodePoint.Create(0x0041)));
    }

    [Fact]
    public void TryResolve_Block_NoSpaces_BasicLatin()
    {
        var ok1 = UnicodeProperties.TryResolve("Basic Latin", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("BasicLatin", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void TryResolve_Block_IsPrefix_IsBasicLatin()
    {
        var ok1 = UnicodeProperties.TryResolve("Basic Latin", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("IsBasicLatin", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void TryResolve_Block_InPrefix_InBasicLatin()
    {
        var ok1 = UnicodeProperties.TryResolve("Basic Latin", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("InBasicLatin", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    // ── Script resolution ───────────────────────────────────────────────────

    [Fact]
    public void TryResolve_Script_ByName_Greek()
    {
        var ok = UnicodeProperties.TryResolve("Greek", V, out var set);
        Assert.True(ok);
        Assert.True(set.Contains(CodePoint.Create(0x0370)));
    }

    [Fact]
    public void TryResolve_Script_ISOAlias_Grek()
    {
        var ok1 = UnicodeProperties.TryResolve("Greek", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("Grek", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void TryResolve_Script_CompoundSyntax_sc_Grek()
    {
        var ok1 = UnicodeProperties.TryResolve("Greek", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("sc=Grek", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    [Fact]
    public void TryResolve_Script_CompoundSyntax_Script_Greek()
    {
        var ok1 = UnicodeProperties.TryResolve("Greek", V, out var set1);
        var ok2 = UnicodeProperties.TryResolve("Script=Greek", V, out var set2);
        Assert.True(ok1);
        Assert.True(ok2);
        Assert.Equal(set1, set2);
    }

    // ── Binary property resolution ──────────────────────────────────────────

    [Fact]
    public void TryResolve_BinaryProperty_Alphabetic()
    {
        var ok = UnicodeProperties.TryResolve("Alphabetic", V, out var set);
        Assert.True(ok);
        Assert.True(set.Contains(CodePoint.Create(0x0041))); // 'A'
    }

    [Fact]
    public void TryResolve_BinaryProperty_White_Space()
    {
        var ok = UnicodeProperties.TryResolve("White_Space", V, out var set);
        Assert.True(ok);
        Assert.True(set.Contains(CodePoint.Create(0x0020))); // SPACE
    }

    // ── Unknown returns false ────────────────────────────────────────────────

    [Fact]
    public void TryResolve_Unknown_ReturnsFalse()
    {
        var ok = UnicodeProperties.TryResolve("NotARealProperty", V, out var set);
        Assert.False(ok);
        Assert.Equal(CodePointSet.Empty, set);
    }

    [Fact]
    public void TryResolve_EmptyString_ReturnsFalse()
    {
        var ok = UnicodeProperties.TryResolve("", V, out _);
        Assert.False(ok);
    }

    // ── Suggestions ─────────────────────────────────────────────────────────

    [Fact]
    public void Suggest_LU_IncludesLu()
    {
        var suggestions = UnicodeProperties.Suggest("LU", V).ToList();
        Assert.Contains("Lu", suggestions);
    }

    [Fact]
    public void Suggest_BasicLaten_IncludesBasicLatin()
    {
        var suggestions = UnicodeProperties.Suggest("BasicLaten", V).ToList();
        Assert.Contains("BasicLatin", suggestions);
    }

    [Fact]
    public void Suggest_Greak_IncludesGreek()
    {
        var suggestions = UnicodeProperties.Suggest("Greak", V).ToList();
        Assert.Contains("Greek", suggestions);
    }

    [Fact]
    public void Suggest_ReturnsAtMostMaxSuggestions()
    {
        var suggestions = UnicodeProperties.Suggest("Lu", V, maxSuggestions: 2).ToList();
        Assert.True(suggestions.Count <= 2);
    }

    // ── Resolve() throws UnknownPropertyException ────────────────────────────

    [Fact]
    public void Resolve_UnknownProperty_ThrowsUnknownPropertyException()
    {
        var ex = Assert.Throws<UnknownPropertyException>(() =>
            UnicodeProperties.Resolve("UnknownProp", V));
        Assert.Equal("UnknownProp", ex.PropertyName);
        Assert.NotNull(ex.Suggestions);
    }

    [Fact]
    public void Resolve_UnknownProperty_ExceptionHasSuggestions()
    {
        var ex = Assert.Throws<UnknownPropertyException>(() =>
            UnicodeProperties.Resolve("Greak", V));
        Assert.NotEmpty(ex.Suggestions);
    }

    [Fact]
    public void Resolve_KnownProperty_ReturnsSet()
    {
        var set = UnicodeProperties.Resolve("Lu", V);
        Assert.True(set.Contains(CodePoint.Create(0x0041)));
    }
}
