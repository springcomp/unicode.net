using Unicode.NET;

namespace Unicode.NET.CodeGen.Models;

/// <summary>Represents one Unicode block entry from Blocks.txt.</summary>
public sealed class BlockRecord
{
  public required CodePointRange Range { get; init; }
  public required string BlockName { get; init; }
}
