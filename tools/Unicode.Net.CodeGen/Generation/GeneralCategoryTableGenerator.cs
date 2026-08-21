using Unicode.NET.CodeGen.Models;
using Unicode.NET;

namespace Unicode.NET.CodeGen.Generation;

/// <summary>
/// Generates <c>GeneralCategories.{version}.g.cs</c> with one <see cref="CodePointSet"/>
/// per Unicode general category plus a <c>GetGeneralCategory(CodePoint)</c> lookup.
/// </summary>
public static class GeneralCategoryTableGenerator
{
  public static string Generate(
      IReadOnlyList<UnicodeCharacterRecord> records,
      string ucdVersion)
  {
    // Group records by general category and build CodePointSets
    var byCategory = new Dictionary<string, CodePointSetBuilder>(StringComparer.Ordinal);

    foreach (var rec in records)
    {
      if (!byCategory.TryGetValue(rec.GeneralCategory, out var builder))
      {
        builder = new CodePointSetBuilder();
        byCategory[rec.GeneralCategory] = builder;
      }
      builder.Add(CodePointRange.Create(rec.StartCodePoint, rec.EndCodePoint));
    }

    var assignedSets = byCategory
        .Select(kv => (Category: kv.Key, Set: kv.Value.Build()))
        .OrderBy(x => x.Category, StringComparer.Ordinal)
        .ToList();

    // Cn (unassigned) = complement of all assigned code points.
    // UnicodeData.txt does not list Cn explicitly.
    var assignedUnion = assignedSets
        .Select(x => x.Set)
        .Aggregate(CodePointSet.Empty, (acc, s) => acc.Union(s));
    var cnSet = assignedUnion.Complement();

    var categories = assignedSets
        .Append((Category: "Cn", Set: cnSet))
        .OrderBy(x => x.Category, StringComparer.Ordinal)
        .ToList();

    // Version identifier safe for C# identifiers: "16.0.0" -> "16_0_0"
    string versionId = ucdVersion.Replace('.', '_');
    string className = $"GeneralCategories_{versionId}";

    var w = new CodeWriter();
    w.WriteFileHeader(ucdVersion);
    w.WriteUsing("Unicode.NET");
    w.WriteBlankLine();
    w.WriteNamespaceOpen("Unicode.NET.Generated");

    w.WriteClassOpen("internal static", className);

    // One field per category
    foreach (var (cat, set) in categories)
    {
      var ranges = set.Ranges.ToList();
      w.WriteLine($"/// <summary>General category {cat}: {set.RangeCount} range(s), {set.Count} code point(s).</summary>");
      w.WriteLine($"internal static readonly CodePointSet {cat} = BuildSet_{cat}();");
      w.WriteBlankLine();
    }

    // Lookup method
    w.WriteMethodOpen("internal static", "string?", "GetGeneralCategory(CodePoint cp)");
    foreach (var (cat, _) in categories)
    {
      w.WriteLine($"if ({cat}.Contains(cp)) return \"{cat}\";");
    }
    w.WriteLine("return null;");
    w.WriteMethodClose();
    w.WriteBlankLine();

    // Builder methods
    foreach (var (cat, set) in categories)
    {
      w.WriteMethodOpen("private static", "CodePointSet", $"BuildSet_{cat}()");
      w.WriteLine("var b = new CodePointSetBuilder();");
      foreach (var r in set.Ranges.OrderBy(r => r.Start.Value))
      {
        if (r.Start == r.End)
          w.WriteLine($"b.Add(CodePointRange.Create(0x{r.Start.Value:X}, 0x{r.End.Value:X}));");
        else
          w.WriteLine($"b.Add(CodePointRange.Create(0x{r.Start.Value:X}, 0x{r.End.Value:X}));");
      }
      w.WriteLine("return b.Build();");
      w.WriteMethodClose();
      w.WriteBlankLine();
    }

    w.WriteClassClose();

    return w.GetContent();
  }
}
