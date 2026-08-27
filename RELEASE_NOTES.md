# Unicode.NET 1.1.0

- Added full Unicode default whole-string lower- and upper-case mapping.
- Added contextual Greek final-sigma handling and one-to-many mappings.
- Added Unicode 15.1.0 and 16.0.0 case-mapping provenance and documentation.
- NuGet and symbol packages now include XML API documentation.

# Unicode.NET 1.0.0

Initial release of Unicode.NET — Unicode data and algorithms library for .NET.

## Features

- Unicode 15.1.0 data (versioned, updatable)
- General categories (30 categories + major unions)
- Unicode blocks (~300+ blocks)
- Scripts (~150 scripts) and script extensions
- Binary properties (Alphabetic, White_Space, Hex_Digit, etc.)
- Simple case folding (1:1 mappings)
- Case closure (symmetric BFS expansion)
- XML character sets (NameStartChar, NameChar, Char, Whitespace)
- XPath shortcut escapes (\d, \s, \w — ASCII-only)
- Unified property resolution API with alias support
- Property name suggestions (Levenshtein distance)

## Scope

Designed for XSD/XPath regex engines and Unicode-aware text processing.

## License

Apache-2.0 License
