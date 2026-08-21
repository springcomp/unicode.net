using Unicode.NET.CodeGen.Parsing;
using Unicode.NET;
using Xunit;

namespace Unicode.NET.CodeGen.UnitTests.Parsing;

public sealed class BlocksParserTests
{
  [Fact]
  public void Parse_IgnoresCommentsAndBlankLines()
  {
    string fixture = "# Comment line" + Environment.NewLine +
                     "0000..007F; Basic Latin" + Environment.NewLine +
                     "" + Environment.NewLine +
                     "0080..00FF; Latin-1 Supplement # trailing comment" + Environment.NewLine +
                     "0100..017F; Latin Extended-A";
    var path = WriteTempFixture(fixture);

    var records = BlocksParser.Parse(path);

    Assert.Equal(3, records.Count);
    Assert.Collection(records,
        r => AssertRecord(r, 0x0000, 0x007F, "Basic Latin"),
        r => AssertRecord(r, 0x0080, 0x00FF, "Latin-1 Supplement"),
        r => AssertRecord(r, 0x0100, 0x017F, "Latin Extended-A"));
  }

  private static void AssertRecord(Unicode.NET.CodeGen.Models.BlockRecord record, int start, int end, string name)
  {
    Assert.Equal(CodePointRange.Create(start, end), record.Range);
    Assert.Equal(name, record.BlockName);
  }

  private static string WriteTempFixture(string content)
  {
    string path = Path.GetTempFileName();
    File.WriteAllText(path, content);
    return path;
  }
}
