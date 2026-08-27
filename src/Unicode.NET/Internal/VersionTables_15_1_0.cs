using Unicode.NET;

namespace Unicode.NET.Generated;

/// <summary>Registry entry for Unicode 15.1.0 — delegates to generated static tables.</summary>
internal sealed class VersionTables_15_1_0 : IVersionTables
{
    public static readonly VersionTables_15_1_0 Instance = new();

    private VersionTables_15_1_0() { }

    public CaseMappingData GetCaseMappingData() => CaseMapping_15_1_0.Data;

    public CodePointSet GetCategorySet(GeneralCategory category) => category switch
    {
        GeneralCategory.Lu => GeneralCategories_15_1_0.Lu,
        GeneralCategory.Ll => GeneralCategories_15_1_0.Ll,
        GeneralCategory.Lt => GeneralCategories_15_1_0.Lt,
        GeneralCategory.Lm => GeneralCategories_15_1_0.Lm,
        GeneralCategory.Lo => GeneralCategories_15_1_0.Lo,
        GeneralCategory.Mn => GeneralCategories_15_1_0.Mn,
        GeneralCategory.Mc => GeneralCategories_15_1_0.Mc,
        GeneralCategory.Me => GeneralCategories_15_1_0.Me,
        GeneralCategory.Nd => GeneralCategories_15_1_0.Nd,
        GeneralCategory.Nl => GeneralCategories_15_1_0.Nl,
        GeneralCategory.No => GeneralCategories_15_1_0.No,
        GeneralCategory.Pc => GeneralCategories_15_1_0.Pc,
        GeneralCategory.Pd => GeneralCategories_15_1_0.Pd,
        GeneralCategory.Ps => GeneralCategories_15_1_0.Ps,
        GeneralCategory.Pe => GeneralCategories_15_1_0.Pe,
        GeneralCategory.Pi => GeneralCategories_15_1_0.Pi,
        GeneralCategory.Pf => GeneralCategories_15_1_0.Pf,
        GeneralCategory.Po => GeneralCategories_15_1_0.Po,
        GeneralCategory.Sm => GeneralCategories_15_1_0.Sm,
        GeneralCategory.Sc => GeneralCategories_15_1_0.Sc,
        GeneralCategory.Sk => GeneralCategories_15_1_0.Sk,
        GeneralCategory.So => GeneralCategories_15_1_0.So,
        GeneralCategory.Zs => GeneralCategories_15_1_0.Zs,
        GeneralCategory.Zl => GeneralCategories_15_1_0.Zl,
        GeneralCategory.Zp => GeneralCategories_15_1_0.Zp,
        GeneralCategory.Cc => GeneralCategories_15_1_0.Cc,
        GeneralCategory.Cf => GeneralCategories_15_1_0.Cf,
        GeneralCategory.Cs => GeneralCategories_15_1_0.Cs,
        GeneralCategory.Co => GeneralCategories_15_1_0.Co,
        GeneralCategory.Cn => GeneralCategories_15_1_0.Cn,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown general category.")
    };

    public IReadOnlyList<(CodePointRange Range, string Name)> GetBlocks() =>
        UnicodeBlocks_15_1_0.All;

    public IReadOnlyDictionary<string, string> GetGeneralCategoryAliases() =>
        PropertyAliases_15_1_0.GeneralCategoryAliases;

    public CodePointSet GetScriptSet(string scriptName)
    {
        if (!Scripts_15_1_0.ScriptRanges.TryGetValue(scriptName, out var ranges))
            throw new ArgumentOutOfRangeException(nameof(scriptName), scriptName, "Unknown script.");
        return new CodePointSet(ranges);
    }

    public IReadOnlyDictionary<int, string[]> GetScriptExtensions() =>
        Scripts_15_1_0.ScriptExtensions;

    public IReadOnlyDictionary<string, string> GetScriptAliases() =>
        PropertyAliases_15_1_0.ScriptAliases;

    public CodePointRange[] GetBinaryPropertyRanges(BinaryProperty property)
    {
        if (!BinaryProperties_15_1_0.Properties.TryGetValue(property, out var ranges))
            throw new ArgumentOutOfRangeException(nameof(property), property, "Unknown binary property.");
        return ranges;
    }
}
