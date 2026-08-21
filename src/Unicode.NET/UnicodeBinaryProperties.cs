using Unicode.NET.Internal;

namespace Unicode.NET;

/// <summary>
/// Facade for Unicode binary property data.
/// All methods accept an explicit <see cref="UnicodeVersion"/>; use
/// <see cref="UnicodeVersion.Current"/> to avoid specifying the version explicitly.
/// </summary>
public static class UnicodeBinaryProperties
{
    /// <summary>
    /// Get code-point set for a binary property.
    /// </summary>
    /// <param name="property">The binary property enum value.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>Code-point set for the property.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="property"/> is not defined.
    /// </exception>
    public static CodePointSet GetPropertySet(BinaryProperty property, UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        var ranges = tables.GetBinaryPropertyRanges(property);
        return new CodePointSet(ranges);
    }

    /// <summary>
    /// Resolve binary property by name or alias (case-insensitive, underscore/hyphen tolerant).
    /// </summary>
    /// <param name="nameOrAlias">Property name (e.g., "Alphabetic", "White_Space", "whitespace", "white-space").</param>
    /// <param name="property">Resolved property value on success.</param>
    /// <returns>True if resolved; false otherwise.</returns>
    public static bool TryResolveProperty(string nameOrAlias, out BinaryProperty property)
    {
        property = default;

        if (string.IsNullOrWhiteSpace(nameOrAlias))
            return false;

        string normalized = StringNormalization.NormalizePropertyName(nameOrAlias);

        foreach (BinaryProperty candidate in Enum.GetValues<BinaryProperty>())
        {
            string candidateNorm = StringNormalization.NormalizePropertyName(candidate.ToString());
            if (candidateNorm == normalized)
            {
                property = candidate;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get property set by name or alias. Throws if unresolved.
    /// </summary>
    /// <param name="nameOrAlias">Property name or alias.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>Code-point set for the property.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="nameOrAlias"/> cannot be resolved to a known binary property.
    /// </exception>
    public static CodePointSet GetPropertySet(string nameOrAlias, UnicodeVersion version)
    {
        if (TryResolveProperty(nameOrAlias, out var property))
            return GetPropertySet(property, version);

        throw new ArgumentException(
            $"Binary property \"{nameOrAlias}\" is not defined.", nameof(nameOrAlias));
    }
}
