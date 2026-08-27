using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Parsing;

public static class SpecialCasingParser
{
  private static readonly HashSet<string> LocaleTags = new(StringComparer.OrdinalIgnoreCase) { "az", "lt", "tr" };

  public static IReadOnlyList<SpecialCasingRecord> Parse(string filePath)
  {
    var records = new List<SpecialCasingRecord>();
    var seen = new HashSet<string>(StringComparer.Ordinal);
    int lineNumber = 0;
    foreach (var rawLine in File.ReadLines(filePath))
    {
      lineNumber++;
      var line = rawLine;
      int comment = line.IndexOf('#');
      if (comment >= 0) line = line[..comment];
      if (string.IsNullOrWhiteSpace(line)) continue;
      var fields = line.Split(';');
      if (fields.Length > 4 && string.IsNullOrWhiteSpace(fields[^1]))
        fields = fields[..^1];
      if (fields.Length is < 4 or > 5)
        throw Error(filePath, lineNumber, "expected four or five fields");
      int source = CaseMappingParser.ParseScalar(fields[0], filePath, lineNumber, "source");
      var lower = ParseSequence(fields[1], filePath, lineNumber, "lowercase mapping");
      var title = ParseSequence(fields[2], filePath, lineNumber, "titlecase mapping");
      var upper = ParseSequence(fields[3], filePath, lineNumber, "uppercase mapping");
      var conditions = fields.Length == 5
        ? fields[4].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => new SpecialCasingCondition
            {
              Token = token,
              Kind = LocaleTags.Contains(token)
                ? SpecialCasingConditionKind.LocaleTag
                : SpecialCasingConditionKind.ContextPredicate,
            }).ToArray()
        : Array.Empty<SpecialCasingCondition>();
      string key = $"{source:X};{string.Join(' ', conditions.Select(c => c.Token))}";
      if (!seen.Add(key))
        throw Error(filePath, lineNumber, $"duplicate logical record for U+{source:X4}");
      records.Add(new SpecialCasingRecord
      {
        Source = source, LowercaseMapping = lower, TitlecaseMapping = title,
        UppercaseMapping = upper, Conditions = conditions,
      });
    }
    return records;
  }

  private static int[] ParseSequence(string value, string file, int line, string field)
    => value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
      .Select(token => CaseMappingParser.ParseScalar(token, file, line, field)).ToArray();

  private static InvalidDataException Error(string file, int line, string reason)
    => new($"{Path.GetFileName(file)}, line {line}: {reason}.");
}
