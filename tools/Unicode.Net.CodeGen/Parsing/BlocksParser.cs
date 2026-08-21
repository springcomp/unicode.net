using Unicode.NET.CodeGen.Models;
using Unicode.NET;

namespace Unicode.NET.CodeGen.Parsing;

/// <summary>
/// Parses Blocks.txt into <see cref="BlockRecord"/> entries.
/// Format: start..end; Block Name  (# comments and blank lines ignored)
/// </summary>
public static class BlocksParser
{
  public static IReadOnlyList<BlockRecord> Parse(string filePath)
  {
    var records = new List<BlockRecord>();

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
      if (dotDot < 0) continue;

      int start = Convert.ToInt32(rangePart[..dotDot].Trim(), 16);
      int end = Convert.ToInt32(rangePart[(dotDot + 2)..].Trim(), 16);

      records.Add(new BlockRecord
      {
        Range = CodePointRange.Create(start, end),
        BlockName = namePart,
      });
    }

    return records;
  }
}
