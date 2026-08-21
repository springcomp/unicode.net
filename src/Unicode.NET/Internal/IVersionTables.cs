using System.Collections.Generic;

namespace Unicode.NET.Generated;

/// <summary>Per-version table contract bridging the version registry to generated static data.</summary>
internal interface IVersionTables
{
    /// <summary>Returns the <see cref="Unicode.NET.CodePointSet"/> for a given general category.</summary>
    Unicode.NET.CodePointSet GetCategorySet(Unicode.NET.GeneralCategory category);

    /// <summary>All (Range, Name) block pairs for this version, ordered by range start.</summary>
    IReadOnlyList<(Unicode.NET.CodePointRange Range, string Name)> GetBlocks();

    /// <summary>
    /// Normalized-alias to canonical-name map for general categories.
    /// Keys are lowercase with underscores/hyphens stripped.
    /// </summary>
    IReadOnlyDictionary<string, string> GetGeneralCategoryAliases();

    /// <summary>Returns the <see cref="Unicode.NET.CodePointSet"/> for a given script.</summary>
    Unicode.NET.CodePointSet GetScriptSet(string scriptName);

    /// <summary>All script extension data: maps code point -> script names.</summary>
    IReadOnlyDictionary<int, string[]> GetScriptExtensions();

    /// <summary>
    /// Normalized-alias to canonical-name map for scripts.
    /// Keys are lowercase with underscores/hyphens stripped.
    /// </summary>
    IReadOnlyDictionary<string, string> GetScriptAliases();

    /// <summary>Returns the code-point ranges for a binary property.</summary>
    CodePointRange[] GetBinaryPropertyRanges(BinaryProperty property);
}
