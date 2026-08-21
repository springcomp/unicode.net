using System.Collections;

namespace Unicode.NET;

/// <summary>
/// Immutable set of Unicode code points backed by a sorted, disjoint, coalesced array of
/// <see cref="CodePointRange"/> values. All set-algebra operations return canonical sets.
/// </summary>
public sealed class CodePointSet : IReadOnlyCollection<CodePoint>, IEquatable<CodePointSet>
{
    private static readonly CodePointRange[] s_emptyRanges = Array.Empty<CodePointRange>();
    private static readonly CodePointRange[] s_allRanges =
        [CodePointRange.Create(CodePoint.MinValue, CodePoint.MaxValue)];

    /// <summary>Empty set containing no code points.</summary>
    public static readonly CodePointSet Empty = new(s_emptyRanges);

    /// <summary>Full set containing every code point <c>U+000000..U+10FFFF</c>.</summary>
    public static readonly CodePointSet All = new(s_allRanges);

    // Sorted, disjoint, coalesced ranges — never null.
    private readonly CodePointRange[] _ranges;

    // Cached count; -1 = not yet computed.
    private int _count = -1;

    internal CodePointSet(CodePointRange[] ranges)
    {
        _ranges = ranges;
    }

    // ── Public surface ──────────────────────────────────────────────────────

    public int RangeCount => _ranges.Length;
    public bool IsEmpty => _ranges.Length == 0;

    /// <summary>Canonical ranges in ascending order.</summary>
    public IEnumerable<CodePointRange> Ranges => _ranges;

    /// <summary>Membership test in <c>O(log r)</c> via binary search.</summary>
    public bool Contains(CodePoint value)
    {
        int lo = 0, hi = _ranges.Length - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var r = _ranges[mid];
            if (value < r.Start)       hi = mid - 1;
            else if (value > r.End)    lo = mid + 1;
            else                       return true;
        }
        return false;
    }

    // ── IReadOnlyCollection<CodePoint> ──────────────────────────────────────

    /// <summary>Total number of code points in the set. Computed lazily.</summary>
    public int Count
    {
        get
        {
            if (_count >= 0) return _count;
            long total = 0;
            foreach (var r in _ranges)
                total += (long)r.End.Value - r.Start.Value + 1;
            // Clamp to int.MaxValue for the rare case the caller uses the value.
            _count = total > int.MaxValue ? int.MaxValue : (int)total;
            return _count;
        }
    }

    /// <summary>Lazy ascending enumeration — never eagerly materialises all code points.</summary>
    public IEnumerator<CodePoint> GetEnumerator()
    {
        foreach (var range in _ranges)
            for (int v = range.Start.Value; v <= range.End.Value; v++)
                yield return CodePoint.Create(v);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ── Set algebra ─────────────────────────────────────────────────────────

    public CodePointSet Union(CodePointSet other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;

        var result = new List<CodePointRange>(_ranges.Length + other._ranges.Length);
        int i = 0, j = 0;
        while (i < _ranges.Length && j < other._ranges.Length)
        {
            CodePointRange a = _ranges[i], b = other._ranges[j];
            CodePointRange pick = a.Start <= b.Start ? a : b;
            if (a.Start <= b.Start) i++; else j++;
            MergeInto(result, pick);
        }
        while (i < _ranges.Length) { MergeInto(result, _ranges[i++]); }
        while (j < other._ranges.Length) { MergeInto(result, other._ranges[j++]); }
        return new CodePointSet(result.ToArray());
    }

    public CodePointSet Intersect(CodePointSet other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (IsEmpty || other.IsEmpty) return Empty;

        var result = new List<CodePointRange>();
        int i = 0, j = 0;
        while (i < _ranges.Length && j < other._ranges.Length)
        {
            var a = _ranges[i];
            var b = other._ranges[j];
            int lo = Math.Max(a.Start.Value, b.Start.Value);
            int hi = Math.Min(a.End.Value, b.End.Value);
            if (lo <= hi)
                result.Add(CodePointRange.Create(lo, hi));
            if (a.End <= b.End) i++; else j++;
        }
        return result.Count == 0 ? Empty : new CodePointSet(result.ToArray());
    }

    public CodePointSet Subtract(CodePointSet other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (IsEmpty || other.IsEmpty) return this;

        var result = new List<CodePointRange>();
        int j = 0;
        foreach (var a in _ranges)
        {
            int cur = a.Start.Value;
            // Advance j past ranges that end before cur
            while (j < other._ranges.Length && other._ranges[j].End.Value < cur)
                j++;
            int k = j;
            while (k < other._ranges.Length && other._ranges[k].Start.Value <= a.End.Value)
            {
                var b = other._ranges[k];
                int bStart = b.Start.Value;
                int bEnd = b.End.Value;
                if (cur < bStart)
                    result.Add(CodePointRange.Create(cur, bStart - 1));
                cur = bEnd + 1;
                if (cur > a.End.Value) break;
                k++;
            }
            if (cur <= a.End.Value)
                result.Add(CodePointRange.Create(cur, a.End.Value));
        }
        return result.Count == 0 ? Empty : new CodePointSet(result.ToArray());
    }

    public CodePointSet Complement() => All.Subtract(this);

    // ── Equality / hashing ──────────────────────────────────────────────────

    public bool Equals(CodePointSet? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_ranges.Length != other._ranges.Length) return false;
        for (int i = 0; i < _ranges.Length; i++)
            if (_ranges[i] != other._ranges[i]) return false;
        return true;
    }

    public override bool Equals(object? obj) => obj is CodePointSet other && Equals(other);

    public override int GetHashCode()
    {
        var hc = new HashCode();
        foreach (var r in _ranges)
            hc.Add(r);
        return hc.ToHashCode();
    }

    public static bool operator ==(CodePointSet? left, CodePointSet? right)
        => left is null ? right is null : left.Equals(right);
    public static bool operator !=(CodePointSet? left, CodePointSet? right) => !(left == right);

    public override string ToString() => $"CodePointSet({_ranges.Length} range(s))";

    // ── Private helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Appends <paramref name="range"/> to <paramref name="list"/>, merging with the last
    /// element when they overlap or are adjacent.
    /// </summary>
    private static void MergeInto(List<CodePointRange> list, CodePointRange range)
    {
        if (list.Count == 0)
        {
            list.Add(range);
            return;
        }
        var last = list[^1];
        if (last.Overlaps(range) || last.IsAdjacentTo(range))
        {
            int newStart = Math.Min(last.Start.Value, range.Start.Value);
            int newEnd   = Math.Max(last.End.Value,   range.End.Value);
            list[^1] = CodePointRange.Create(newStart, newEnd);
        }
        else
        {
            list.Add(range);
        }
    }
}
