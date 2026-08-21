using Unicode.NET.Internal;

namespace Unicode.NET;

/// <summary>
/// Facade for Unicode block data.
/// All methods accept an explicit <see cref="UnicodeVersion"/>; use
/// <see cref="UnicodeVersion.Current"/> to avoid specifying the version explicitly.
/// </summary>
public static class UnicodeBlocks
{
    /// <summary>
    /// Returns the <see cref="UnicodeBlock"/> that contains <paramref name="codePoint"/>,
    /// or <see langword="null"/> if the code point does not fall within any defined block.
    /// </summary>
    /// <param name="codePoint">The code point to look up.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>The containing block, or <see langword="null"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static UnicodeBlock? GetBlock(CodePoint codePoint, UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        var blocks = tables.GetBlocks();

        // Binary search: blocks are sorted by range start and are non-overlapping.
        int lo = 0, hi = blocks.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var (range, name) = blocks[mid];
            if (codePoint < range.Start)
                hi = mid - 1;
            else if (codePoint > range.End)
                lo = mid + 1;
            else
                return new UnicodeBlock(name, range);
        }
        return null;
    }

    /// <summary>
    /// Returns the <see cref="CodePointRange"/> of the block with the given name.
    /// </summary>
    /// <param name="blockName">
    /// The official Unicode block name, e.g. <c>"Basic Latin"</c>.
    /// Comparison is case-sensitive and uses the exact name from the Unicode standard.
    /// </param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>The <see cref="CodePointRange"/> of the named block.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="blockName"/> is not a known block name in this version.
    /// </exception>
    public static CodePointRange GetBlockRange(string blockName, UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        var blocks = tables.GetBlocks();

        foreach (var (range, name) in blocks)
        {
            if (string.Equals(name, blockName, StringComparison.Ordinal))
                return range;
        }

        throw new ArgumentException(
            $"Block \"{blockName}\" is not defined in Unicode {version}.", nameof(blockName));
    }

    /// <summary>
    /// Tries to resolve a block by name or alias.
    /// Accepts:
    /// <list type="bullet">
    ///   <item>Exact names: <c>Basic Latin</c></item>
    ///   <item>Normalized (no spaces/underscores/hyphens): <c>BasicLatin</c></item>
    ///   <item><c>Is</c>/<c>In</c> prefix stripped: <c>IsBasicLatin</c>, <c>InBasicLatin</c></item>
    ///   <item>Compound syntax: <c>blk=BasicLatin</c>, <c>Block=BasicLatin</c></item>
    ///   <item>Case-insensitive.</item>
    /// </list>
    /// </summary>
    /// <param name="nameOrAlias">Block name or alias to resolve.</param>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <param name="set">The resolved code-point set on success.</param>
    /// <returns><see langword="true"/> if resolved; <see langword="false"/> otherwise.</returns>
    public static bool TryResolveBlock(string nameOrAlias, UnicodeVersion version, out CodePointSet set)
    {
        set = CodePointSet.Empty;

        var tables = UnicodeVersion.GetTablesOrThrow(version);
        if (!BlockNameResolver.TryResolve(nameOrAlias, tables.GetBlocks(), out var range))
            return false;

        set = new CodePointSet([range]);
        return true;
    }

    /// <summary>
    /// Returns all Unicode blocks defined in the given version.
    /// </summary>
    /// <param name="version">The Unicode version whose data tables to query.</param>
    /// <returns>A read-only list of all blocks, ordered by range start.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="version"/> is not a registered Unicode version.
    /// </exception>
    public static IReadOnlyList<UnicodeBlock> GetAllBlocks(UnicodeVersion version)
    {
        var tables = UnicodeVersion.GetTablesOrThrow(version);
        var raw = tables.GetBlocks();
        var result = new UnicodeBlock[raw.Count];
        for (int i = 0; i < raw.Count; i++)
            result[i] = new UnicodeBlock(raw[i].Name, raw[i].Range);
        return result;
    }
}
