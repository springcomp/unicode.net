using Unicode.NET;
using Unicode.NET.CodeGen.Generation;
using Unicode.NET.CodeGen.Models;
using Xunit;

namespace Unicode.NET.CodeGen.UnitTests.Generation;

public sealed class CaseMappingTableGeneratorTests
{
  [Fact]
  public void Generate_ComposesMappingsContextAndPropertiesDeterministically()
  {
    var simple = new[]
    {
      new CaseMappingRecord { CodePoint = 0x41, LowercaseMapping = 0x61 },
      new CaseMappingRecord { CodePoint = 0x61, UppercaseMapping = 0x41 },
    };
    var special = new[]
    {
      new SpecialCasingRecord
      {
        Source = 0xDF, LowercaseMapping = new[] { 0xDF }, TitlecaseMapping = new[] { 0x53, 0x74 },
        UppercaseMapping = new[] { 0x53, 0x53 }, Conditions = Array.Empty<SpecialCasingCondition>()
      },
      new SpecialCasingRecord
      {
        Source = 0x3A3, LowercaseMapping = new[] { 0x3C2 }, TitlecaseMapping = new[] { 0x3A3 },
        UppercaseMapping = new[] { 0x3A3 },
        Conditions = new[] { new SpecialCasingCondition { Token = "Final_Sigma", Kind = SpecialCasingConditionKind.ContextPredicate } }
      },
      new SpecialCasingRecord
      {
        Source = 0x130, LowercaseMapping = new[] { 0x69 }, TitlecaseMapping = new[] { 0x130 },
        UppercaseMapping = new[] { 0x130 },
        Conditions = new[] { new SpecialCasingCondition { Token = "tr", Kind = SpecialCasingConditionKind.LocaleTag } }
      },
    };
    var properties = new[]
    {
      new BinaryPropertyRecord { PropertyName = "Cased", Range = CodePointRange.Create(0x41, 0x5A) },
      new BinaryPropertyRecord { PropertyName = "Case_Ignorable", Range = CodePointRange.Create(0x27, 0x27) },
    };

    string first = CaseMappingTableGenerator.Generate(simple, special, properties, "15.1.0");
    string second = CaseMappingTableGenerator.Generate(simple, special, properties, "15.1.0");

    Assert.Equal(first, second);
    Assert.Contains("{ 0xDF, new[] { 0x53, 0x53 } }", first);
    Assert.Contains("new(0x3A3, \"Final_Sigma\"", first);
    Assert.DoesNotContain("0x130", first);
    Assert.Contains("CodePointRange.Create(0x41, 0x5A)", first);
    Assert.Contains("CodePointRange.Create(0x27, 0x27)", first);
  }

  [Fact]
  public void Generate_RejectsUnknownContextPredicate()
  {
    var record = new SpecialCasingRecord
    {
      Source = 1, LowercaseMapping = new[] { 1 }, TitlecaseMapping = new[] { 1 },
      UppercaseMapping = new[] { 1 },
      Conditions = new[] { new SpecialCasingCondition { Token = "Unknown", Kind = SpecialCasingConditionKind.ContextPredicate } }
    };

    var ex = Assert.Throws<InvalidDataException>(() =>
      CaseMappingTableGenerator.Generate(Array.Empty<CaseMappingRecord>(), new[] { record },
        Array.Empty<BinaryPropertyRecord>(), "15.1.0"));
    Assert.Contains("unsupported", ex.Message);
  }
}
