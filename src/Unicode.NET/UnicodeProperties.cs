using Unicode.NET.Internal;

namespace Unicode.NET;

/// <summary>
/// Unified property resolution facade for categories, blocks, scripts, and binary properties.
/// Single entry point for resolving any Unicode property by name or alias.
/// </summary>
public static class UnicodeProperties
{
    /// <summary>
    /// Resolve any Unicode property by name or alias.
    /// Resolution order: general categories, blocks, scripts, binary properties.
    /// Supports compound syntax: <c>gc=Lu</c>, <c>Script=Greek</c>, <c>blk=BasicLatin</c>.
    /// </summary>
    /// <param name="name">Property name or alias.</param>
    /// <param name="version">The Unicode version to query.</param>
    /// <param name="set">Resolved code-point set on success.</param>
    /// <returns><see langword="true"/> if resolved; <see langword="false"/> otherwise.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static bool TryResolve(string name, UnicodeVersion version, out CodePointSet set)
    {
        set = CodePointSet.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return false;

        // If compound syntax, route by property name prefix.
        if (StringNormalization.TrySplitPropertyPrefix(name, out var prefix, out var value))
        {
            return prefix switch
            {
                "gc" or "generalcategory" => TryResolveCategory(value, version, out set),
                "sc" or "script" => TryResolveScript(value, version, out set),
                "blk" or "block" => UnicodeBlocks.TryResolveBlock(value, version, out set),
                _ => false,
            };
        }

        // No prefix: try each kind in order.
        return TryResolveCategory(name, version, out set)
            || UnicodeBlocks.TryResolveBlock(name, version, out set)
            || TryResolveScript(name, version, out set)
            || TryResolveBinaryProperty(name, version, out set);
    }

    /// <summary>
    /// Resolve property or throw <see cref="UnknownPropertyException"/> with suggestions.
    /// </summary>
    /// <param name="name">Property name or alias.</param>
    /// <param name="version">The Unicode version to query.</param>
    /// <returns>Resolved code-point set.</returns>
    /// <exception cref="UnknownPropertyException">
    /// Thrown when <paramref name="name"/> cannot be resolved to any known property.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static CodePointSet Resolve(string name, UnicodeVersion version)
    {
        if (TryResolve(name, version, out var set))
            return set;

        var suggestions = Suggest(name, version);
        throw new UnknownPropertyException(name, suggestions);
    }

    /// <summary>
    /// Suggest property names when resolution fails.
    /// Returns up to <paramref name="maxSuggestions"/> candidates sorted by Levenshtein distance.
    /// </summary>
    /// <param name="query">The unrecognised name to find suggestions for.</param>
    /// <param name="version">The Unicode version to query for available names.</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return.</param>
    /// <returns>Closest-matching property names, best match first.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static IEnumerable<string> Suggest(string query, UnicodeVersion version, int maxSuggestions = 3)
    {
        var candidates = PropertyNameCatalog.GatherAllNames(version);
        return candidates
            .Select(name => (name, distance: LevenshteinDistance.Compute(query, name)))
            .OrderBy(x => x.distance)
            .Take(maxSuggestions)
            .Select(x => x.name);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool TryResolveCategory(string name, UnicodeVersion version, out CodePointSet set)
    {
        return UnicodeData.TryResolveCategorySet(name, version, out set);
    }

    private static bool TryResolveScript(string name, UnicodeVersion version, out CodePointSet set)
    {
        set = CodePointSet.Empty;
        if (UnicodeScripts.TryResolveScript(name, version, out var script))
        {
            set = UnicodeScripts.GetScriptSet(script, version);
            return true;
        }
        return false;
    }

    private static bool TryResolveBinaryProperty(string name, UnicodeVersion version, out CodePointSet set)
    {
        set = CodePointSet.Empty;
        if (UnicodeBinaryProperties.TryResolveProperty(name, out var prop))
        {
            set = UnicodeBinaryProperties.GetPropertySet(prop, version);
            return true;
        }
        return false;
    }
}
