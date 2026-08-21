using System.Linq;
using Unicode.NET.CodeGen.Parsing;
using Xunit;

namespace Unicode.NET.CodeGen.UnitTests.Parsing;

public sealed class CaseFoldingParserTests
{
  [Fact]
  public void Parse_PreservesAllStatusVariants()
  {
    string fixture = "0130; T; 0069 0307; # LATIN CAPITAL LETTER I WITH DOT ABOVE" + Environment.NewLine +
                     "0130; F; 0069 0307; # LATIN CAPITAL LETTER I WITH DOT ABOVE" + Environment.NewLine +
                     "0130; S; 0069; # LATIN SMALL LETTER I" + Environment.NewLine +
                     "0130; C; 0069; # Common simple";
    var path = WriteTempFixture(fixture);

    var records = CaseFoldingParser.Parse(path);

    Assert.Equal(4, records.Count);
    Assert.Contains(records, r => r.Status == 'T' && r.Mapping.SequenceEqual(new[] { 0x0069, 0x0307 }));
    Assert.Contains(records, r => r.Status == 'F' && r.Mapping.SequenceEqual(new[] { 0x0069, 0x0307 }));
    Assert.Contains(records, r => r.Status == 'S' && r.Mapping.SequenceEqual(new[] { 0x0069 }));
    Assert.Contains(records, r => r.Status == 'C' && r.Mapping.SequenceEqual(new[] { 0x0069 }));
  }

  [Fact]
  public void Parse_SkipsMalformedLines()
  {
    string fixture = "0130; C; 0069" + Environment.NewLine +
                     "badline" + Environment.NewLine +
                     "0049; C; 0069";
    var path = WriteTempFixture(fixture);

    var records = CaseFoldingParser.Parse(path);

    Assert.Equal(2, records.Count);
    Assert.Collection(records,
        r => Assert.Equal(0x0130, r.CodePoint),
        r => Assert.Equal(0x0049, r.CodePoint));
  }

  private static string WriteTempFixture(string content)
  {
    string path = Path.GetTempFileName();
    File.WriteAllText(path, content);
    return path;
  }
}
