using Unicode.NET.CodeGen.Models;
using Unicode.NET;

namespace Unicode.NET.CodeGen.Parsing;

public static class CaseMappingParser
{
  public static IReadOnlyList<CaseMappingRecord> Parse(string filePath)
  {
    var records = new List<CaseMappingRecord>();
    var seen = new HashSet<int>();
    int lineNumber = 0;
    foreach (var rawLine in File.ReadLines(filePath))
    {
      lineNumber++;
      if (string.IsNullOrWhiteSpace(rawLine)) continue;
      var fields = rawLine.Split(';');
      if (fields.Length < 14)
        throw Error(filePath, lineNumber, "expected at least 14 semicolon-separated fields");
      int source = ParseCodePoint(fields[0], filePath, lineNumber, "source");
      if (!seen.Add(source))
        throw Error(filePath, lineNumber, $"duplicate source U+{source:X4}");
      var uppercase = ParseOptionalScalar(fields[12], filePath, lineNumber, "uppercase mapping");
      var lowercase = ParseOptionalScalar(fields[13], filePath, lineNumber, "lowercase mapping");
      if (uppercase is null && lowercase is null)
        continue;
      if (source is >= CodePoint.HighSurrogateStart and <= CodePoint.LowSurrogateEnd)
        throw Error(filePath, lineNumber, $"invalid Unicode scalar in source: '{fields[0].Trim()}'");
      records.Add(new CaseMappingRecord
      {
        CodePoint = source,
        UppercaseMapping = uppercase,
        LowercaseMapping = lowercase,
      });
    }
    return records;
  }

  private static int? ParseOptionalScalar(string value, string file, int line, string field)
    => string.IsNullOrWhiteSpace(value) ? null : ParseScalar(value, file, line, field);

  internal static int ParseScalar(string value, string file, int line, string field)
  {
    int result = ParseCodePoint(value, file, line, field);
    if (result is >= CodePoint.HighSurrogateStart and <= CodePoint.LowSurrogateEnd)
      throw Error(file, line, $"invalid Unicode scalar in {field}: '{value.Trim()}'");
    return result;
  }

  private static int ParseCodePoint(string value, string file, int line, string field)
  {
    try
    {
      int result = Convert.ToInt32(value.Trim(), 16);
      if (result < CodePoint.MinValue || result > CodePoint.MaxValue)
        throw new FormatException();
      return result;
    }
    catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
    {
      throw Error(file, line, $"invalid Unicode code point in {field}: '{value.Trim()}'", ex);
    }
  }

  private static InvalidDataException Error(string file, int line, string reason, Exception? inner = null)
    => new($"{Path.GetFileName(file)}, line {line}: {reason}.", inner);
}
