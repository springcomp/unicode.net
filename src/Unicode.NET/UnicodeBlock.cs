namespace Unicode.NET;

/// <summary>
/// A named Unicode block: a contiguous range of code points with an official Unicode name.
/// </summary>
public readonly struct UnicodeBlock : IEquatable<UnicodeBlock>
{
    /// <summary>The official Unicode block name (e.g. "Basic Latin").</summary>
    public string Name { get; }

    /// <summary>The code-point range covered by this block.</summary>
    public CodePointRange Range { get; }

    public UnicodeBlock(string name, CodePointRange range)
    {
        Name = name;
        Range = range;
    }

    public bool Equals(UnicodeBlock other) => Name == other.Name && Range == other.Range;
    public override bool Equals(object? obj) => obj is UnicodeBlock b && Equals(b);
    public override int GetHashCode() => HashCode.Combine(Name, Range);

    public static bool operator ==(UnicodeBlock left, UnicodeBlock right) => left.Equals(right);
    public static bool operator !=(UnicodeBlock left, UnicodeBlock right) => !left.Equals(right);

    public override string ToString() => $"{Name} ({Range})";
}
