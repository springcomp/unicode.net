using Unicode.NET;

namespace Unicode.NET.Generated;

/// <summary>Registry entry for Unicode 16.0.0 — delegates to generated static tables.</summary>
internal sealed class VersionTables_16_0_0 : IVersionTables
{
    public static readonly VersionTables_16_0_0 Instance = new();

    private VersionTables_16_0_0() { }

    private static readonly CaseFoldingData CaseFolding =
        new(CaseFolding_16_0_0.SimpleMap, CaseFolding_16_0_0.FullMap);

    public CaseMappingData GetCaseMappingData() => CaseMapping_16_0_0.Data;

    public CaseFoldingData GetCaseFoldingData() => CaseFolding;

    public CodePointSet GetCategorySet(GeneralCategory category) => category switch
    {
        GeneralCategory.Lu => GeneralCategories_16_0_0.Lu,
        GeneralCategory.Ll => GeneralCategories_16_0_0.Ll,
        GeneralCategory.Lt => GeneralCategories_16_0_0.Lt,
        GeneralCategory.Lm => GeneralCategories_16_0_0.Lm,
        GeneralCategory.Lo => GeneralCategories_16_0_0.Lo,
        GeneralCategory.Mn => GeneralCategories_16_0_0.Mn,
        GeneralCategory.Mc => GeneralCategories_16_0_0.Mc,
        GeneralCategory.Me => GeneralCategories_16_0_0.Me,
        GeneralCategory.Nd => GeneralCategories_16_0_0.Nd,
        GeneralCategory.Nl => GeneralCategories_16_0_0.Nl,
        GeneralCategory.No => GeneralCategories_16_0_0.No,
        GeneralCategory.Pc => GeneralCategories_16_0_0.Pc,
        GeneralCategory.Pd => GeneralCategories_16_0_0.Pd,
        GeneralCategory.Ps => GeneralCategories_16_0_0.Ps,
        GeneralCategory.Pe => GeneralCategories_16_0_0.Pe,
        GeneralCategory.Pi => GeneralCategories_16_0_0.Pi,
        GeneralCategory.Pf => GeneralCategories_16_0_0.Pf,
        GeneralCategory.Po => GeneralCategories_16_0_0.Po,
        GeneralCategory.Sm => GeneralCategories_16_0_0.Sm,
        GeneralCategory.Sc => GeneralCategories_16_0_0.Sc,
        GeneralCategory.Sk => GeneralCategories_16_0_0.Sk,
        GeneralCategory.So => GeneralCategories_16_0_0.So,
        GeneralCategory.Zs => GeneralCategories_16_0_0.Zs,
        GeneralCategory.Zl => GeneralCategories_16_0_0.Zl,
        GeneralCategory.Zp => GeneralCategories_16_0_0.Zp,
        GeneralCategory.Cc => GeneralCategories_16_0_0.Cc,
        GeneralCategory.Cf => GeneralCategories_16_0_0.Cf,
        GeneralCategory.Cs => GeneralCategories_16_0_0.Cs,
        GeneralCategory.Co => GeneralCategories_16_0_0.Co,
        GeneralCategory.Cn => GeneralCategories_16_0_0.Cn,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown general category.")
    };

    public IReadOnlyList<(CodePointRange Range, string Name)> GetBlocks() =>
        UnicodeBlocks_16_0_0.All;

    public IReadOnlyDictionary<string, string> GetGeneralCategoryAliases() =>
        PropertyAliases_16_0_0.GeneralCategoryAliases;

    public CodePointSet GetScriptSet(string scriptName) =>
        throw new NotImplementedException("Script data not yet generated for Unicode 16.0.0.");

    public IReadOnlyDictionary<int, string[]> GetScriptExtensions() =>
        throw new NotImplementedException("Script extensions not yet generated for Unicode 16.0.0.");

    public IReadOnlyDictionary<string, string> GetScriptAliases() =>
        throw new NotImplementedException("Script aliases not yet generated for Unicode 16.0.0.");

    public CodePointRange[] GetBinaryPropertyRanges(BinaryProperty property) =>
        throw new NotImplementedException("Binary property data not yet generated for Unicode 16.0.0.");
}
