namespace Unicode.NET;

/// <summary>
/// Mutable builder that produces an immutable, canonical <see cref="CodePointSet"/>.
/// May be reused after <see cref="Build"/>; already-built sets are not affected by later mutations.
/// </summary>
public sealed class CodePointSetBuilder
{
    private readonly List<CodePointRange> _pending = new();

    public void Add(CodePoint value) => _pending.Add(new CodePointRange(value));

    public void Add(CodePointRange range) => _pending.Add(range);

    public void AddRange(IEnumerable<CodePointRange> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);
        foreach (var r in ranges)
            _pending.Add(r);
    }

    /// <summary>
    /// Sorts and coalesces all added ranges into an immutable <see cref="CodePointSet"/>.
    /// Returns <see cref="CodePointSet.Empty"/> when nothing was added.
    /// </summary>
    public CodePointSet Build()
    {
        if (_pending.Count == 0)
            return CodePointSet.Empty;

        // Sort by start, break ties by end (wider first is fine — coalescing handles it).
        var sorted = _pending
            .OrderBy(r => r.Start.Value)
            .ThenBy(r => r.End.Value)
            .ToList();

        var coalesced = new List<CodePointRange>(sorted.Count);
        var current = sorted[0];
        for (int i = 1; i < sorted.Count; i++)
        {
            var next = sorted[i];
            if (current.Overlaps(next) || current.IsAdjacentTo(next))
            {
                int newEnd = Math.Max(current.End.Value, next.End.Value);
                current = CodePointRange.Create(current.Start.Value, newEnd);
            }
            else
            {
                coalesced.Add(current);
                current = next;
            }
        }
        coalesced.Add(current);

        return new CodePointSet(coalesced.ToArray());
    }
}
