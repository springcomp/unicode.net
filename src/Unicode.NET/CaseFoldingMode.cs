namespace Unicode.NET;

/// <summary>
/// Controls whether simple (1:1) or full (1:N) Unicode case-folding mappings are applied.
/// </summary>
/// <remarks>
/// Turkic (T) mappings are excluded from both modes by default.
/// Use <see cref="CaseFoldingLocale.Turkic"/> to opt into Turkic-specific behavior.
/// Both members are defined now so that adding Full mode later is purely additive
/// (no enum member additions = no breaking change).
/// </remarks>
public enum CaseFoldingMode
{
    /// <summary>
    /// Simple folding: each code point maps to exactly one code point (C and S records).
    /// </summary>
    Simple,

    /// <summary>
    /// Full folding: each code point may map to a sequence of code points (C and F records).
    /// Full folding is designed-in and reserved for future implementation.
    /// </summary>
    Full
}
