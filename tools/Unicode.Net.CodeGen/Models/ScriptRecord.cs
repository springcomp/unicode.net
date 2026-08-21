using Unicode.NET;

namespace Unicode.NET.CodeGen.Models;

/// <summary>Represents one script assignment entry from Scripts.txt.</summary>
public sealed class ScriptRecord
{
  public required CodePointRange Range { get; init; }
  public required string ScriptName { get; init; }
}
