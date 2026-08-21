using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Generation;

/// <summary>
/// Generates <c>UnicodeBlocks.{version}.g.cs</c> with a readonly list of block entries.
/// </summary>
public static class BlocksTableGenerator
{
  public static string Generate(
      IReadOnlyList<BlockRecord> blocks,
      string ucdVersion)
  {
    string versionId = ucdVersion.Replace('.', '_');
    string className = $"UnicodeBlocks_{versionId}";

    var w = new CodeWriter();
    w.WriteFileHeader(ucdVersion);
    w.WriteUsing("Unicode.NET");
    w.WriteBlankLine();
    w.WriteNamespaceOpen("Unicode.NET.Generated");

    w.WriteClassOpen("internal static", className);

    w.WriteLine("/// <summary>All Unicode blocks defined in Unicode " + ucdVersion + ".</summary>");
    w.WriteLine("internal static readonly IReadOnlyList<(CodePointRange Range, string Name)> All =");
    w.WriteLine("[");

    // Sort by range start for determinism
    var sorted = blocks.OrderBy(b => b.Range.Start.Value).ToList();
    for (int i = 0; i < sorted.Count; i++)
    {
      var b = sorted[i];
      string comma = i < sorted.Count - 1 ? "," : "";
      w.WriteLine($"    (CodePointRange.Create(0x{b.Range.Start.Value:X}, 0x{b.Range.End.Value:X}), \"{EscapeString(b.BlockName)}\"){comma}");
    }

    w.WriteLine("];");

    w.WriteClassClose();

    return w.GetContent();
  }

  private static string EscapeString(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
