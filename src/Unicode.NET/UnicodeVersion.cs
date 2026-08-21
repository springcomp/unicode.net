using System.Collections.Generic;
using Unicode.NET.Generated;

namespace Unicode.NET;

/// <summary>
/// Identifies a supported Unicode standard version (Major.Minor.Update).
/// </summary>
public readonly struct UnicodeVersion : IEquatable<UnicodeVersion>, IComparable<UnicodeVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Update { get; }

    public UnicodeVersion(int major, int minor, int update)
    {
        Major = major;
        Minor = minor;
        Update = update;
    }

    /// <summary>Unicode 15.1.0.</summary>
    public static readonly UnicodeVersion V15_1_0 = new(15, 1, 0);

    /// <summary>Unicode 16.0.0.</summary>
    public static readonly UnicodeVersion V16_0_0 = new(16, 0, 0);

    /// <summary>
    /// The Unicode version used by default by this library.
    /// </summary>
    public static readonly UnicodeVersion Current = V15_1_0;

    // ── Registry ────────────────────────────────────────────────────────────────

    internal static bool TryGetTables(UnicodeVersion version, out IVersionTables tables)
    {
        if (version == V15_1_0) { tables = VersionTables_15_1_0.Instance; return true; }
        if (version == V16_0_0) { tables = VersionTables_16_0_0.Instance; return true; }
        tables = null!;
        return false;
    }

    internal static IVersionTables GetTablesOrThrow(UnicodeVersion version)
    {
        if (!TryGetTables(version, out var tables))
            throw new NotSupportedException(
                $"Unicode version {version} is not registered. Registered versions: 15.1.0, 16.0.0.");
        return tables;
    }

    // ── Equality / comparison ────────────────────────────────────────────────────

    public bool Equals(UnicodeVersion other) =>
        Major == other.Major && Minor == other.Minor && Update == other.Update;

    public override bool Equals(object? obj) => obj is UnicodeVersion v && Equals(v);

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Update);

    public int CompareTo(UnicodeVersion other)
    {
        int c = Major.CompareTo(other.Major);
        if (c != 0) return c;
        c = Minor.CompareTo(other.Minor);
        if (c != 0) return c;
        return Update.CompareTo(other.Update);
    }

    public static bool operator ==(UnicodeVersion left, UnicodeVersion right) => left.Equals(right);
    public static bool operator !=(UnicodeVersion left, UnicodeVersion right) => !left.Equals(right);

    public override string ToString() => $"{Major}.{Minor}.{Update}";
}
