namespace Unicode.NET;

/// <summary>Extension methods for enumerating <see cref="CodePoint"/> values from text.</summary>
public static class CodePointEnumerable
{
    /// <summary>Enumerates the code points of a char span, pairing surrogates.</summary>
    public static CodePointEnumerator EnumerateCodePoints(this ReadOnlySpan<char> source) => new(source);

    /// <summary>Enumerates the code points of a string, pairing surrogates.</summary>
    public static CodePointEnumerator EnumerateCodePoints(this string source) => new(source.AsSpan());
}
