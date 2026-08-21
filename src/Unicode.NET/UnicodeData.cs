using Unicode.NET.Generated;
using Unicode.NET.Internal;

namespace Unicode.NET;

/// <summary>
/// Facade for Unicode character property data (general categories).
/// All methods accept an explicit <see cref="UnicodeVersion"/>; use
/// <see cref="UnicodeVersion.Current"/> to avoid specifying the version explicitly.
/// </summary>
public static class UnicodeData
{
    // ── Canonical long name → GeneralCategory enum ───────────────────────────

    private static readonly IReadOnlyDictionary<string, GeneralCategory> CanonicalToEnum =
        new Dictionary<string, GeneralCategory>(StringComparer.OrdinalIgnoreCase)
        {
            { "Uppercase_Letter",        GeneralCategory.Lu },
            { "Lowercase_Letter",        GeneralCategory.Ll },
            { "Titlecase_Letter",        GeneralCategory.Lt },
            { "Modifier_Letter",         GeneralCategory.Lm },
            { "Other_Letter",            GeneralCategory.Lo },
            { "Nonspacing_Mark",         GeneralCategory.Mn },
            { "Spacing_Mark",            GeneralCategory.Mc },
            { "Enclosing_Mark",          GeneralCategory.Me },
            { "Decimal_Number",          GeneralCategory.Nd },
            { "Letter_Number",           GeneralCategory.Nl },
            { "Other_Number",            GeneralCategory.No },
            { "Connector_Punctuation",   GeneralCategory.Pc },
            { "Dash_Punctuation",        GeneralCategory.Pd },
            { "Open_Punctuation",        GeneralCategory.Ps },
            { "Close_Punctuation",       GeneralCategory.Pe },
            { "Initial_Punctuation",     GeneralCategory.Pi },
            { "Final_Punctuation",       GeneralCategory.Pf },
            { "Other_Punctuation",       GeneralCategory.Po },
            { "Math_Symbol",             GeneralCategory.Sm },
            { "Currency_Symbol",         GeneralCategory.Sc },
            { "Modifier_Symbol",         GeneralCategory.Sk },
            { "Other_Symbol",            GeneralCategory.So },
            { "Space_Separator",         GeneralCategory.Zs },
            { "Line_Separator",          GeneralCategory.Zl },
            { "Paragraph_Separator",     GeneralCategory.Zp },
            { "Control",                 GeneralCategory.Cc },
            { "Format",                  GeneralCategory.Cf },
            { "Surrogate",               GeneralCategory.Cs },
            { "Private_Use",             GeneralCategory.Co },
            { "Unassigned",              GeneralCategory.Cn },
        };

    // Major-category canonical names that expand to a union of sub-categories.
    private static readonly IReadOnlyDictionary<string, GeneralCategory[]> MajorCategories =
        new Dictionary<string, GeneralCategory[]>(StringComparer.OrdinalIgnoreCase)
        {
            // L — Letter
            { "Letter",        new[] { GeneralCategory.Lu, GeneralCategory.Ll, GeneralCategory.Lt, GeneralCategory.Lm, GeneralCategory.Lo } },
            // LC — Cased_Letter
            { "Cased_Letter",  new[] { GeneralCategory.Lu, GeneralCategory.Ll, GeneralCategory.Lt } },
            // M — Mark
            { "Mark",          new[] { GeneralCategory.Mn, GeneralCategory.Mc, GeneralCategory.Me } },
            // N — Number
            { "Number",        new[] { GeneralCategory.Nd, GeneralCategory.Nl, GeneralCategory.No } },
            // P — Punctuation
            { "Punctuation",   new[] { GeneralCategory.Pc, GeneralCategory.Pd, GeneralCategory.Ps, GeneralCategory.Pe, GeneralCategory.Pi, GeneralCategory.Pf, GeneralCategory.Po } },
            // S — Symbol
            { "Symbol",        new[] { GeneralCategory.Sm, GeneralCategory.Sc, GeneralCategory.Sk, GeneralCategory.So } },
            // Z — Separator
            { "Separator",     new[] { GeneralCategory.Zs, GeneralCategory.Zl, GeneralCategory.Zp } },
            // C — Other
            { "Other",         new[] { GeneralCategory.Cc, GeneralCategory.Cf, GeneralCategory.Cs, GeneralCategory.Co, GeneralCategory.Cn } },
        };

    /// <summary>
    /// Returns the general category of <paramref name="codePoint"/> in the given Unicode version.
    /// </summary>
    /// <param name="codePoint">The code point to look up.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>The <see cref="GeneralCategory"/> of the code point.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static GeneralCategory GetGeneralCategory(CodePoint codePoint, UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        foreach (GeneralCategory cat in Enum.GetValues<GeneralCategory>())
        {
            if (tables.GetCategorySet(cat).Contains(codePoint))
                return cat;
        }
        // Should never happen: Cn covers all unassigned code points.
        throw new InvalidOperationException(
            $"Code point U+{codePoint.Value:X4} has no general category in Unicode {version}.");
    }

    /// <summary>
    /// Returns the <see cref="CodePointSet"/> of all code points that belong to
    /// <paramref name="category"/> in the given Unicode version.
    /// </summary>
    /// <param name="category">The general category to look up.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>An immutable <see cref="CodePointSet"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static CodePointSet GetCategorySet(GeneralCategory category, UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        return tables.GetCategorySet(category);
    }

    // ── Alias-based API ────────────────────────────────────────────────────

    /// <summary>
    /// Resolves a general-category name or alias to a <see cref="GeneralCategory"/> enum value.
    /// Accepts:
    /// <list type="bullet">
    ///   <item>Short aliases: <c>Lu</c>, <c>Nd</c></item>
    ///   <item>Long names: <c>Uppercase_Letter</c>, <c>Decimal_Number</c></item>
    ///   <item>Compound syntax: <c>gc=Lu</c>, <c>General_Category=Uppercase_Letter</c></item>
    ///   <item>Case-insensitive, underscores/hyphens ignored.</item>
    /// </list>
    /// Returns <see langword="false"/> for major-category aliases that expand to a union
    /// (e.g., <c>L</c> / <c>Letter</c>): use <see cref="GetCategorySet(string, UnicodeVersion)"/> instead.
    /// </summary>
    public static bool TryResolveCategory(
        string nameOrAlias,
        UnicodeVersion version,
        out GeneralCategory category)
    {
        category = default;
        string input = StripPropertyPrefix(nameOrAlias);
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        var aliases = tables.GetGeneralCategoryAliases();

        // 1. Try exact match against enum names (Lu, Ll, …)
        if (Enum.TryParse<GeneralCategory>(input, ignoreCase: true, out category))
            return true;

        // 2. Normalize and look up alias dictionary
        string normalized = StringNormalization.NormalizePropertyName(input);
        if (aliases.TryGetValue(normalized, out string? canonical) && canonical is not null)
        {
            // Skip major-category aliases (they expand to a union, not a single enum value)
            if (MajorCategories.ContainsKey(canonical))
                return false;

            if (CanonicalToEnum.TryGetValue(canonical, out category))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the <see cref="CodePointSet"/> for a general-category name or alias.
    /// Supports both single categories and major-category unions (e.g., <c>L</c> → all Letter sub-categories).
    /// </summary>
    /// <exception cref="NotSupportedException">Unknown Unicode version.</exception>
    /// <exception cref="ArgumentException">Unresolvable alias.</exception>
    public static CodePointSet GetCategorySet(string nameOrAlias, UnicodeVersion version)
    {
        if (TryResolveCategorySet(nameOrAlias, version, out var set))
            return set;

        throw new ArgumentException(
            $"'{nameOrAlias}' is not a recognized Unicode general category name or alias.",
            nameof(nameOrAlias));
    }

    /// <summary>
    /// Tries to resolve a general-category name or alias to a code-point set.
    /// </summary>
    /// <exception cref="NotSupportedException">Unknown Unicode version.</exception>
    public static bool TryResolveCategorySet(
        string nameOrAlias,
        UnicodeVersion version,
        out CodePointSet set)
    {
        set = CodePointSet.Empty;
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        string input = StripPropertyPrefix(nameOrAlias);
        var aliases = tables.GetGeneralCategoryAliases();

        if (TryResolveCategory(nameOrAlias, version, out var single))
        {
            set = tables.GetCategorySet(single);
            return true;
        }

        string normalized = StringNormalization.NormalizePropertyName(input);
        if (aliases.TryGetValue(normalized, out string? canonical) && canonical is not null
            && MajorCategories.TryGetValue(canonical, out var members))
        {
            foreach (var member in members)
                set = set.Union(tables.GetCategorySet(member));
            return true;
        }

        return false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Strips a property prefix such as <c>gc=</c> or <c>General_Category=</c>.
    /// Returns the value part only.
    /// </summary>
    private static string StripPropertyPrefix(string input)
    {
        return StringNormalization.TrySplitPropertyPrefix(input, out _, out var value)
            ? value
            : input.Trim();
    }
}
