using Unicode.NET.CodeGen.Models;
using Unicode.NET;

namespace Unicode.NET.CodeGen.Parsing;

/// <summary>
/// Parses PropList.txt or DerivedCoreProperties.txt into <see cref="BinaryPropertyRecord"/> entries.
/// Format: start..end; PropertyName  or  start; PropertyName
/// </summary>
public static class BinaryPropertiesParser
{
  /// <summary>Phase 1 binary properties to extract.</summary>
  private static readonly HashSet<string> Phase1Properties = new(StringComparer.Ordinal)
    {
        "Alphabetic",
        "White_Space",
        "Hex_Digit",
        "Default_Ignorable_Code_Point",
        "Noncharacter_Code_Point",
    };

  public static IReadOnlyList<BinaryPropertyRecord> Parse(string filePath, ISet<string>? filter = null)
  {
    var allowed = filter ?? Phase1Properties;
    var records = new List<BinaryPropertyRecord>();

    foreach (var rawLine in File.ReadLines(filePath))
    {
      var line = rawLine;
      int hashIdx = line.IndexOf('#');
      if (hashIdx >= 0) line = line[..hashIdx];
      line = line.Trim();
      if (line.Length == 0) continue;

      var semi = line.IndexOf(';');
      if (semi < 0) continue;

      var rangePart = line[..semi].Trim();
      var propPart = line[(semi + 1)..].Trim();

      // Only keep Phase 1 properties
      if (!allowed.Contains(propPart))
        continue;

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

      records.Add(new BinaryPropertyRecord
      {
        Range = CodePointRange.Create(start, end),
        PropertyName = propPart,
      });
    }

    return records;
  }
}
