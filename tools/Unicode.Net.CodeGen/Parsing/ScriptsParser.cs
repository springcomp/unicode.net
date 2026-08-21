using Unicode.NET.CodeGen.Models;
using Unicode.NET;

namespace Unicode.NET.CodeGen.Parsing;

/// <summary>
/// Parses Scripts.txt into <see cref="ScriptRecord"/> entries.
/// Format: start..end; ScriptName  or  start; ScriptName
/// </summary>
public static class ScriptsParser
{
  public static IReadOnlyList<ScriptRecord> Parse(string filePath)
  {
    var records = new List<ScriptRecord>();

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
      var namePart = line[(semi + 1)..].Trim();

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

      records.Add(new ScriptRecord
      {
        Range = CodePointRange.Create(start, end),
        ScriptName = namePart,
      });
    }

    return records;
  }
}
