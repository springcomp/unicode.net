using Unicode.NET.CodeGen.Parsing;
using Xunit;

namespace Unicode.NET.CodeGen.UnitTests.Parsing;

public sealed class CaseMappingParserTests
{
  [Fact]
  public void Parse_ExtractsSimpleUpperAndLowerFields()
  {
    var path = Write(string.Join(Environment.NewLine,
      string.Join(';', "0041", "A", "Lu", "0", "L", "", "", "", "", "N", "", "", "", "0061", ""),
      string.Join(';', "0061", "a", "Ll", "0", "L", "", "", "", "", "N", "", "", "0041", "0041", ""),
      string.Join(';', "0020", "SPACE", "Zs", "0", "WS", "", "", "", "", "N", "", "", "", "", "")));

    var records = CaseMappingParser.Parse(path);

    Assert.Equal(2, records.Count);
    Assert.Equal(0x0061, records[0].LowercaseMapping);
    Assert.Null(records[0].UppercaseMapping);
    Assert.Equal(0x0041, records[1].UppercaseMapping);
    Assert.Equal(0x0041, records[1].LowercaseMapping);
  }

  [Fact]
  public void Parse_InvalidScalarAndDuplicate_AreDiagnostic()
  {
    var surrogate = Write(string.Join(';', "D800", "BAD", "Lu", "0", "L", "", "", "", "", "N", "", "", "0061", "", ""));
    var ex = Assert.Throws<InvalidDataException>(() => CaseMappingParser.Parse(surrogate));
    Assert.Contains("line 1", ex.Message);

    var duplicate = Write(string.Join(Environment.NewLine,
      string.Join(';', "0041", "A", "Lu", "0", "L", "", "", "", "", "N", "", "", "", "0061", ""),
      string.Join(';', "0041", "A", "Lu", "0", "L", "", "", "", "", "N", "", "", "", "0062", "")));
    ex = Assert.Throws<InvalidDataException>(() => CaseMappingParser.Parse(duplicate));
    Assert.Contains("duplicate", ex.Message);
  }

  private static string Write(string content)
  {
    var path = Path.GetTempFileName();
    File.WriteAllText(path, content);
    return path;
  }
}
