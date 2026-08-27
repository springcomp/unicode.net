using System.Globalization;
using Unicode.NET;
using Xunit;

namespace Unicode.NET.UnitTests;

public class CaseMappingTests
{
    [Theory]
    [InlineData("ABCxyz09!", "abcxyz09!", "ABCXYZ09!")]
    [InlineData("", "", "")]
    public void MapsAsciiAndIdentityScalars(string input, string expectedLower, string expectedUpper)
    {
        AssertScalars(expectedLower, CaseMapping.ToLower(input));
        AssertScalars(expectedUpper, CaseMapping.ToUpper(input));
    }

    [Theory]
    [InlineData(0x0391, 0x03B1, 0x0391)] // Greek alpha
    [InlineData(0x0416, 0x0436, 0x0416)] // Cyrillic zhe
    [InlineData(0x13A0, 0xAB70, 0x13A0)] // Cherokee
    [InlineData(0x10400, 0x10428, 0x10400)] // Deseret supplementary scalar
    public void MapsRepresentativeUnicodeScripts(int input, int expectedLower, int expectedUpper)
    {
        AssertScalars([expectedLower], CaseMapping.ToLower(Scalar(input)));
        AssertScalars([expectedUpper], CaseMapping.ToUpper(Scalar(input)));
    }

    [Fact]
    public void AppliesFullExpansionsAndDottedI()
    {
        AssertScalars([0x0053, 0x0053], CaseMapping.ToUpper("\u00DF"));
        AssertScalars([0x0046, 0x0049], CaseMapping.ToUpper("\uFB01"));
        AssertScalars([0x0069, 0x0307], CaseMapping.ToLower("\u0130"));
    }

    [Theory]
    [InlineData("A\u03A3", "a\u03C2")]
    [InlineData("\u03A3A", "\u03C3a")]
    [InlineData("A\u03A3A", "a\u03C3a")]
    [InlineData("A\u03A7\u03A3", "a\u03C7\u03C2")]
    public void AppliesBothFinalSigmaBranches(string input, string expected)
        => AssertScalars(expected, CaseMapping.ToLower(input));

    [Theory]
    [InlineData("A\u03A3\u0027!", "a\u03C2\u0027!")]
    [InlineData("A\u03A3\u0301!", "a\u03C2\u0301!")]
    [InlineData("A\u03A3\u0027B", "a\u03C3\u0027b")]
    [InlineData("A\u03A3\u0301B", "a\u03C3\u0301b")]
    public void SkipsCaseIgnorableScalarsWhenEvaluatingSigmaContext(string input, string expected)
        => AssertScalars(expected, CaseMapping.ToLower(input));

    [Fact]
    public void UsesDefaultMappingNotCurrentCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            AssertScalars([0x0069], CaseMapping.ToLower("I"));
            AssertScalars([0x0049], CaseMapping.ToUpper("\u0131"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void RejectsNullUnknownVersionAndBothLoneSurrogates()
    {
        Assert.Throws<ArgumentNullException>(() => CaseMapping.ToLower(null));
        Assert.Throws<NotSupportedException>(() =>
            CaseMapping.ToUpper("A", new UnicodeVersion(1, 0, 0)));
        Assert.Throws<ArgumentException>(() => CaseMapping.ToLower("\uD800"));
        Assert.Throws<ArgumentException>(() => CaseMapping.ToUpper("A\uDC00"));
    }

    [Fact]
    public void OmittedVersionIsExactlyUnicode151()
    {
        const string input = "A\u00DF\u0130\u03A3\uD801\uDC00";
        AssertScalars(CaseMapping.ToLower(input, UnicodeVersion.V15_1_0),
            CaseMapping.ToLower(input));
        AssertScalars(CaseMapping.ToUpper(input, UnicodeVersion.V15_1_0),
            CaseMapping.ToUpper(input));
    }

    [Fact]
    public void DispatchesBothVersionedTablesForStableMappings()
    {
        const string input = "A\u00DF\u0130\u03A3\u0416\u13A0\uD801\uDC00";
        AssertScalars([0x0061, 0x00DF, 0x0069, 0x0307, 0x03C3, 0x0436, 0xAB70, 0x10428],
            CaseMapping.ToLower(input, UnicodeVersion.V15_1_0));
        AssertScalars([0x0041, 0x0053, 0x0053, 0x0130, 0x03A3, 0x0416, 0x13A0, 0x10400],
            CaseMapping.ToUpper(input, UnicodeVersion.V16_0_0));
        Assert.Contains(UnicodeVersion.V15_1_0, UnicodeVersionInfo.SupportedVersions);
        Assert.Contains(UnicodeVersion.V16_0_0, UnicodeVersionInfo.SupportedVersions);
    }

    [Fact]
    public void DocumentsNonRoundTrippingDefaultMapping()
    {
        string lowered = CaseMapping.ToLower("\u0130");
        AssertScalars([0x0069, 0x0307], lowered);
        Assert.NotEqual("\u0130", CaseMapping.ToUpper(lowered));
        AssertScalars([0x0049, 0x0307], CaseMapping.ToUpper(lowered));
    }

    private static string Scalar(int value) => Utf16.Encode(CodePoint.CreateScalar(value));

    private static void AssertScalars(string expected, string actual)
        => Assert.Equal(Scalars(expected), Scalars(actual));

    private static void AssertScalars(IReadOnlyList<int> expected, string actual)
        => Assert.Equal(expected, Scalars(actual));

    private static List<int> Scalars(string value)
    {
        var result = new List<int>();
        foreach (CodePoint codePoint in value.EnumerateCodePoints())
            result.Add(codePoint.Value);
        return result;
    }
}
