namespace Unicode.NET;

/// <summary>Enumerates the <see cref="CodePoint"/> values of a <see cref="ReadOnlySpan{Char}"/>, pairing surrogates.</summary>
public ref struct CodePointEnumerator
{
    private ReadOnlySpan<char> _remaining;

    internal CodePointEnumerator(ReadOnlySpan<char> source) => _remaining = source;

    /// <summary>The code point produced by the most recent successful call to <see cref="MoveNext"/>.</summary>
    public CodePoint Current { get; private set; }

    /// <summary>Advances to the next code point. Returns <see langword="false"/> when the source is exhausted.</summary>
    public bool MoveNext()
    {
        if (_remaining.IsEmpty)
            return false;

        Utf16.Decode(_remaining, out var value, out int charsConsumed);
        Current = value;
        _remaining = _remaining[charsConsumed..];
        return true;
    }

    /// <summary>Returns this enumerator, enabling <c>foreach</c> support.</summary>
    public readonly CodePointEnumerator GetEnumerator() => this;
}
