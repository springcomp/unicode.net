using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

/// <summary>
/// Tests for <see cref="CaseClosure"/>.
/// </summary>
public class CaseClosureTests
{
    private static CodePointSet SetOf(params int[] values)
    {
        var builder = new CodePointSetBuilder();
        foreach (var v in values)
            builder.Add(CodePoint.Create(v));
        return builder.Build();
    }

    // ── Basic closure ───────────────────────────────────────────────────────

    [Fact]
    public void Closure_OfUppercaseA_IncludesLowercaseA()
    {
        // {'A'} should close to {'A', 'a'}
        var input = SetOf(0x0041); // A
        var result = CaseClosure.Closure(input);

        Assert.True(result.Contains(CodePoint.Create(0x0041))); // A
        Assert.True(result.Contains(CodePoint.Create(0x0061))); // a
    }

    [Fact]
    public void Closure_OfLowercaseK_IncludesKelvin()
    {
        // {'k'} (U+006B) closes to {U+004B 'K', U+006B 'k', U+212A '℃'}
        var input = SetOf(0x006B); // k
        var result = CaseClosure.Closure(input);

        Assert.True(result.Contains(CodePoint.Create(0x004B))); // K
        Assert.True(result.Contains(CodePoint.Create(0x006B))); // k
        Assert.True(result.Contains(CodePoint.Create(0x212A))); // Kelvin sign
    }

    [Fact]
    public void Closure_OfSigma_IncludesFinalSigmaAndCapital()
    {
        // Closure({σ}) = {Σ, σ, ς}
        var input = SetOf(0x03C3); // σ
        var result = CaseClosure.Closure(input);

        Assert.True(result.Contains(CodePoint.Create(0x03A3))); // Σ
        Assert.True(result.Contains(CodePoint.Create(0x03C3))); // σ
        Assert.True(result.Contains(CodePoint.Create(0x03C2))); // ς
    }

    [Fact]
    public void Closure_OfLowercaseS_IncludesLongS()
    {
        // Closure({s}) includes S (U+0053), s (U+0073), ſ (U+017F long s)
        var input = SetOf(0x0073); // s
        var result = CaseClosure.Closure(input);

        Assert.True(result.Contains(CodePoint.Create(0x0053))); // S
        Assert.True(result.Contains(CodePoint.Create(0x0073))); // s
        Assert.True(result.Contains(CodePoint.Create(0x017F))); // ſ long s
    }

    // ── Superset guarantee ──────────────────────────────────────────────────

    [Fact]
    public void Closure_IsSuperset_OfInput()
    {
        var input = SetOf(0x0041, 0x03A3, 0x0410); // A, Σ, А (Cyrillic)
        var result = CaseClosure.Closure(input);

        foreach (var cp in input)
            Assert.True(result.Contains(cp), $"Result missing input member {cp}");
    }

    // ── Idempotence ─────────────────────────────────────────────────────────

    [Fact]
    public void Closure_IsIdempotent()
    {
        var input = SetOf(0x0041, 0x03C3, 0x006B); // A, σ, k
        var once = CaseClosure.Closure(input);
        var twice = CaseClosure.Closure(once);

        Assert.Equal(once, twice);
    }

    [Fact]
    public void Closure_Idempotent_LargerSet()
    {
        // Build a set with various scripts
        var input = SetOf(0x0041, 0x0042, 0x03A3, 0x0410, 0x212A);
        var once = CaseClosure.Closure(input);
        var twice = CaseClosure.Closure(once);

        Assert.Equal(once, twice);
    }

    // ── Empty set ───────────────────────────────────────────────────────────

    [Fact]
    public void Closure_OfEmptySet_IsEmpty()
    {
        var result = CaseClosure.Closure(CodePointSet.Empty);
        Assert.True(result.IsEmpty);
    }

    // ── Termination ─────────────────────────────────────────────────────────

    [Fact]
    public void Closure_Terminates_OnFullAlphabetSet()
    {
        // Close all Latin uppercase A-Z — should terminate quickly and include lowercase
        var builder = new CodePointSetBuilder();
        for (int c = 0x41; c <= 0x5A; c++)
            builder.Add(CodePoint.Create(c));
        var input = builder.Build();

        var result = CaseClosure.Closure(input); // must terminate

        // Should include lowercase a-z
        for (int c = 0x61; c <= 0x7A; c++)
            Assert.True(result.Contains(CodePoint.Create(c)), $"Missing U+{c:X4}");
    }

    // ── Mode validation ─────────────────────────────────────────────────────

    [Fact]
    public void Closure_FullMode_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            CaseClosure.Closure(SetOf(0x0041), CaseFoldingMode.Full));
    }

    [Fact]
    public void Closure_TurkicLocale_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            CaseClosure.Closure(SetOf(0x0049), locale: CaseFoldingLocale.Turkic));
    }
}
