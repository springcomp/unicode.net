using Unicode.NET.Internal;

namespace Unicode.NET;

/// <summary>
/// Facade for Unicode script property data.
/// All methods accept an explicit <see cref="UnicodeVersion"/>; use
/// <see cref="UnicodeVersion.Current"/> to avoid specifying the version explicitly.
/// </summary>
public static class UnicodeScripts
{
    /// <summary>
    /// Get code point set for a script.
    /// </summary>
    /// <param name="script">The script enum value.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>Code point set for the script.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="script"/> is unknown or not defined.
    /// </exception>
    public static CodePointSet GetScriptSet(Script script, UnicodeVersion version)
    {
        if (script == Script.Unknown)
            return CodePointSet.Empty;

        var scriptName = script.ToString();
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        return tables.GetScriptSet(scriptName);
    }

    /// <summary>
    /// Resolve script by name or alias (case-insensitive).
    /// </summary>
    /// <param name="nameOrAlias">Script name or ISO 15924 alias (e.g., "Latin", "Latn", "sc=Latn").</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <param name="script">Resolved script value.</param>
    /// <returns>True if resolved; false otherwise.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static bool TryResolveScript(string nameOrAlias, UnicodeVersion version, out Script script)
    {
        script = Script.Unknown;

        if (string.IsNullOrWhiteSpace(nameOrAlias))
            return false;

        var tables = UnicodeVersion.GetTablesOrThrow(version);
        var aliases = tables.GetScriptAliases();

        // Strip "sc=" prefix if present
        string query = nameOrAlias.Trim();
        if (query.StartsWith("sc=", StringComparison.OrdinalIgnoreCase))
            query = query.Substring(3);

        // Normalize: lowercase, strip underscores/hyphens
        string normalized = StringNormalization.NormalizePropertyName(query);

        if (!aliases.TryGetValue(normalized, out var canonicalName))
            return false;

        // Try parse enum
        if (Enum.TryParse<Script>(canonicalName, ignoreCase: false, out var parsed))
        {
            script = parsed;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get script set by name or alias. Throws if unresolved.
    /// </summary>
    /// <param name="nameOrAlias">Script name or ISO 15924 alias.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>Code point set for the script.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="nameOrAlias"/> cannot be resolved to a known script.
    /// </exception>
    public static CodePointSet GetScriptSet(string nameOrAlias, UnicodeVersion version)
    {
        if (TryResolveScript(nameOrAlias, version, out var script))
            return GetScriptSet(script, version);

        throw new ArgumentException(
            $"Script \"{nameOrAlias}\" is not defined in Unicode {version}.", nameof(nameOrAlias));
    }

    /// <summary>
    /// Get all scripts for a code point (includes extensions).
    /// </summary>
    /// <param name="codePoint">The code point to query.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>Array of script names assigned to this code point.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static Script[] GetScriptExtensions(CodePoint codePoint, UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        var extensions = tables.GetScriptExtensions();

        if (!extensions.TryGetValue(codePoint.Value, out var scriptNames))
            return [];

        var aliases = tables.GetScriptAliases();
        var result = new List<Script>();
        
        foreach (var name in scriptNames)
        {
            // Normalize alias to find canonical name
            string normalized = StringNormalization.NormalizePropertyName(name);
            if (aliases.TryGetValue(normalized, out var canonicalName))
            {
                if (Enum.TryParse<Script>(canonicalName, ignoreCase: false, out var s))
                    result.Add(s);
            }
        }

        return [.. result];
    }
}
