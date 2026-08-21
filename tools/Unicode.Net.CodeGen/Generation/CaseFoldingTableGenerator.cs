using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Generation;

/// <summary>
/// Generates <c>CaseFolding.{version}.g.cs</c> with case-folding maps.
/// Emits SimpleMap (C+S records, 1:1) and FullMap (C+F records, 1:N).
/// </summary>
public static class CaseFoldingTableGenerator
{
  /// <summary>
  /// Generates the source file. Emits SimpleMap (C+S) and FullMap (C+F).
  /// </summary>
  public static string Generate(
      IReadOnlyList<CaseFoldingRecord> records,
      string ucdVersion)
  {
    string versionId = ucdVersion.Replace('.', '_');
    string className = $"CaseFolding_{versionId}";

    var w = new CodeWriter();
    w.WriteFileHeader(ucdVersion);
    w.WriteUsing("System.Collections.Frozen");
    w.WriteUsing("Unicode.NET");
    w.WriteBlankLine();
    w.WriteNamespaceOpen("Unicode.NET.Generated");

    w.WriteClassOpen("internal static", className);

    // FullMap: C and F statuses, 1:N code point mapping
    w.WriteLine("/// <summary>");
    w.WriteLine("/// Full case-folding map (status C and F).");
    w.WriteLine("/// Maps a code point to a sequence of folded code points (1:N mapping).");
    w.WriteLine("/// Code points not in this map use simple folding (see <see cref=\"SimpleMap\"/>).");
    w.WriteLine("/// </summary>");
    w.WriteLine("internal static readonly FrozenDictionary<int, int[]> FullMap =");
    w.WriteLine("    BuildFullMap();");
    w.WriteBlankLine();

    // FullMap builder
    w.WriteMethodOpen("private static", "FrozenDictionary<int, int[]>", "BuildFullMap()");
    EmitFullMapEntries(w, records, new HashSet<char> { 'C', 'F' });
    w.WriteMethodClose();
    w.WriteBlankLine();

    // SimpleMap: C and S statuses, single code point mapping
    w.WriteLine("/// <summary>");
    w.WriteLine("/// Simple case-folding map (status C and S).");
    w.WriteLine("/// Maps a code point to its simple case-fold equivalent.");
    w.WriteLine("/// Code points not in this map fold to themselves.");
    w.WriteLine("/// </summary>");
    w.WriteLine("internal static readonly FrozenDictionary<int, int> SimpleMap =");
    w.WriteLine("    BuildSimpleMap();");
    w.WriteBlankLine();

    // SimpleMap builder
    w.WriteMethodOpen("private static", "FrozenDictionary<int, int>", "BuildSimpleMap()");
    EmitSimpleMapEntries(w, records, new HashSet<char> { 'C', 'S' });
    w.WriteMethodClose();

    w.WriteClassClose();

    return w.GetContent();
  }

  /// <summary>
  /// Emits sorted dictionary entries filtered by the given status codes.
  /// Only records with a single-code-point mapping are included.
  /// </summary>
  private static void EmitSimpleMapEntries(
      CodeWriter w,
      IReadOnlyList<CaseFoldingRecord> records,
      HashSet<char> statusFilter)
  {
    var entries = records
        .Where(r => statusFilter.Contains(r.Status) && r.Mapping.Length == 1)
        .GroupBy(r => r.CodePoint)
        .Select(g =>
        {
          // S overrides C for the same code point
          var s = g.FirstOrDefault(r => r.Status == 'S');
          var c = g.FirstOrDefault(r => r.Status == 'C');
          var chosen = s ?? c!;
          return (cp: chosen.CodePoint, mapped: chosen.Mapping[0]);
        })
        .OrderBy(e => e.cp)
        .ToList();

    w.WriteLine("var dict = new Dictionary<int, int>(" + entries.Count + ")");
    w.WriteLine("{");
    for (int i = 0; i < entries.Count; i++)
    {
      var (cp, mapped) = entries[i];
      string comma = i < entries.Count - 1 ? "," : "";
      w.WriteLine($"    {{ 0x{cp:X}, 0x{mapped:X} }}{comma}");
    }
    w.WriteLine("};");
    w.WriteLine("return dict.ToFrozenDictionary();");
  }

  /// <summary>
  /// Emits sorted dictionary entries for the full map (C+F records, 1:N mappings).
  /// </summary>
  private static void EmitFullMapEntries(
      CodeWriter w,
      IReadOnlyList<CaseFoldingRecord> records,
      HashSet<char> statusFilter)
  {
    // Full map: use F record when both C and F exist (prefer F over C for 1:N)
    var entries = records
        .Where(r => statusFilter.Contains(r.Status))
        .GroupBy(r => r.CodePoint)
        .Select(g =>
        {
          var f = g.FirstOrDefault(r => r.Status == 'F');
          var c = g.FirstOrDefault(r => r.Status == 'C');
          var chosen = f ?? c!;
          return (cp: chosen.CodePoint, mapping: chosen.Mapping);
        })
        .OrderBy(e => e.cp)
        .ToList();

    w.WriteLine("var dict = new Dictionary<int, int[]>(" + entries.Count + ")");
    w.WriteLine("{");
    for (int i = 0; i < entries.Count; i++)
    {
      var (cp, mapping) = entries[i];
      string comma = i < entries.Count - 1 ? "," : "";
      string arrayLiteral = "new[] { " + string.Join(", ", mapping.Select(v => $"0x{v:X}")) + " }";
      w.WriteLine($"    {{ 0x{cp:X}, {arrayLiteral} }}{comma}");
    }
    w.WriteLine("};");
    w.WriteLine("return dict.ToFrozenDictionary();");
  }

  /// <summary>
  /// Public entry point for generating a custom-filtered map.
  /// Pass <c>['C','S']</c> for simple folding or <c>['C','F']</c> for full folding
  /// (note: full folding maps to multiple code points — use a different return type).
  /// </summary>
  public static IEnumerable<(int CodePoint, int[] Mapping)> GenerateMap(
      IReadOnlyList<CaseFoldingRecord> records,
      HashSet<char> statusFilter)
  {
    return records
        .Where(r => statusFilter.Contains(r.Status))
        .GroupBy(r => r.CodePoint)
        .Select(g =>
        {
          // prefer higher-specificity status (S over C) when both present
          var best = g.OrderByDescending(r => r.Status == 'S' ? 1 : 0).First();
          return (best.CodePoint, best.Mapping);
        })
        .OrderBy(e => e.CodePoint);
  }
}
