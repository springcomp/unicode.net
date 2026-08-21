namespace Unicode.NET;

/// <summary>Unicode binary properties (true/false per code point).</summary>
public enum BinaryProperty
{
    /// <summary>Alphabetic characters (includes letters and letter modifiers).</summary>
    Alphabetic,

    /// <summary>Default ignorable code points that should be ignored in rendering.</summary>
    Default_Ignorable_Code_Point,

    /// <summary>Hexadecimal digit characters (0-9, A-F, a-f and fullwidth variants).</summary>
    Hex_Digit,

    /// <summary>Reserved non-character code points (U+FDD0..U+FDEF and U+*FFFE, U+*FFFF).</summary>
    Noncharacter_Code_Point,

    /// <summary>Whitespace characters (U+0009..U+000D, U+0020, U+0085, U+00A0, etc.).</summary>
    White_Space,
}
