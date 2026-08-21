using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

public class UnicodeScriptsTests
{
    // ── TryResolveScript tests ───────────────────────────────────────────────────

    [Fact]
    public void TryResolveScript_FullName_Success()
    {
        var result = UnicodeScripts.TryResolveScript("Latin", UnicodeVersion.Current, out var script);

        Assert.True(result);
        Assert.Equal(Script.Latin, script);
    }

    [Fact]
    public void TryResolveScript_Alias_Success()
    {
        // "Latn" is ISO 15924 alias for Latin
        var result = UnicodeScripts.TryResolveScript("Latn", UnicodeVersion.Current, out var script);

        Assert.True(result);
        Assert.Equal(Script.Latin, script);
    }

    [Fact]
    public void TryResolveScript_WithPrefix_Success()
    {
        // "sc=Grek" should resolve to Greek
        var result = UnicodeScripts.TryResolveScript("sc=Grek", UnicodeVersion.Current, out var script);

        Assert.True(result);
        Assert.Equal(Script.Greek, script);
    }

    [Fact]
    public void TryResolveScript_CaseInsensitive_Success()
    {
        var result1 = UnicodeScripts.TryResolveScript("latin", UnicodeVersion.Current, out var script1);
        Assert.True(result1, "lowercase failed");
        Assert.Equal(Script.Latin, script1);

        var result2 = UnicodeScripts.TryResolveScript("LATIN", UnicodeVersion.Current, out var script2);
        Assert.True(result2, "uppercase failed");
        Assert.Equal(Script.Latin, script2);

        var result3 = UnicodeScripts.TryResolveScript("LatIn", UnicodeVersion.Current, out var script3);
        Assert.True(result3, "mixed case failed");
        Assert.Equal(Script.Latin, script3);
    }

    [Fact]
    public void TryResolveScript_Unknown_ReturnsFalse()
    {
        var result = UnicodeScripts.TryResolveScript("NotAScript", UnicodeVersion.Current, out var script);

        Assert.False(result);
        Assert.Equal(Script.Unknown, script);
    }

    [Fact]
    public void TryResolveScript_EmptyString_ReturnsFalse()
    {
        var result = UnicodeScripts.TryResolveScript("", UnicodeVersion.Current, out var script);

        Assert.False(result);
        Assert.Equal(Script.Unknown, script);
    }

    // ── GetScriptSet tests ───────────────────────────────────────────────────────

    [Fact]
    public void GetScriptSet_Latin_ContainsASCII()
    {
        var set = UnicodeScripts.GetScriptSet(Script.Latin, UnicodeVersion.Current);

        // ASCII uppercase A-Z should be in Latin
        Assert.True(set.Contains(CodePoint.Create(0x0041))); // A
        Assert.True(set.Contains(CodePoint.Create(0x005A))); // Z
        Assert.True(set.Contains(CodePoint.Create(0x0061))); // a
        Assert.True(set.Contains(CodePoint.Create(0x007A))); // z
    }

    [Fact]
    public void GetScriptSet_Greek_ContainsGreekLetters()
    {
        var set = UnicodeScripts.GetScriptSet(Script.Greek, UnicodeVersion.Current);

        // U+0370..U+0373 per TASK-003 doc
        Assert.True(set.Contains(CodePoint.Create(0x0370)));
        Assert.True(set.Contains(CodePoint.Create(0x0373)));
    }

    [Fact]
    public void GetScriptSet_ByName_Latin_Success()
    {
        var set = UnicodeScripts.GetScriptSet("Latin", UnicodeVersion.Current);

        Assert.True(set.Contains(CodePoint.Create(0x0041)));
    }

    [Fact]
    public void GetScriptSet_ByAlias_Latin_Success()
    {
        var set = UnicodeScripts.GetScriptSet("Latn", UnicodeVersion.Current);

        Assert.True(set.Contains(CodePoint.Create(0x0041)));
    }

    [Fact]
    public void GetScriptSet_UnknownName_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            UnicodeScripts.GetScriptSet("NotAScript", UnicodeVersion.Current));

        Assert.Contains("NotAScript", ex.Message);
    }

    [Fact]
    public void GetScriptSet_Unknown_ReturnsEmpty()
    {
        var set = UnicodeScripts.GetScriptSet(Script.Unknown, UnicodeVersion.Current);

        Assert.True(set.IsEmpty);
    }

    // ── GetScriptExtensions tests ────────────────────────────────────────────────

    [Fact]
    public void GetScriptExtensions_ArabicComma_MultiScript()
    {
        // U+060C ARABIC COMMA has multiple scripts per TASK-003 doc
        var cp = CodePoint.Create(0x060C);
        var scripts = UnicodeScripts.GetScriptExtensions(cp, UnicodeVersion.Current);

        Assert.NotEmpty(scripts);
        // Should include at least Arab, Nkoo, Rohg, Syrc, Thaa, Yezi
        // Check for a few known ones
        Assert.Contains(Script.Arabic, scripts);
        Assert.Contains(Script.Syriac, scripts);
    }

    [Fact]
    public void GetScriptExtensions_LatinA_ReturnsEmpty()
    {
        // U+0041 'A' is primarily Latin, no extensions in ScriptExtensions.txt
        var cp = CodePoint.Create(0x0041);
        var scripts = UnicodeScripts.GetScriptExtensions(cp, UnicodeVersion.Current);

        // If no script extensions entry exists, should return empty array
        Assert.Empty(scripts);
    }

    [Fact]
    public void GetScriptExtensions_UnassignedCodePoint_ReturnsEmpty()
    {
        // U+FFF0 is unassigned
        var cp = CodePoint.Create(0xFFF0);
        var scripts = UnicodeScripts.GetScriptExtensions(cp, UnicodeVersion.Current);

        Assert.Empty(scripts);
    }
}
