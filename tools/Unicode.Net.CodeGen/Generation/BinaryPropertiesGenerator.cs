using Unicode.NET.CodeGen.Models;
using Unicode.NET;

namespace Unicode.NET.CodeGen.Generation;

/// <summary>
/// Generates <c>BinaryProperties.{version}.g.cs</c> with binary property range data.
/// </summary>
public static class BinaryPropertiesGenerator
{
  /// <summary>Phase 1 properties in enum declaration order.</summary>
  private static readonly string[] Phase1PropertyNames =
  [
      "Alphabetic",
        "Default_Ignorable_Code_Point",
        "Hex_Digit",
        "Noncharacter_Code_Point",
        "White_Space",
    ];

  public static string Generate(
      IReadOnlyList<BinaryPropertyRecord> records,
      string ucdVersion)
  {
    string versionId = ucdVersion.Replace('.', '_');
    string className = $"BinaryProperties_{versionId}";

    // Group by property name, merge and sort ranges
    var byProperty = records
        .GroupBy(r => r.PropertyName, StringComparer.Ordinal)
        .ToDictionary(
            g => g.Key,
            g => g.OrderBy(r => r.Range.Start.Value).Select(r => r.Range).ToList(),
            StringComparer.Ordinal);

    var w = new CodeWriter();
    w.WriteFileHeader(ucdVersion);
    w.WriteUsing("Unicode.NET");
    w.WriteBlankLine();
    w.WriteNamespaceOpen("Unicode.NET.Generated");

    w.WriteClassOpen("internal static", className);

    w.WriteLine("/// <summary>Maps binary property to its code-point ranges.</summary>");
    w.WriteLine("internal static readonly IReadOnlyDictionary<BinaryProperty, CodePointRange[]> Properties =");
    w.WriteLine("    new Dictionary<BinaryProperty, CodePointRange[]>");
    w.WriteLine("    {");

    foreach (var propName in Phase1PropertyNames)
    {
      if (!byProperty.TryGetValue(propName, out var ranges))
        ranges = [];

      // Map property name to enum value
      string enumValue = $"BinaryProperty.{propName}";
      w.WriteLine($"        {{ {enumValue}, [");
      foreach (var range in ranges)
      {
        w.WriteLine($"            CodePointRange.Create(0x{range.Start.Value:X}, 0x{range.End.Value:X}),");
      }
      w.WriteLine("        ]},");
    }

    w.WriteLine("    };");

    w.WriteClassClose();

    return w.GetContent();
  }
}
