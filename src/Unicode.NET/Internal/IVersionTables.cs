using System.Collections.Frozen;
using System.Collections.Generic;

namespace Unicode.NET.Generated;

/// <summary>Per-version table contract bridging the version registry to generated static data.</summary>
internal interface IVersionTables
{
    CaseMappingData GetCaseMappingData();
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

internal sealed class CaseMappingData
{
    public CaseMappingData(
        FrozenDictionary<int, int> simpleLowercaseMap,
        FrozenDictionary<int, int> simpleUppercaseMap,
        FrozenDictionary<int, int[]> fullLowercaseMap,
        FrozenDictionary<int, int[]> fullUppercaseMap,
        IReadOnlyList<CaseMappingContextRule> contextualRules,
        IReadOnlyList<CodePointRange> cased,
        IReadOnlyList<CodePointRange> caseIgnorable)
    {
        SimpleLowercaseMap = simpleLowercaseMap;
        SimpleUppercaseMap = simpleUppercaseMap;
        FullLowercaseMap = fullLowercaseMap;
        FullUppercaseMap = fullUppercaseMap;
        ContextualRules = contextualRules;
        Cased = cased;
        CaseIgnorable = caseIgnorable;
    }

    internal FrozenDictionary<int, int> SimpleLowercaseMap { get; }
    internal FrozenDictionary<int, int> SimpleUppercaseMap { get; }
    internal FrozenDictionary<int, int[]> FullLowercaseMap { get; }
    internal FrozenDictionary<int, int[]> FullUppercaseMap { get; }
    internal IReadOnlyList<CaseMappingContextRule> ContextualRules { get; }
    internal IReadOnlyList<CodePointRange> Cased { get; }
    internal IReadOnlyList<CodePointRange> CaseIgnorable { get; }
}

internal sealed record CaseMappingContextRule(
    int Source,
    string Predicate,
    int[] LowercaseMapping,
    int[] UppercaseMapping);
