namespace Unicode.NET.CodeGen.Models;

/// <summary>
/// Represents one record from UnicodeData.txt, or a merged range from a First/Last pair.
/// </summary>
public sealed class UnicodeCharacterRecord
{
  /// <summary>First code point of this record (inclusive).</summary>
  public required int StartCodePoint { get; init; }

  /// <summary>
  /// Last code point of this record (inclusive).
  /// Equal to <see cref="StartCodePoint"/> for single-code-point records.
  /// </summary>
  public required int EndCodePoint { get; init; }

  /// <summary>Unicode general category (e.g. "Lu", "Nd").</summary>
  public required string GeneralCategory { get; init; }

  /// <summary>Character name from UnicodeData.txt field 1.</summary>
  public required string Name { get; init; }

  /// <summary>True when this record spans a range (First/Last pair or derived range).</summary>
  public bool IsRange => EndCodePoint != StartCodePoint;
}
