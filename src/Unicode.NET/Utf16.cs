namespace Unicode.NET;

/// <summary>UTF-16 surrogate-pair encoding/decoding helpers for <see cref="CodePoint"/>.</summary>
public static class Utf16
{
    private const int SupplementaryPlaneBase = 0x10000;
    private const int SurrogateShift = 10;
    private const int SurrogateMask = (1 << SurrogateShift) - 1;

    /// <summary>True if <paramref name="c"/> is a UTF-16 high (lead) surrogate.</summary>
    public static bool IsHighSurrogate(char c) => c is >= (char)CodePoint.HighSurrogateStart and <= (char)CodePoint.HighSurrogateEnd;
    /// <summary>True if <paramref name="c"/> is a UTF-16 low (trail) surrogate.</summary>
    public static bool IsLowSurrogate(char c) => c is >= (char)CodePoint.LowSurrogateStart and <= (char)CodePoint.LowSurrogateEnd;

    /// <summary>Number of UTF-16 code units (1 or 2) needed to encode <paramref name="value"/>.</summary>
    public static int Utf16CodeUnitCount(CodePoint value) => value.Value > 0xFFFF ? 2 : 1;

    /// <summary>Tries to encode <paramref name="value"/> into <paramref name="destination"/>, returning <see langword="false"/> if there is not enough room.</summary>
    public static bool TryEncode(CodePoint value, Span<char> destination, out int charsWritten)
    {
        int scalar = value.Value;

        if (scalar <= 0xFFFF)
        {
            if (destination.Length < 1)
            {
                charsWritten = 0;
                return false;
            }

            destination[0] = (char)scalar;
            charsWritten = 1;
            return true;
        }

        if (destination.Length < 2)
        {
            charsWritten = 0;
            return false;
        }

        scalar -= SupplementaryPlaneBase;
        destination[0] = (char)(CodePoint.HighSurrogateStart + (scalar >> SurrogateShift));
        destination[1] = (char)(CodePoint.LowSurrogateStart + (scalar & SurrogateMask));
        charsWritten = 2;
        return true;
    }

    /// <summary>Encodes <paramref name="value"/> as a UTF-16 string of 1 or 2 chars.</summary>
    public static string Encode(CodePoint value)
    {
        Span<char> buffer = stackalloc char[2];
        TryEncode(value, buffer, out int charsWritten);
        return new string(buffer[..charsWritten]);
    }

    /// <summary>
    /// Decodes the code point starting at index 0 of <paramref name="source"/>. A well-formed
    /// high/low surrogate pair decodes to a single supplementary-plane code point (2 chars consumed).
    /// A lone surrogate (unpaired) is permissively decoded as its own <see cref="CodePoint"/>
    /// value (1 char consumed) rather than throwing, since <see cref="CodePoint"/> allows surrogate values.
    /// </summary>
    public static void Decode(ReadOnlySpan<char> source, out CodePoint value, out int charsConsumed)
    {
        if (source.IsEmpty)
            throw new ArgumentException("Source span must not be empty.", nameof(source));

        char first = source[0];

        if (IsHighSurrogate(first) && source.Length > 1 && IsLowSurrogate(source[1]))
        {
            int scalar = SupplementaryPlaneBase + ((first - CodePoint.HighSurrogateStart) << SurrogateShift) + (source[1] - CodePoint.LowSurrogateStart);
            value = CodePoint.Create(scalar);
            charsConsumed = 2;
            return;
        }

        value = CodePoint.Create(first);
        charsConsumed = 1;
    }
}
