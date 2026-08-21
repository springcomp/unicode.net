using System.Collections.Generic;
using System.Linq;

namespace Unicode.NET;

/// <summary>
/// Provides metadata about Unicode versions baked into the current build.
/// </summary>
public static class UnicodeVersionInfo
{
    /// <summary>
    /// Gets the set of Unicode versions supported by this build.
    /// </summary>
    public static IReadOnlyList<UnicodeVersion> SupportedVersions { get; } = new[]
    {
        UnicodeVersion.V15_1_0,
        UnicodeVersion.V16_0_0 // Add newer versions here when available
    };

    /// <summary>
    /// Returns the current Unicode version used by this library.
    /// </summary>
    public static UnicodeVersion Current => UnicodeVersion.Current;
}
