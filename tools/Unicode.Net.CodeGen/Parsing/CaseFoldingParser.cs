using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Parsing;

/// <summary>
/// Parses CaseFolding.txt into <see cref="CaseFoldingRecord"/> objects.
/// Format: code; status; mapping; # comment
/// Preserves all four statuses (C, F, S, T) in the parsed model.
/// </summary>
public static class CaseFoldingParser
{
  public static IReadOnlyList<CaseFoldingRecord> Parse(string filePath)
  {
    var records = new List<CaseFoldingRecord>();

    foreach (var rawLine in File.ReadLines(filePath))
    {
      // Strip comment
      var line = rawLine;
      int hashIdx = line.IndexOf('#');
      if (hashIdx >= 0) line = line[..hashIdx];
      line = line.Trim();
      if (line.Length == 0) continue;

      var fields = line.Split(';');
      if (fields.Length < 3) continue;

      int codePoint = Convert.ToInt32(fields[0].Trim(), 16);
      char status = fields[1].Trim()[0];

      // Mapping may be single or space-separated multiple code points (F status)
      var mappingParts = fields[2].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
      var mapping = mappingParts.Select(p => Convert.ToInt32(p.Trim(), 16)).ToArray();

      records.Add(new CaseFoldingRecord
      {
        CodePoint = codePoint,
        Status = status,
        Mapping = mapping,
      });
    }

    return records;
  }
}
