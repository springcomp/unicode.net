# Unicode.NET

Swiss-army knife toolkit for Unicode data and algorithms, built for XSD/XPath-compatible regex engines.

## Features

- **Unicode 15.1.0** data (default; 16.0.0 available for explicit selection)
- General categories (30 categories + major-category unions)
- Unicode blocks (~300+ blocks)
- Scripts and script extensions (163 scripts, ISO 15924 aliases)
- Binary properties: Alphabetic, White_Space, Hex_Digit, Default_Ignorable_Code_Point, Noncharacter_Code_Point
- Case folding: simple (1:1) and full (1:N)
- Default whole-string case mapping: lower- and upper-casing, including contextual and one-to-many mappings
- Case closure: symmetric BFS over simple-fold equivalents
- XML 1.0 character sets: `Char`, `NameStartChar`, `NameChar`, `Whitespace`
- XPath shortcut escapes: `\d` = `\p{Nd}`, `\w` = `[#x0000-#x10FFFF]-[\p{P}\p{Z}\p{C}]` (both Unicode-based),
- XPath shortcut escapes: `\s` = `[#x20\t\n\r]` (ASCII-only) — per the XSD/XPath regex spec
- Unified property resolution with fuzzy suggestions

## Installation

```xml
<PackageReference Include="Unicode.NET" Version="1.1.0" />
```

## Quick Start

### Resolve a Unicode property by name

```csharp
using Unicode.NET;

// Resolve by short name, long name, alias, or compound syntax
var set = UnicodeProperties.Resolve("Lu", UnicodeVersion.Current);
Console.WriteLine(set.Contains(CodePoint.Create('A'))); // true

// Compound syntax
var greekSet = UnicodeProperties.Resolve("sc=Grek", UnicodeVersion.Current);
var basicLatin = UnicodeProperties.Resolve("blk=BasicLatin", UnicodeVersion.Current);
```

### XML name validation

```csharp
using Unicode.NET.Xml;

var nameStartChar = XmlCharacterSets.NameStartChar;
Console.WriteLine(nameStartChar.Contains(CodePoint.Create('_')));  // true
Console.WriteLine(nameStartChar.Contains(CodePoint.Create('0')));  // false
```

### Case folding and closure

```csharp
using Unicode.NET;

// Simple fold: A → a
var folded = CaseFolding.Fold(CodePoint.Create('A'));
// folded[0].Value == 'a'

// Full fold: ﬁ → f + i
var fullFolded = CaseFolding.Fold(CodePoint.Create(0xFB01), CaseFoldingMode.Full);
// fullFolded.Count == 2

// Case closure: {A} → {A, a}
var input = new CodePointSet(new[] { CodePointRange.Create('A', 'A') });
var closure = CaseClosure.Closure(input, version: UnicodeVersion.Current);
Console.WriteLine(closure.Contains(CodePoint.Create('a'))); // true
```

### Default case mapping

```csharp
using Unicode.NET;

var lower = CaseMapping.ToLower("ΟΣ"); // "ος" (final sigma is contextual)
var upper = CaseMapping.ToUpper("straße"); // "STRASSE" (ß expands to SS)

// Omitted version uses UnicodeVersion.Current (15.1.0).
var current = CaseMapping.ToLower("İ");
var v16 = CaseMapping.ToLower("İ", UnicodeVersion.V16_0_0);
```

Case mapping is locale-insensitive and does not normalize text. Valid UTF-16 surrogate
pairs are supported; lone surrogates throw `ArgumentException`. Use `CaseFolding` for
caseless comparison rather than lower- or upper-casing. Unicode 15.1.0 remains the
default; Unicode 16.0.0 must be selected explicitly.

### Fuzzy suggestions on unknown property

```csharp
using Unicode.NET;

if (!UnicodeProperties.TryResolve("Leter", UnicodeVersion.Current, out _))
{
    var suggestions = UnicodeProperties.Suggest("Leter", UnicodeVersion.Current);
    // suggestions: ["Letter", "Lu", ...]
}
```

## Documentation

- [Unicode Version Policy](docs/UNICODE_VERSION.md)
- [Property Resolution](docs/PROPERTY_RESOLUTION.md)
- [Case Folding](docs/CASE_FOLDING.md)
- [Case Mapping](docs/CASE_MAPPING.md)
