# Property Resolution

`UnicodeProperties` is the unified facade for resolving any Unicode property by name or alias.

## API

```csharp
// Try-resolve pattern (no exception on failure)
bool TryResolve(string name, UnicodeVersion version, out CodePointSet set)

// Resolve or throw UnknownPropertyException with suggestions
CodePointSet Resolve(string name, UnicodeVersion version)

// Get closest-matching candidates for an unrecognised name
IEnumerable<string> Suggest(string query, UnicodeVersion version, int maxSuggestions = 3)
```

## Alias Resolution Rules

- Case-insensitive ASCII comparison.
- Underscores and hyphens stripped for fuzzy matching (e.g., `White_Space` = `whitespace` = `white-space`).
- `Is`/`In` prefix stripped for blocks (e.g., `IsBasicLatin` → `BasicLatin`).
- ISO 15924 four-letter aliases accepted for scripts (e.g., `Latn` = `Latin`, `Grek` = `Greek`).

## Compound Syntax

Property name and value can be combined with `=`:

| Syntax | Resolves to |
|--------|------------|
| `gc=Lu` | General_Category = Uppercase_Letter |
| `gc=Uppercase_Letter` | General_Category = Uppercase_Letter |
| `Script=Greek` | Script = Greek |
| `sc=Grek` | Script = Greek (ISO 15924 alias) |
| `blk=BasicLatin` | Block = Basic Latin |
| `block=Basic Latin` | Block = Basic Latin |

## Resolution Order (no prefix)

When no compound prefix is provided, resolution proceeds in this order:

1. **General categories** — 30 categories (`Lu`, `Ll`, …) plus major-category unions (`L`, `Letter`, `N`, `Number`, …)
2. **Unicode blocks** — ~300+ blocks; `Is`/`In` prefix stripped
3. **Scripts** — ~163 scripts; ISO 15924 aliases accepted
4. **Binary properties** — Alphabetic, White_Space, Hex_Digit, Default_Ignorable_Code_Point, Noncharacter_Code_Point

The first match wins.

## Suggestions on Error

When resolution fails, `Resolve()` throws `UnknownPropertyException` with a `Suggestions` list.  
`Suggest()` can be called directly to get candidates sorted by Levenshtein distance.

```csharp
try
{
    var set = UnicodeProperties.Resolve("Leter", UnicodeVersion.Current);
}
catch (UnknownPropertyException ex)
{
    // ex.PropertyName == "Leter"
    // ex.Suggestions may include "Letter", "Lu", ...
}
```

## Dialect Considerations

`UnicodeProperties` returns code-point sets without dialect filtering.  
XSD and XPath have different rules about which property categories are supported:

- XSD allows general categories and blocks; scripts are not part of XSD character classes.
- XPath 2.0+ allows general categories and blocks; XPath 3.1 also allows script properties.

Callers (e.g., the regex engine) are responsible for dialect-specific gating.
