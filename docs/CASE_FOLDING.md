# Case Folding

Unicode case folding maps code points to a canonical form for case-insensitive comparison.  
It is not lowercasing and does not imply normalization.

## API

```csharp
// Fold a single code point
IReadOnlyList<CodePoint> CaseFolding.Fold(
    CodePoint codePoint,
    CaseFoldingMode mode = CaseFoldingMode.Simple,
    CaseFoldingLocale locale = CaseFoldingLocale.Default,
    UnicodeVersion? version = null)

// Compute case closure of a set
CodePointSet CaseClosure.Closure(
    CodePointSet input,
    CaseFoldingMode mode = CaseFoldingMode.Simple,
    CaseFoldingLocale locale = CaseFoldingLocale.Default,
    UnicodeVersion? version = null)
```

## Simple vs Full Folding

| Mode | Mapping | Example |
|------|---------|---------|
| Simple | 1:1 — one code point in, one code point out | `A` → `a`, `İ` → `i` |
| Full | 1:N — one code point may expand to multiple | `ẞ` → `ss`, `ﬁ` → `fi` |

XPath/XSD use simple folding only (per spec).  
Full folding is used when string-level case-insensitive matching is required.

```csharp
// Simple fold
var simple = CaseFolding.Fold(CodePoint.Create('A'));
// simple[0].Value == (int)'a'

// Full fold: ﬁ (U+FB01) → f + i
var full = CaseFolding.Fold(CodePoint.Create(0xFB01), CaseFoldingMode.Full);
// full.Count == 2, full[0].Value == (int)'f', full[1].Value == (int)'i'

// Identity: no fold entry → returns input unchanged
var id = CaseFolding.Fold(CodePoint.Create('1'));
// id[0].Value == (int)'1'
```

## Case Closure

Case closure computes all code points case-equivalent to any member of the input set, using BFS over the simple-fold map and its reverse.

```csharp
var input = new CodePointSet(new[] { CodePointRange.Create('A', 'A') });
var closure = CaseClosure.Closure(input);
closure.Contains(CodePoint.Create('a')); // true — a folds to a, A folds to a
closure.Contains(CodePoint.Create('A')); // true — input always included
```

Closure is idempotent: `Closure(Closure(s)) == Closure(s)`.

## Locale Handling

Only `CaseFoldingLocale.Default` is implemented.  
`CaseFoldingLocale.Turkic` (dotted-I rules for Turkish/Azeri) is reserved; passing it throws `NotSupportedException`.

## Limitations

- Case closure operates on code-point sets, not strings.  
  Full-folding 1:N mappings (e.g., `ß` → `ss`) are not expanded by closure; only simple-fold equivalents are added.
- `CaseFoldingMode.Full` is not supported by `CaseClosure.Closure`; it throws `NotSupportedException`.
- Turkic locale is not yet implemented in either `Fold` or `Closure`.
