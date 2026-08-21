namespace Unicode.NET.Internal;

/// <summary>Resolves a Unicode block name/alias (with compound and Is/In prefixes) against a block table.</summary>
internal static class BlockNameResolver
{
    /// <summary>Tries to resolve <paramref name="nameOrAlias"/> to a block range within <paramref name="blocks"/>.</summary>
    public static bool TryResolve(
        string nameOrAlias,
        IReadOnlyList<(CodePointRange Range, string Name)> blocks,
        out CodePointRange range)
    {
        range = default;

        if (string.IsNullOrWhiteSpace(nameOrAlias))
            return false;

        string query = nameOrAlias.Trim();
        if (StringNormalization.TrySplitPropertyPrefix(query, out var prefix, out var value))
        {
            if (prefix is not ("blk" or "block"))
                return false;

            query = value;
        }

        // Strip Is/In prefix (case-insensitive)
        if (query.Length > 2)
        {
            string prefix2 = query[..2].ToLowerInvariant();
            if (prefix2 is "is" or "in")
                query = query[2..];
        }

        string normalized = StringNormalization.NormalizePropertyName(query, stripSpaces: true);

        foreach (var (blockRange, name) in blocks)
        {
            string blockNorm = StringNormalization.NormalizePropertyName(name, stripSpaces: true);
            if (blockNorm == normalized)
            {
                range = blockRange;
                return true;
            }
        }

        return false;
    }
}
