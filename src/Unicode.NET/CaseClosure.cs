using System.Collections.Frozen;

namespace Unicode.NET;

/// <summary>
/// Computes the simple case closure of a <see cref="CodePointSet"/>.
/// </summary>
/// <remarks>
/// <para>
/// Simple case closure expands a set to include all code points whose simple fold
/// is the same as any member's simple fold — i.e., all code points that are
/// case-equivalent under simple folding.
/// </para>
/// <para>
/// Full case closure (sequence-aware, for 1:N mappings) is not supported and requires
/// <see cref="CaseFoldingMode.Full"/>, which throws <see cref="NotSupportedException"/>.
/// </para>
/// </remarks>
public static class CaseClosure
{
    /// <summary>
    /// Computes the case closure of <paramref name="input"/> under the given folding mode.
    /// </summary>
    /// <param name="input">The input set to close over.</param>
    /// <param name="mode">
    /// The folding mode. Only <see cref="CaseFoldingMode.Simple"/> is supported;
    /// <see cref="CaseFoldingMode.Full"/> throws <see cref="NotSupportedException"/>.
    /// </param>
    /// <param name="locale">
    /// The locale policy. Only <see cref="CaseFoldingLocale.Default"/> is supported.
    /// </param>
    /// <param name="version">
    /// The Unicode version. Defaults to <see cref="UnicodeVersion.Current"/>.
    /// </param>
    /// <returns>
    /// A <see cref="CodePointSet"/> that is a superset of <paramref name="input"/>
    /// and is idempotent: closing an already-closed set returns an equal set.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="mode"/> is <see cref="CaseFoldingMode.Full"/>,
    /// when <paramref name="locale"/> is <see cref="CaseFoldingLocale.Turkic"/>,
    /// or when <paramref name="version"/> is not registered.
    /// </exception>
    public static CodePointSet Closure(
        CodePointSet input,
        CaseFoldingMode mode = CaseFoldingMode.Simple,
        CaseFoldingLocale locale = CaseFoldingLocale.Default,
        UnicodeVersion? version = null)
    {
        ArgumentNullException.ThrowIfNull(input);

        var ver = version ?? UnicodeVersion.Current;
        UnicodeVersion.GetTablesOrThrow(ver);

        if (locale == CaseFoldingLocale.Turkic)
            throw new NotSupportedException(
                "CaseFoldingLocale.Turkic is reserved for future implementation.");

        if (mode != CaseFoldingMode.Simple)
            throw new NotSupportedException(
                $"CaseFoldingMode.{mode} is reserved for future implementation. " +
                "Only CaseFoldingMode.Simple is currently supported for case closure.");

        if (input.IsEmpty)
            return CodePointSet.Empty;

        var simpleMap = CaseFolding.GetSimpleMap(ver);
        var reverseMap = BuildReverseMap(simpleMap);

        // BFS: for each member, add its fold and all reverse-fold preimages
        var result = new HashSet<int>();
        var queue = new Queue<int>();

        foreach (var cp in input)
        {
            int v = cp.Value;
            if (result.Add(v))
                queue.Enqueue(v);
        }

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            // Forward: fold the current code point
            int folded = simpleMap.TryGetValue(current, out int f) ? f : current;
            if (result.Add(folded))
                queue.Enqueue(folded);

            // Reverse: all code points that fold to current
            if (reverseMap.TryGetValue(current, out var preimages))
            {
                foreach (int pre in preimages)
                {
                    if (result.Add(pre))
                        queue.Enqueue(pre);
                }
            }

            // Reverse: all code points that fold to folded (if different from current)
            if (folded != current && reverseMap.TryGetValue(folded, out var preimages2))
            {
                foreach (int pre in preimages2)
                {
                    if (result.Add(pre))
                        queue.Enqueue(pre);
                }
            }
        }

        var builder = new CodePointSetBuilder();
        foreach (int v in result)
            builder.Add(CodePoint.Create(v));
        return builder.Build();
    }

    private static Dictionary<int, List<int>> BuildReverseMap(
        FrozenDictionary<int, int> simpleMap)
    {
        var rev = new Dictionary<int, List<int>>(simpleMap.Count);
        foreach (var (source, target) in simpleMap)
        {
            if (!rev.TryGetValue(target, out var list))
                rev[target] = list = new List<int>();
            list.Add(source);
        }
        return rev;
    }
}
