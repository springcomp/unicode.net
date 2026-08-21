namespace Unicode.NET.Internal;

/// <summary>Gathers all known Unicode property names/aliases for use in typo suggestions.</summary>
internal static class PropertyNameCatalog
{
    /// <summary>Enumerates every known category, block, script, and binary-property name for <paramref name="version"/>.</summary>
    public static IEnumerable<string> GatherAllNames(UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);

        // General category short names (Lu, Ll, …)
        foreach (GeneralCategory cat in Enum.GetValues<GeneralCategory>())
            yield return cat.ToString();

        // General category long names + aliases
        foreach (var alias in tables.GetGeneralCategoryAliases().Values.Distinct())
            yield return alias;

        // Block names
        foreach (var block in UnicodeBlocks.GetAllBlocks(version))
        {
            yield return block.Name;
            // Also yield normalised (no spaces) form
            string noSpaces = block.Name.Replace(" ", "").Replace("-", "");
            if (noSpaces != block.Name)
                yield return noSpaces;
        }

        // Script names + aliases
        foreach (Script sc in Enum.GetValues<Script>())
            yield return sc.ToString();

        foreach (var alias in tables.GetScriptAliases().Values.Distinct())
            yield return alias;

        // Binary property names
        foreach (BinaryProperty bp in Enum.GetValues<BinaryProperty>())
            yield return bp.ToString();
    }
}
