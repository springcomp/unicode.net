using Unicode.NET.CodeGen.Models;
using Unicode.NET.CodeGen.Parsing;
using Xunit;

namespace Unicode.NET.CodeGen.UnitTests.Parsing;

public sealed class UnicodeDataParserTests
{
  [Fact]
  public void Parse_SingleCodePointLine()
  {
    string fixture = "0041;LATIN CAPITAL LETTER A;Lu;;;;;N;;;;;";
    var path = WriteTempFixture(fixture);

    var records = UnicodeDataParser.Parse(path);

    Assert.Single(records);
    var record = records[0];
    Assert.Equal(0x0041, record.StartCodePoint);
    Assert.Equal(0x0041, record.EndCodePoint);
    Assert.Equal("LATIN CAPITAL LETTER A", record.Name);
    Assert.Equal("Lu", record.GeneralCategory);
  }

  [Fact]
  public void Parse_FirstLastPair_MergesIntoRange()
  {
    string fixture = "AC00;<Hangul Syllable, First>;Lo" + Environment.NewLine +
                     "D7A3;<Hangul Syllable, Last>;Lo";
    var path = WriteTempFixture(fixture);

    var records = UnicodeDataParser.Parse(path);

    Assert.Single(records);
    var record = records[0];
    Assert.True(record.IsRange);
    Assert.Equal(0xAC00, record.StartCodePoint);
    Assert.Equal(0xD7A3, record.EndCodePoint);
    Assert.Equal("Lo", record.GeneralCategory);
  }

  [Fact]
  public void Parse_UnassignedLine_Preserved()
  {
    string fixture = "0378;<reserved>;Cn";
    var path = WriteTempFixture(fixture);

    var records = UnicodeDataParser.Parse(path);

    Assert.Single(records);
    Assert.Equal("<reserved>", records[0].Name);
    Assert.Equal("Cn", records[0].GeneralCategory);
  }

  [Fact]
  public void Parse_MalformedLine_Skipped()
  {
    string fixture = "0041;LATIN CAPITAL LETTER A;Lu" + Environment.NewLine +
                     "badline" + Environment.NewLine +
                     "0042;LATIN CAPITAL LETTER B;Lu";
    var path = WriteTempFixture(fixture);

    var records = UnicodeDataParser.Parse(path);

    Assert.Equal(2, records.Count);
    Assert.Collection(records,
        r => Assert.Equal(0x0041, r.StartCodePoint),
        r => Assert.Equal(0x0042, r.StartCodePoint));
  }

  [Fact]
  public void Parse_OrphanedLast_Throws()
  {
    string fixture = "D7A3;<Hangul Syllable, Last>;Lo";
    var path = WriteTempFixture(fixture);

    var ex = Assert.Throws<InvalidDataException>(() => UnicodeDataParser.Parse(path));
    Assert.Contains("Unexpected Last>", ex.Message);
  }

  [Fact]
  public void Parse_OrphanedFirst_Throws()
  {
    string fixture = "AC00;<Hangul Syllable, First>;Lo";
    var path = WriteTempFixture(fixture);

    var ex = Assert.Throws<InvalidDataException>(() => UnicodeDataParser.Parse(path));
    Assert.Contains("Unclosed First>", ex.Message);
  }

  private static string WriteTempFixture(string content)
  {
    string path = Path.GetTempFileName();
    File.WriteAllText(path, content);
    return path;
  }
}
