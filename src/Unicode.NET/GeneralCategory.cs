namespace Unicode.NET;

/// <summary>
/// Unicode general category values as defined in the Unicode Character Database.
/// </summary>
public enum GeneralCategory
{
    // ── Letter ──────────────────────────────────────────────────────────────────
    /// <summary>Uppercase letter (Lu).</summary>
    Lu,
    /// <summary>Lowercase letter (Ll).</summary>
    Ll,
    /// <summary>Titlecase letter (Lt).</summary>
    Lt,
    /// <summary>Modifier letter (Lm).</summary>
    Lm,
    /// <summary>Other letter (Lo).</summary>
    Lo,

    // ── Mark ────────────────────────────────────────────────────────────────────
    /// <summary>Non-spacing mark (Mn).</summary>
    Mn,
    /// <summary>Spacing combining mark (Mc).</summary>
    Mc,
    /// <summary>Enclosing mark (Me).</summary>
    Me,

    // ── Number ──────────────────────────────────────────────────────────────────
    /// <summary>Decimal digit number (Nd).</summary>
    Nd,
    /// <summary>Letter number (Nl).</summary>
    Nl,
    /// <summary>Other number (No).</summary>
    No,

    // ── Punctuation ─────────────────────────────────────────────────────────────
    /// <summary>Connector punctuation (Pc).</summary>
    Pc,
    /// <summary>Dash punctuation (Pd).</summary>
    Pd,
    /// <summary>Open punctuation (Ps).</summary>
    Ps,
    /// <summary>Close punctuation (Pe).</summary>
    Pe,
    /// <summary>Initial quote punctuation (Pi).</summary>
    Pi,
    /// <summary>Final quote punctuation (Pf).</summary>
    Pf,
    /// <summary>Other punctuation (Po).</summary>
    Po,

    // ── Symbol ──────────────────────────────────────────────────────────────────
    /// <summary>Math symbol (Sm).</summary>
    Sm,
    /// <summary>Currency symbol (Sc).</summary>
    Sc,
    /// <summary>Modifier symbol (Sk).</summary>
    Sk,
    /// <summary>Other symbol (So).</summary>
    So,

    // ── Separator ───────────────────────────────────────────────────────────────
    /// <summary>Space separator (Zs).</summary>
    Zs,
    /// <summary>Line separator (Zl).</summary>
    Zl,
    /// <summary>Paragraph separator (Zp).</summary>
    Zp,

    // ── Other ───────────────────────────────────────────────────────────────────
    /// <summary>Control character (Cc).</summary>
    Cc,
    /// <summary>Format character (Cf).</summary>
    Cf,
    /// <summary>Surrogate (Cs).</summary>
    Cs,
    /// <summary>Private-use character (Co).</summary>
    Co,
    /// <summary>Unassigned code point (Cn).</summary>
    Cn,
}
