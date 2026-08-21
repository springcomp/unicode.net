namespace Unicode.NET.CodeGen.Generation;

/// <summary>
/// Generates <c>PropertyAliases.{version}.g.cs</c> with general-category alias
/// dictionaries keyed by normalized alias name.
/// </summary>
public static class PropertyAliasesGenerator
{
  public static string Generate(
      IReadOnlyDictionary<string, string> gcAliases,
      IReadOnlyDictionary<string, string> scriptAliases,
      string ucdVersion)
  {
    string versionId = ucdVersion.Replace('.', '_');
    string className = $"PropertyAliases_{versionId}";

    var w = new CodeWriter();
    w.WriteFileHeader(ucdVersion);
    w.WriteUsing("System.Collections.Generic");
    w.WriteBlankLine();
    w.WriteNamespaceOpen("Unicode.NET.Generated");

    w.WriteClassOpen("internal static", className);

    // Static dictionary field
    w.WriteLine("/// <summary>");
    w.WriteLine("/// Maps normalized general category alias (lowercase, no underscores/hyphens)");
    w.WriteLine("/// to canonical long name (e.g., \"Uppercase_Letter\").");
    w.WriteLine("/// </summary>");
    w.WriteLine("internal static readonly IReadOnlyDictionary<string, string> GeneralCategoryAliases =");
    w.WriteLine("    new Dictionary<string, string>(StringComparer.Ordinal)");
    w.WriteLine("    {");

    foreach (var (key, value) in gcAliases.OrderBy(kv => kv.Key, StringComparer.Ordinal))
    {
      // Escape strings for C# literal
      string escapedKey = key.Replace("\\", "\\\\").Replace("\"", "\\\"");
      string escapedValue = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
      w.WriteLine($"        {{ \"{escapedKey}\", \"{escapedValue}\" }},");
    }

    w.WriteLine("    };");
    w.WriteBlankLine();

    // Script aliases dictionary
    w.WriteLine("/// <summary>");
    w.WriteLine("/// Maps normalized script alias (lowercase, no underscores/hyphens)");
    w.WriteLine("/// to canonical script name (e.g., \"Latin\", \"Greek\").");
    w.WriteLine("/// </summary>");
    w.WriteLine("internal static readonly IReadOnlyDictionary<string, string> ScriptAliases =");
    w.WriteLine("    new Dictionary<string, string>(StringComparer.Ordinal)");
    w.WriteLine("    {");

    foreach (var (key, value) in scriptAliases.OrderBy(kv => kv.Key, StringComparer.Ordinal))
    {
      string escapedKey = key.Replace("\\", "\\\\").Replace("\"", "\\\"");
      string escapedValue = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
      w.WriteLine($"        {{ \"{escapedKey}\", \"{escapedValue}\" }},");
    }

    w.WriteLine("    };");

    w.WriteClassClose();

    return w.GetContent();
  }
}
