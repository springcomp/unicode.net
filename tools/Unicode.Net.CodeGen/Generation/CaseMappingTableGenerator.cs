using Unicode.NET.CodeGen.Models;

namespace Unicode.NET.CodeGen.Generation;

/// <summary>Generates immutable default case-mapping data for one UCD release.</summary>
public static class CaseMappingTableGenerator
{
  public static string Generate(
      IReadOnlyList<CaseMappingRecord> simpleRecords,
      IReadOnlyList<SpecialCasingRecord> specialRecords,
      IReadOnlyList<BinaryPropertyRecord> binaryRecords,
      string ucdVersion)
  {
    var unconditional = specialRecords.Where(r => r.Conditions.Count == 0).ToList();
    var contextual = specialRecords.Where(r => r.Conditions.Count > 0 &&
                                               !r.Conditions.Any(c => c.Kind == SpecialCasingConditionKind.LocaleTag)).ToList();
    foreach (var record in contextual)
    {
      if (record.Conditions.Any(c => c.Kind == SpecialCasingConditionKind.ContextPredicate &&
                                     c.Token != "Final_Sigma"))
        throw new InvalidDataException(
            $"SpecialCasing contains unsupported locale-neutral context for U+{record.Source:X4}.");
    }

    string className = $"CaseMapping_{ucdVersion.Replace('.', '_')}";
    var w = new CodeWriter();
    w.WriteFileHeader(ucdVersion);
    w.WriteUsing("System.Collections.Frozen");
    w.WriteUsing("Unicode.NET");
    w.WriteBlankLine();
    w.WriteNamespaceOpen("Unicode.NET.Generated");
    w.WriteClassOpen("internal static", className);

    EmitMap(w, "SimpleLowercaseMap", simpleRecords.Where(r => r.LowercaseMapping.HasValue)
      .Select(r => (r.CodePoint, r.LowercaseMapping!.Value)));
    EmitMap(w, "SimpleUppercaseMap", simpleRecords.Where(r => r.UppercaseMapping.HasValue)
      .Select(r => (r.CodePoint, r.UppercaseMapping!.Value)));
    EmitSequenceMap(w, "FullLowercaseMap", unconditional.Select(r => (r.Source, r.LowercaseMapping)));
    EmitSequenceMap(w, "FullUppercaseMap", unconditional.Select(r => (r.Source, r.UppercaseMapping)));

    w.WriteLine("internal static readonly CaseMappingContextRule[] ContextualRules =");
    w.WriteLine("    [");
    foreach (var record in contextual.OrderBy(r => r.Source).ThenBy(r => string.Join(" ", r.ContextPredicates), StringComparer.Ordinal))
    {
      var predicate = record.ContextPredicates.Single();
      w.WriteLine($"        new(0x{record.Source:X}, \"{predicate}\", " +
                  $"new[] {{ {FormatSequence(record.LowercaseMapping)} }}, " +
                  $"new[] {{ {FormatSequence(record.UppercaseMapping)} }}),");
    }
    w.WriteLine("    ];");
    w.WriteBlankLine();

    var cased = GetRanges(binaryRecords, "Cased");
    var ignorable = GetRanges(binaryRecords, "Case_Ignorable");
    EmitRanges(w, "Cased", cased);
    EmitRanges(w, "CaseIgnorable", ignorable);
    w.WriteBlankLine();
    w.WriteLine("internal static readonly CaseMappingData Data = new(");
    w.WriteLine("    SimpleLowercaseMap, SimpleUppercaseMap, FullLowercaseMap, FullUppercaseMap,");
    w.WriteLine("    ContextualRules, Cased, CaseIgnorable);");
    w.WriteClassClose();
    return w.GetContent();
  }

  private static void EmitMap(CodeWriter w, string name, IEnumerable<(int Source, int Mapping)> source)
  {
    var entries = source.Where(e => e.Source != e.Mapping).OrderBy(e => e.Source).ToArray();
    w.WriteLine($"internal static readonly FrozenDictionary<int, int> {name} =");
    w.WriteLine($"    new Dictionary<int, int>({entries.Length})");
    w.WriteLine("    {");
    foreach (var entry in entries)
      w.WriteLine($"        {{ 0x{entry.Source:X}, 0x{entry.Mapping:X} }},");
    w.WriteLine("    }.ToFrozenDictionary();");
    w.WriteBlankLine();
  }

  private static void EmitSequenceMap(CodeWriter w, string name, IEnumerable<(int Source, int[] Mapping)> source)
  {
    var entries = source.Where(e => e.Mapping.Length > 0 && !(e.Mapping.Length == 1 && e.Mapping[0] == e.Source))
      .GroupBy(e => e.Source).Select(g => g.First()).OrderBy(e => e.Source).ToArray();
    w.WriteLine($"internal static readonly FrozenDictionary<int, int[]> {name} =");
    w.WriteLine($"    new Dictionary<int, int[]>({entries.Length})");
    w.WriteLine("    {");
    foreach (var entry in entries)
      w.WriteLine($"        {{ 0x{entry.Source:X}, new[] {{ {FormatSequence(entry.Mapping)} }} }},");
    w.WriteLine("    }.ToFrozenDictionary();");
    w.WriteBlankLine();
  }

  private static void EmitRanges(CodeWriter w, string name, IReadOnlyList<CodePointRange> ranges)
  {
    w.WriteLine($"internal static readonly CodePointRange[] {name} =");
    w.WriteLine("    [");
    foreach (var range in ranges)
      w.WriteLine($"        CodePointRange.Create(0x{range.Start.Value:X}, 0x{range.End.Value:X}),");
    w.WriteLine("    ];");
  }

  private static IReadOnlyList<CodePointRange> GetRanges(
      IReadOnlyList<BinaryPropertyRecord> records, string propertyName)
  {
    var result = new List<CodePointRange>();
    foreach (var range in records.Where(r => r.PropertyName == propertyName)
      .OrderBy(r => r.Range.Start.Value).ThenBy(r => r.Range.End.Value).Select(r => r.Range))
    {
      if (result.Count > 0 && range.Start.Value <= result[^1].End.Value + 1)
      {
        var previous = result[^1];
        result[^1] = CodePointRange.Create(previous.Start.Value, Math.Max(previous.End.Value, range.End.Value));
      }
      else
        result.Add(range);
    }
    return result;
  }

  private static string FormatSequence(IEnumerable<int> values) =>
    string.Join(", ", values.Select(v => $"0x{v:X}"));
}
