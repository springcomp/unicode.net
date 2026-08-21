namespace Unicode.NET;

/// <summary>
/// A raw Unicode code point in <c>U+0000..U+10FFFF</c>. This single type covers both
/// scalar values and UTF-16 surrogates (<c>U+D800..U+DFFF</c>) — see <see cref="IsScalarValue"/> —
/// rather than introducing a separate scalar-only type, since UCD range data spans surrogates.
/// </summary>
public readonly struct CodePoint : IEquatable<CodePoint>, IComparable<CodePoint>
{
    /// <summary>The lowest valid code point, <c>U+0000</c>.</summary>
    public const int MinValue = 0x0000;
    /// <summary>The highest valid code point, <c>U+10FFFF</c>.</summary>
    public const int MaxValue = 0x10FFFF;

    /// <summary>The first high-surrogate value, <c>U+D800</c>.</summary>
    public const int HighSurrogateStart = 0xD800;
    /// <summary>The last high-surrogate value, <c>U+DBFF</c>.</summary>
    public const int HighSurrogateEnd = 0xDBFF;
    /// <summary>The first low-surrogate value, <c>U+DC00</c>.</summary>
    public const int LowSurrogateStart = 0xDC00;
    /// <summary>The last low-surrogate value, <c>U+DFFF</c>.</summary>
    public const int LowSurrogateEnd = 0xDFFF;

    private readonly int _value;

    private CodePoint(int value) => _value = value;

    /// <summary>The raw scalar/surrogate value of this code point.</summary>
    public int Value => _value;

    /// <summary>True if this is a UTF-16 high (lead) surrogate.</summary>
    public bool IsHighSurrogate => _value is >= HighSurrogateStart and <= HighSurrogateEnd;
    /// <summary>True if this is a UTF-16 low (trail) surrogate.</summary>
    public bool IsLowSurrogate => _value is >= LowSurrogateStart and <= LowSurrogateEnd;
    /// <summary>True if this is a high or low UTF-16 surrogate.</summary>
    public bool IsSurrogate => _value is >= HighSurrogateStart and <= LowSurrogateEnd;
    /// <summary>True if this is a valid Unicode scalar value (not a surrogate).</summary>
    public bool IsScalarValue => !IsSurrogate;
    /// <summary>True if this code point lies within the Basic Multilingual Plane (<c>U+0000..U+FFFF</c>).</summary>
    public bool IsBmp => _value <= 0xFFFF;

    /// <summary>Creates a code point, throwing if <paramref name="value"/> is out of range.</summary>
    public static CodePoint Create(int value)
    {
        if (!TryCreate(value, out var codePoint))
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Code point must be in range U+{MinValue:X4}..U+{MaxValue:X4}.");

        return codePoint;
    }

    /// <summary>Tries to create a code point, returning <see langword="false"/> if <paramref name="value"/> is out of range.</summary>
    public static bool TryCreate(int value, out CodePoint codePoint)
    {
        if (value < MinValue || value > MaxValue)
        {
            codePoint = default;
            return false;
        }

        codePoint = new CodePoint(value);
        return true;
    }

    /// <summary>Creates a code point that is guaranteed not to be a surrogate.</summary>
    public static CodePoint CreateScalar(int value)
    {
        var codePoint = Create(value);
        if (codePoint.IsSurrogate)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value is a UTF-16 surrogate and is not a valid Unicode scalar value.");

        return codePoint;
    }

    /// <inheritdoc/>
    public bool Equals(CodePoint other) => _value == other._value;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CodePoint other && Equals(other);
    /// <inheritdoc/>
    public override int GetHashCode() => _value;
    /// <inheritdoc/>
    public int CompareTo(CodePoint other) => _value.CompareTo(other._value);
    /// <summary>Returns the code point in <c>U+XXXX</c> notation.</summary>
    public override string ToString() => $"U+{_value:X4}";

    /// <summary>True if the two code points have the same value.</summary>
    public static bool operator ==(CodePoint left, CodePoint right) => left.Equals(right);
    /// <summary>True if the two code points have different values.</summary>
    public static bool operator !=(CodePoint left, CodePoint right) => !left.Equals(right);
    /// <summary>True if <paramref name="left"/> sorts before <paramref name="right"/>.</summary>
    public static bool operator <(CodePoint left, CodePoint right) => left.CompareTo(right) < 0;
    /// <summary>True if <paramref name="left"/> sorts before or equal to <paramref name="right"/>.</summary>
    public static bool operator <=(CodePoint left, CodePoint right) => left.CompareTo(right) <= 0;
    /// <summary>True if <paramref name="left"/> sorts after <paramref name="right"/>.</summary>
    public static bool operator >(CodePoint left, CodePoint right) => left.CompareTo(right) > 0;
    /// <summary>True if <paramref name="left"/> sorts after or equal to <paramref name="right"/>.</summary>
    public static bool operator >=(CodePoint left, CodePoint right) => left.CompareTo(right) >= 0;

    /// <summary>Converts to the raw <see cref="int"/> value.</summary>
    public static explicit operator int(CodePoint codePoint) => codePoint._value;
    /// <summary>Creates a code point from a raw <see cref="int"/> value, throwing if out of range.</summary>
    public static explicit operator CodePoint(int value) => Create(value);
}
