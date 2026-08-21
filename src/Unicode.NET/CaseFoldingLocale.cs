namespace Unicode.NET;

/// <summary>
/// Controls locale-specific case-folding behavior.
/// </summary>
/// <remarks>
/// Turkic (T) mappings apply alternative folding rules for Turkish and Azerbaijani text.
/// Do not infer this from <see cref="System.Globalization.CultureInfo"/> or the process locale.
/// </remarks>
public enum CaseFoldingLocale
{
    /// <summary>Default folding — Turkic (T) mappings are excluded.</summary>
    Default,

    /// <summary>
    /// Turkic folding — T mappings are applied where defined.
    /// Reserved for future implementation alongside full Turkic map generation.
    /// </summary>
    Turkic
}
