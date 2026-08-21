namespace Unicode.NET.CodeGen.Models;

/// <summary>
/// Represents one mapping entry from CaseFolding.txt.
/// Status codes:
///   C = Common (simple + full)
///   F = Full (multi-code-point, full folding only)
///   S = Simple (overrides C for simple folding when different)
///   T = Turkic (special handling for dotted/dotless I)
/// </summary>
public sealed class CaseFoldingRecord
{
  /// <summary>Source code point.</summary>
  public required int CodePoint { get; init; }

  /// <summary>Status code: C, F, S, or T.</summary>
  public required char Status { get; init; }

  /// <summary>
  /// Mapping code points. Single element for C/S/T, multiple for F.
  /// </summary>
  public required int[] Mapping { get; init; }
}
