using Unicode.NET.CodeGen.Models;
using Unicode.NET.CodeGen.Parsing;
using Xunit;

namespace Unicode.NET.CodeGen.UnitTests.Parsing;

public sealed class SpecialCasingParserTests
{
  [Fact]
  public void Parse_PreservesExpansionsAndClassifiesConditions()
  {
    var path = Write("00DF;00DF;0053 0073;0053 0053;" + Environment.NewLine +
                     "03A3;03C3;03A3;03A3; Final_Sigma" + Environment.NewLine +
                     "0069;0069;0049;0130; tr");

    var records = SpecialCasingParser.Parse(path);

    Assert.Equal(new[] { 0x0053, 0x0053 }, records[0].UppercaseMapping);
    Assert.Contains(records[1].ContextPredicates, c => c == "Final_Sigma");
    Assert.Contains(records[2].LocaleTags, c => c == "tr");
    Assert.Equal(SpecialCasingConditionKind.LocaleTag, records[2].Conditions[0].Kind);
  }

  [Fact]
  public void Parse_RejectsMalformedAndDuplicateRecords()
  {
    var malformed = Write("03A3;03C2;03A3");
    var ex = Assert.Throws<InvalidDataException>(() => SpecialCasingParser.Parse(malformed));
    Assert.Contains("line 1", ex.Message);

    var duplicate = Write("03A3;03C2;03A3;03A3; Final_Sigma" + Environment.NewLine +
                          "03A3;03C2;03A3;03A3; Final_Sigma");
    ex = Assert.Throws<InvalidDataException>(() => SpecialCasingParser.Parse(duplicate));
    Assert.Contains("duplicate", ex.Message);
  }

  private static string Write(string content)
  {
    var path = Path.GetTempFileName();
    File.WriteAllText(path, content);
    return path;
  }
}
