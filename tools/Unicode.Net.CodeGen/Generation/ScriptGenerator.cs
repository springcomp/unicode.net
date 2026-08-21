using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Generation;

/// <summary>
/// Generates <c>Script.cs</c> enum with all script values from Scripts.txt.
/// </summary>
public static class ScriptGenerator
{
  public static string Generate(IReadOnlyList<ScriptRecord> records)
  {
    // Collect unique script names
    var scriptNames = records
        .Select(r => r.ScriptName)
        .Distinct()
        .OrderBy(s => s, StringComparer.Ordinal)
        .ToList();

    var w = new CodeWriter();
    w.WriteLine("namespace Unicode.NET;");
    w.WriteBlankLine();
    w.WriteLine("/// <summary>Unicode Script property (ISO 15924 four-letter codes).</summary>");
    w.WriteLine("public enum Script");
    w.WriteLine("{");
    w.WriteLine("    /// <summary>Unknown or unassigned script.</summary>");
    w.WriteLine("    Unknown = 0,");

    for (int i = 0; i < scriptNames.Count; i++)
    {
      var scriptName = scriptNames[i];
      // Convert underscores to spaces for doc comment
      var friendlyName = scriptName.Replace('_', ' ');
      w.WriteLine("");
      w.WriteLine($"    /// <summary>{friendlyName} script.</summary>");
      w.WriteLine($"    {scriptName},");
    }

    w.WriteLine("}");

    return w.GetContent();
  }
}
