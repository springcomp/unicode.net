# Default Case Mapping

`CaseMapping` applies the Unicode default, locale-insensitive case mappings to a
whole UTF-16 string. It is not case folding and it does not normalize text.

## API

```csharp
string CaseMapping.ToLower(string input, UnicodeVersion? version = null);
string CaseMapping.ToUpper(string input, UnicodeVersion? version = null);
```

The omitted version is `UnicodeVersion.Current`, currently Unicode 15.1.0. Unicode
16.0.0 is supported when selected explicitly:

```csharp
var lower = CaseMapping.ToLower("ΟΣ"); // "ος", with contextual final sigma
var upper = CaseMapping.ToUpper("straße"); // "STRASSE", because ß maps to SS

var current = CaseMapping.ToLower("İ");
var v16 = CaseMapping.ToLower("İ", UnicodeVersion.V16_0_0);
```

Mappings can expand one code point to many. For example, uppercase sharp s
(`ß`, U+00DF) produces `SS`; lowercase dotted capital I (`İ`, U+0130) produces
`i` followed by combining dot above.

Greek capital sigma uses context: it becomes final sigma (`ς`) when preceded by a
cased character and not followed by one, otherwise medial sigma (`σ`). Case
ignorable characters do not interrupt that context.

The mapping is locale-insensitive and is not normalization. Valid surrogate pairs
are processed as scalar values; a lone surrogate throws `ArgumentException`.
`ArgumentNullException` is thrown for null input, and `NotSupportedException` is
thrown for an unregistered Unicode version.

For caseless comparison, use `CaseFolding` instead. Unicode 15.1.0 remains the
default; Unicode 16.0.0 requires explicit version selection. Xslt.NET migration is
outside this release.
