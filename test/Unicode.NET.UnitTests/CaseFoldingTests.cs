using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

/// <summary>
/// Tests for <see cref="CaseFolding"/>.
/// </summary>
public class CaseFoldingTests
{
    // ── Simple folding — Latin ──────────────────────────────────────────────

    [Theory]
    [InlineData(0x0041, 0x0061)] // A -> a
    [InlineData(0x0042, 0x0062)] // B -> b
    [InlineData(0x005A, 0x007A)] // Z -> z
    public void SimpleFold_Latin_Uppercase_MapsToLowercase(int input, int expected)
    {
        var result = CaseFolding.Fold(CodePoint.Create(input), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(expected, result[0].Value);
    }

    [Fact]
    public void SimpleFold_LowercaseAscii_IsIdentity()
    {
        // lowercase a..z fold to themselves (not in SimpleMap)
        for (int c = 0x61; c <= 0x7A; c++)
        {
            var result = CaseFolding.Fold(CodePoint.Create(c), CaseFoldingMode.Simple);
            Assert.Single(result);
            Assert.Equal(c, result[0].Value);
        }
    }

    // ── Simple folding — Greek ──────────────────────────────────────────────

    [Theory]
    [InlineData(0x03A3, 0x03C3)] // Σ -> σ
    [InlineData(0x03C2, 0x03C3)] // ς -> σ (final sigma folds same as sigma)
    [InlineData(0x0391, 0x03B1)] // Α -> α
    public void SimpleFold_Greek_UppercaseMapsToLowercase(int input, int expected)
    {
        var result = CaseFolding.Fold(CodePoint.Create(input), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(expected, result[0].Value);
    }

    // ── Simple folding — Cyrillic ───────────────────────────────────────────

    [Theory]
    [InlineData(0x0410, 0x0430)] // А -> а
    [InlineData(0x0411, 0x0431)] // Б -> б
    [InlineData(0x042F, 0x044F)] // Я -> я
    public void SimpleFold_Cyrillic_UppercaseMapsToLowercase(int input, int expected)
    {
        var result = CaseFolding.Fold(CodePoint.Create(input), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(expected, result[0].Value);
    }

    // ── Simple folding — Special cases ─────────────────────────────────────

    [Fact]
    public void SimpleFold_GermanSharpS_IsIdentityUnderSimple()
    {
        // U+00DF ß has no C or S record — simple folds to itself.
        // Full folding would expand to 0073 0073 (ss) — but that requires Full mode.
        var result = CaseFolding.Fold(CodePoint.Create(0x00DF), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(0x00DF, result[0].Value);
    }

    [Fact]
    public void SimpleFold_CapitalSharpS_MapsToSharpS()
    {
        // U+1E9E ẞ has S record → 00DF (ß)
        var result = CaseFolding.Fold(CodePoint.Create(0x1E9E), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(0x00DF, result[0].Value);
    }

    [Fact]
    public void SimpleFold_KelvinSign_MapsToLowercaseK()
    {
        // U+212A K (Kelvin) has C record → 006B (k)
        var result = CaseFolding.Fold(CodePoint.Create(0x212A), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(0x006B, result[0].Value);
    }

    [Fact]
    public void SimpleFold_OhmSign_MapsToLowercaseOmega()
    {
        // U+2126 Ω (Ohm) has C record → 03C9 (ω)
        var result = CaseFolding.Fold(CodePoint.Create(0x2126), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(0x03C9, result[0].Value);
    }

    [Fact]
    public void SimpleFold_Cherokee_MapsToLowerCherokee()
    {
        // U+AB70 is a Cherokee small letter that folds to U+13A0 (not lowercase in Latin sense)
        var result = CaseFolding.Fold(CodePoint.Create(0xAB70), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(0x13A0, result[0].Value);
    }

    [Fact]
    public void SimpleFold_Supplementary_DesertCapital_MapsToSmall()
    {
        // U+10400 (Deseret Capital Letter Long I) → U+10428
        var result = CaseFolding.Fold(CodePoint.Create(0x10400), CaseFoldingMode.Simple);
        Assert.Single(result);
        Assert.Equal(0x10428, result[0].Value);
    }

    // ── Simple folding — Idempotence ────────────────────────────────────────

    [Theory]
    [InlineData(0x0041)] // A
    [InlineData(0x03A3)] // Σ
    [InlineData(0x0410)] // А (Cyrillic)
    [InlineData(0x212A)] // K (Kelvin)
    public void SimpleFold_IsIdempotent(int input)
    {
        var first = CaseFolding.Fold(CodePoint.Create(input), CaseFoldingMode.Simple);
        var second = CaseFolding.Fold(first[0], CaseFoldingMode.Simple);
        Assert.Equal(first[0].Value, second[0].Value);
    }

    // ── Version validation ──────────────────────────────────────────────────

    [Fact]
    public void Fold_UnknownVersion_Throws()
    {
        var unknownVersion = new UnicodeVersion(1, 0, 0);
        Assert.Throws<NotSupportedException>(() =>
            CaseFolding.Fold(CodePoint.Create(0x41), version: unknownVersion));
    }

    [Fact]
    public void Fold_RegisteredVersions_ReturnUsableSimpleAndFullData()
    {
        foreach (var version in new[] { UnicodeVersion.V15_1_0, UnicodeVersion.V16_0_0 })
        {
            var simple = CaseFolding.Fold(
                CodePoint.Create(0x0041),
                CaseFoldingMode.Simple,
                version: version);
            var full = CaseFolding.Fold(
                CodePoint.Create(0x0041),
                CaseFoldingMode.Full,
                version: version);

            Assert.NotEmpty(simple);
            Assert.NotEmpty(full);
            Assert.Equal(0x0061, simple[0].Value);
            Assert.Equal(0x0061, full[0].Value);
        }
    }

    // ── Full mode ───────────────────────────────────────────────────────────

    [Fact]
    public void FullFold_CapitalSharpS_ExpandsToSS()
    {
        // U+1E9E ẞ full-folds to 0073 0073 (ss)
        var result = CaseFolding.Fold(CodePoint.Create(0x1E9E), CaseFoldingMode.Full);
        Assert.Equal(2, result.Count);
        Assert.Equal(0x0073, result[0].Value); // s
        Assert.Equal(0x0073, result[1].Value); // s
    }

    [Fact]
    public void FullFold_SmallSharpS_ExpandsToSS()
    {
        // U+00DF ß full-folds to 0073 0073 (ss)
        var result = CaseFolding.Fold(CodePoint.Create(0x00DF), CaseFoldingMode.Full);
        Assert.Equal(2, result.Count);
        Assert.Equal(0x0073, result[0].Value);
        Assert.Equal(0x0073, result[1].Value);
    }

    [Fact]
    public void FullFold_LigatureFI_ExpandsToFI()
    {
        // U+FB01 ﬁ full-folds to 0066 0069 (fi)
        var result = CaseFolding.Fold(CodePoint.Create(0xFB01), CaseFoldingMode.Full);
        Assert.Equal(2, result.Count);
        Assert.Equal(0x0066, result[0].Value); // f
        Assert.Equal(0x0069, result[1].Value); // i
    }

    [Fact]
    public void FullFold_LigatureFFI_ExpandsToFFI()
    {
        // U+FB03 ﬃ full-folds to 0066 0066 0069 (ffi)
        var result = CaseFolding.Fold(CodePoint.Create(0xFB03), CaseFoldingMode.Full);
        Assert.Equal(3, result.Count);
        Assert.Equal(0x0066, result[0].Value); // f
        Assert.Equal(0x0066, result[1].Value); // f
        Assert.Equal(0x0069, result[2].Value); // i
    }

    [Fact]
    public void FullFold_LatinUppercase_FallsBackToSimple()
    {
        // A has no F record — full fold falls back to simple fold
        var result = CaseFolding.Fold(CodePoint.Create(0x0041), CaseFoldingMode.Full);
        Assert.Single(result);
        Assert.Equal(0x0061, result[0].Value); // a
    }

    [Fact]
    public void FullFold_LowercaseAscii_IsIdentity()
    {
        // lowercase a has no fold record at all — returns itself
        var result = CaseFolding.Fold(CodePoint.Create(0x0061), CaseFoldingMode.Full);
        Assert.Single(result);
        Assert.Equal(0x0061, result[0].Value);
    }

    [Fact]
    public void StringFold_ExpandsAndComparesCaselessText()
    {
        Assert.Equal("ss", CaseFolding.Fold("ß"));
        Assert.Equal("fi", CaseFolding.Fold("ﬁ"));
        Assert.True(CaseFolding.CaselessEquals("MASSE", "Maße"));
        Assert.Equal("σσ", CaseFolding.Fold("Σς"));
    }

    [Fact]
    public void StringFold_PreservesCherokeeAndSupplementaryScalars()
    {
        Assert.Equal("Ꭰ", CaseFolding.Fold("ꭰ"));
        Assert.Equal("𐐨", CaseFolding.Fold("𐐀"));
    }

    [Fact]
    public void StringFold_SimpleDoesNotExpand()
    {
        Assert.Equal("ß", CaseFolding.Fold("ß", CaseFoldingMode.Simple));
    }

    [Fact]
    public void StringFold_IsNotNormalization()
    {
        const string decomposed = "A\u030A";

        Assert.Equal("a\u030A", CaseFolding.Fold(decomposed));
        Assert.NotEqual("å", CaseFolding.Fold(decomposed));
    }

    [Theory]
    [InlineData(0xD800)]
    [InlineData(0xDBFF)]
    [InlineData(0xDC00)]
    [InlineData(0xDFFF)]
    public void StringFold_RejectsEveryUnpairedSurrogate(int value)
    {
        Assert.Throws<ArgumentException>(() => CaseFolding.Fold(new string((char)value, 1)));
    }

    [Fact]
    public void StringFold_EmptyInput_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, CaseFolding.Fold(string.Empty));
    }

    [Fact]
    public void StringFold_RejectsNullAndMalformedUtf16()
    {
        Assert.Throws<ArgumentNullException>(() => CaseFolding.Fold(null!));
        Assert.Throws<ArgumentException>(() => CaseFolding.Fold("\uD800"));
        Assert.Throws<ArgumentException>(() => CaseFolding.CaselessEquals("\uDC00", ""));
    }

    [Fact]
    public void Fold_TurkicLocale_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            CaseFolding.Fold(
                CodePoint.Create(0x0049),
                CaseFoldingMode.Simple,
                CaseFoldingLocale.Turkic));
    }
}
