namespace Unicode.NET.Internal;

/// <summary>Computes Levenshtein edit distance between two strings.</summary>
internal static class LevenshteinDistance
{
    /// <summary>Compute edit distance between two strings (case-insensitive).</summary>
    public static int Compute(string source, string target)
    {
        string s = source.ToLowerInvariant();
        string t = target.ToLowerInvariant();

        int sLen = s.Length;
        int tLen = t.Length;

        if (sLen == 0) return tLen;
        if (tLen == 0) return sLen;

        // Two-row DP.
        int[] prev = new int[tLen + 1];
        int[] curr = new int[tLen + 1];

        for (int j = 0; j <= tLen; j++)
            prev[j] = j;

        for (int i = 1; i <= sLen; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= tLen; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }

        return prev[tLen];
    }
}
