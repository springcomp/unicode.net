using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Parsing;

/// <summary>
/// Parses ScriptExtensions.txt into <see cref="ScriptExtensionRecord"/> entries.
/// Format: codepoint(or range); Script1 Script2 Script3...
/// </summary>
public static class ScriptExtensionsParser
{
  public static IReadOnlyList<ScriptExtensionRecord> Parse(string filePath)
  {
    var records = new List<ScriptExtensionRecord>();

    foreach (var rawLine in File.ReadLines(filePath))
    {
      // Strip comment
      var line = rawLine;
      int hashIdx = line.IndexOf('#');
      if (hashIdx >= 0) line = line[..hashIdx];
      line = line.Trim();
      if (line.Length == 0) continue;

      // Split at ';'
      var semi = line.IndexOf(';');
      if (semi < 0) continue;

      var rangePart = line[..semi].Trim();
      var scriptsPart = line[(semi + 1)..].Trim();

      var scripts = scriptsPart.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

      var dotDot = rangePart.IndexOf("..", StringComparison.Ordinal);
      int start, end;
      if (dotDot >= 0)
      {
        start = Convert.ToInt32(rangePart[..dotDot].Trim(), 16);
        end = Convert.ToInt32(rangePart[(dotDot + 2)..].Trim(), 16);
      }
      else
      {
        start = end = Convert.ToInt32(rangePart.Trim(), 16);
      }

      for (int cp = start; cp <= end; cp++)
      {
        records.Add(new ScriptExtensionRecord
        {
          CodePoint = cp,
          Scripts = scripts,
        });
      }
    }

    return records;
  }
}
