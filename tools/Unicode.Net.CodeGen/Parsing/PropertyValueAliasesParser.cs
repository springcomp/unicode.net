namespace Unicode.NET.CodeGen.Parsing;

/// <summary>
/// Parses PropertyValueAliases.txt and builds alias-to-canonical mappings.
/// Format: property ; short_alias ; canonical_name [; additional_alias ...]  [# comment with sub-members]
/// </summary>
public static class PropertyValueAliasesParser
{
  /// <summary>
  /// Parses all general category (gc) aliases from the file.
  /// Returns a dictionary from every alias/short-name to the canonical long name.
  /// Keys are normalized (lowercase, underscores/hyphens stripped).
  /// </summary>
  public static IReadOnlyDictionary<string, string> ParseGeneralCategoryAliases(string filePath)
  {
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    foreach (var rawLine in File.ReadLines(filePath))
    {
      // Strip inline comment (but keep comment-only lines ignored)
      var line = rawLine;
      int hashIdx = line.IndexOf('#');
      if (hashIdx >= 0) line = line[..hashIdx];
      line = line.Trim();
      if (line.Length == 0) continue;

      var fields = line.Split(';');
      if (fields.Length < 3) continue;

      string property = fields[0].Trim();
      if (!property.Equals("gc", StringComparison.OrdinalIgnoreCase))
        continue;

      // canonical = fields[2]; short alias = fields[1]
      string canonical = fields[2].Trim();

      // Register every alias (fields[1..]) -> canonical
      for (int i = 1; i < fields.Length; i++)
      {
        string alias = fields[i].Trim();
        if (alias.Length == 0) continue;
        string key = Normalize(alias);
        result[key] = canonical;
      }

      // Also register the canonical itself
      result[Normalize(canonical)] = canonical;
    }

    return result;
  }

  /// <summary>
  /// Parses all script (sc) aliases from the file.
  /// Returns a dictionary from every alias/short-name to the canonical script name.
  /// Keys are normalized (lowercase, underscores/hyphens stripped).
  /// </summary>
  public static IReadOnlyDictionary<string, string> ParseScriptAliases(string filePath)
  {
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    foreach (var rawLine in File.ReadLines(filePath))
    {
      var line = rawLine;
      int hashIdx = line.IndexOf('#');
      if (hashIdx >= 0) line = line[..hashIdx];
      line = line.Trim();
      if (line.Length == 0) continue;

      var fields = line.Split(';');
      if (fields.Length < 3) continue;

      string property = fields[0].Trim();
      if (!property.Equals("sc", StringComparison.OrdinalIgnoreCase))
        continue;

      string canonical = fields[2].Trim();

      for (int i = 1; i < fields.Length; i++)
      {
        string alias = fields[i].Trim();
        if (alias.Length == 0) continue;
        string key = Normalize(alias);
        result[key] = canonical;
      }

      result[Normalize(canonical)] = canonical;
    }

    return result;
  }

  /// <summary>Normalize a property-value name: lowercase + strip underscores and hyphens.</summary>
  public static string Normalize(string name) =>
      name.ToLowerInvariant().Replace("_", "").Replace("-", "");
}
