namespace Unicode.NET.CodeGen.Models;

/// <summary>Represents one script extension entry from ScriptExtensions.txt.</summary>
public sealed class ScriptExtensionRecord
{
  public required int CodePoint { get; init; }
  public required List<string> Scripts { get; init; }
}
