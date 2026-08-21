using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Parsing;

/// <summary>
/// Parses UnicodeData.txt into <see cref="UnicodeCharacterRecord"/> objects.
/// Handles First/Last paired lines by merging them into single range records.
/// </summary>
public static class UnicodeDataParser
{
  public static IReadOnlyList<UnicodeCharacterRecord> Parse(string filePath)
  {
    var lines = File.ReadAllLines(filePath);
    var records = new List<UnicodeCharacterRecord>(lines.Length);

    UnicodeCharacterRecord? pendingFirst = null;

    foreach (var line in lines)
    {
      if (string.IsNullOrWhiteSpace(line)) continue;

      var fields = line.Split(';');
      if (fields.Length < 3) continue;

      int codePoint = Convert.ToInt32(fields[0].Trim(), 16);
      string name = fields[1].Trim();
      string category = fields[2].Trim();

      if (name.EndsWith(", First>", StringComparison.Ordinal))
      {
        // Start of a range pair — hold pending
        pendingFirst = new UnicodeCharacterRecord
        {
          StartCodePoint = codePoint,
          EndCodePoint = codePoint,
          GeneralCategory = category,
          Name = name,
        };
      }
      else if (name.EndsWith(", Last>", StringComparison.Ordinal))
      {
        // Close the pending range
        if (pendingFirst is null)
          throw new InvalidDataException($"Unexpected Last> at U+{codePoint:X4} with no matching First>.");

        records.Add(new UnicodeCharacterRecord
        {
          StartCodePoint = pendingFirst.StartCodePoint,
          EndCodePoint = codePoint,
          GeneralCategory = pendingFirst.GeneralCategory,
          Name = pendingFirst.Name,
        });
        pendingFirst = null;
      }
      else
      {
        if (pendingFirst is not null)
          throw new InvalidDataException($"Orphaned First> at U+{pendingFirst.StartCodePoint:X4} — no Last> before U+{codePoint:X4}.");

        records.Add(new UnicodeCharacterRecord
        {
          StartCodePoint = codePoint,
          EndCodePoint = codePoint,
          GeneralCategory = category,
          Name = name,
        });
      }
    }

    if (pendingFirst is not null)
      throw new InvalidDataException($"Unclosed First> at U+{pendingFirst.StartCodePoint:X4} at end of file.");

    return records;
  }
}
