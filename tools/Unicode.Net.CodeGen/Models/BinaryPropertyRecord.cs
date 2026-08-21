using Unicode.NET;

namespace Unicode.NET.CodeGen.Models;

/// <summary>Represents one binary-property assignment entry from PropList.txt or DerivedCoreProperties.txt.</summary>
public sealed class BinaryPropertyRecord
{
  public required CodePointRange Range { get; init; }
  public required string PropertyName { get; init; }
}
