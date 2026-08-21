namespace Unicode.NET.Internal;

/// <summary>Normalizes Unicode property/alias names for case- and separator-insensitive lookup.</summary>
internal static class StringNormalization
{
    public static bool TrySplitPropertyPrefix(
        string name,
        out string prefix,
        out string value)
    {
        int separator = name.IndexOf('=');
        if (separator < 0)
        {
            prefix = string.Empty;
            value = string.Empty;
            return false;
        }

        prefix = NormalizePropertyName(name[..separator]);
        value = name[(separator + 1)..].Trim();
        return true;
    }

    /// <summary>Lowercases and strips underscores/hyphens (and optionally spaces) for alias comparison.</summary>
    public static string NormalizePropertyName(string name, bool stripSpaces = false)
    {
        string normalized = name.Trim().ToLowerInvariant().Replace("_", "").Replace("-", "");
        return stripSpaces ? normalized.Replace(" ", "") : normalized;
    }
}
