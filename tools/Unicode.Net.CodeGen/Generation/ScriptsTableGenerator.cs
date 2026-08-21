using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Generation;

/// <summary>
/// Generates <c>Scripts.{version}.g.cs</c> with script data.
/// </summary>
public static class ScriptsTableGenerator
{
  public static string Generate(
      IReadOnlyList<ScriptRecord> scriptRecords,
      IReadOnlyList<ScriptExtensionRecord> extensionRecords,
      string ucdVersion)
  {
    string versionId = ucdVersion.Replace('.', '_');
    string className = $"Scripts_{versionId}";

    var w = new CodeWriter();
    w.WriteFileHeader(ucdVersion);
    w.WriteUsing("Unicode.NET");
    w.WriteBlankLine();
    w.WriteNamespaceOpen("Unicode.NET.Generated");

    w.WriteClassOpen("internal static", className);

    // Build script -> ranges mapping
    var scriptRanges = scriptRecords
        .GroupBy(r => r.ScriptName)
        .OrderBy(g => g.Key)
        .ToDictionary(
            g => g.Key,
            g => g.OrderBy(r => r.Range.Start.Value).Select(r => r.Range).ToList()
        );

    w.WriteLine("/// <summary>Maps script name to ranges.</summary>");
    w.WriteLine("internal static readonly IReadOnlyDictionary<string, CodePointRange[]> ScriptRanges =");
    w.WriteLine("    new Dictionary<string, CodePointRange[]>(StringComparer.Ordinal)");
    w.WriteLine("    {");

    foreach (var (scriptName, ranges) in scriptRanges)
    {
      w.WriteLine($"        {{ \"{scriptName}\", [");
      foreach (var range in ranges)
      {
        w.WriteLine($"            CodePointRange.Create(0x{range.Start.Value:X}, 0x{range.End.Value:X}),");
      }
      w.WriteLine("        ]},");
    }

    w.WriteLine("    };");
    w.WriteBlankLine();

    // Build extension map: cp -> script list
    var extensionMap = extensionRecords
        .Where(e => e.Scripts.Count > 0)
        .GroupBy(e => e.CodePoint)
        .OrderBy(g => g.Key)
        .ToDictionary(
            g => g.Key,
            g => g.First().Scripts
        );

    w.WriteLine("/// <summary>Maps code point to script extensions (multi-script characters).</summary>");
    w.WriteLine("internal static readonly IReadOnlyDictionary<int, string[]> ScriptExtensions =");
    w.WriteLine("    new Dictionary<int, string[]>");
    w.WriteLine("    {");

    foreach (var (cp, scripts) in extensionMap)
    {
      var scriptList = string.Join(", ", scripts.Select(s => $"\"{s}\""));
      w.WriteLine($"        {{ 0x{cp:X}, [{scriptList}] }},");
    }

    w.WriteLine("    };");

    w.WriteClassClose();

    return w.GetContent();
  }
}
