namespace Unicode.NET;

/// <summary>
/// An immutable inclusive code-point range <c>[Start, End]</c> where
/// <c>U+000000 &lt;= Start &lt;= End &lt;= U+10FFFF</c>.
/// </summary>
public readonly struct CodePointRange : IEquatable<CodePointRange>
{
    /// <summary>The first code point in the range, inclusive.</summary>
    public CodePoint Start { get; }
    /// <summary>The last code point in the range, inclusive.</summary>
    public CodePoint End { get; }

    /// <summary>Creates a range spanning <paramref name="start"/> to <paramref name="end"/> inclusive.</summary>
    public CodePointRange(CodePoint start, CodePoint end)
    {
        if (end < start)
            throw new ArgumentException($"End ({end}) must be >= Start ({start}).");
        Start = start;
        End = end;
    }

    /// <summary>Creates a singleton range containing exactly one code point.</summary>
    public CodePointRange(CodePoint single) : this(single, single) { }

    /// <summary>Convenience factory from raw int values.</summary>
    public static CodePointRange Create(int start, int end)
        => new(CodePoint.Create(start), CodePoint.Create(end));

    /// <summary>True if <paramref name="value"/> falls within this range.</summary>
    public bool Contains(CodePoint value) => value >= Start && value <= End;

    /// <summary>True when the inclusive intervals share at least one code point.</summary>
    public bool Overlaps(CodePointRange other)
        => Start <= other.End && other.Start <= End;

    /// <summary>
    /// True when one range ends immediately before the other begins.
    /// Overflow-safe: <c>U+10FFFF</c> has no adjacent successor.
    /// </summary>
    public bool IsAdjacentTo(CodePointRange other)
    {
        // this ends one before other starts
        if (End.Value < CodePoint.MaxValue && End.Value + 1 == other.Start.Value)
            return true;
        // other ends one before this starts
        if (other.End.Value < CodePoint.MaxValue && other.End.Value + 1 == Start.Value)
            return true;
        return false;
    }

    /// <inheritdoc/>
    public bool Equals(CodePointRange other) => Start == other.Start && End == other.End;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CodePointRange other && Equals(other);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    /// <summary>True if both ranges have the same start and end.</summary>
    public static bool operator ==(CodePointRange left, CodePointRange right) => left.Equals(right);
    /// <summary>True if the ranges differ in start or end.</summary>
    public static bool operator !=(CodePointRange left, CodePointRange right) => !left.Equals(right);

    /// <summary>Returns the range as <c>Start</c>, or <c>Start..End</c> when it spans more than one code point.</summary>
    public override string ToString()
        => Start == End ? Start.ToString() : $"{Start}..{End}";
}
