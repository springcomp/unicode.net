namespace Unicode.NET.CodeGen.Models;

/// <summary>Represents the simple mappings in fields 12 and 13 of UnicodeData.txt.</summary>
public sealed class CaseMappingRecord
{
  public required int CodePoint { get; init; }
  public int? UppercaseMapping { get; init; }
  public int? LowercaseMapping { get; init; }

  public int? SimpleUppercaseMapping => UppercaseMapping;
  public int? SimpleLowercaseMapping => LowercaseMapping;
}
