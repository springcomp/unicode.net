# Case Folding Design

## Purpose

This document defines the implementation contract for Unicode case folding in this repository. It is intended to guide future agents implementing parsers, generated tables, runtime APIs, case closure, and tests.

The design is based on the Unicode Character Database (UCD) `CaseFolding.txt` file and the Unicode Standard, Section 3.13, Default Case Algorithms. The checked-in source data is Unicode 16.0.0 at `Unicode/16.0/CaseFolding.txt`.

Case folding is a Unicode, versioned operation for caseless comparison. It is not ordinary lowercasing, culture-sensitive casing, normalization, or compatibility decomposition.

## Normative Sources and Provenance

### Normative sources

1. Unicode Standard, Section 3.13, Default Case Algorithms.
2. The `CaseFolding.txt` file for the selected UCD version.
3. UAX #44 for UCD file formats, versioning, and data-file conventions.

UTR #30 is useful background and historical prior art, including the repository reference to `tr30-4`, but that old document must not override the current Unicode Standard or the selected UCD file.

### Version rule

Every generated table and every runtime data provider must identify its Unicode version. Never mix files from different UCD releases. The current source is:

- Unicode release: `16.0.0`
- Repository directory: `Unicode/16.0`
- File: `CaseFolding.txt`
- File date: `2024-04-30`

Generated output must be deterministic and must not contain a generation timestamp. The source Unicode version is sufficient provenance in generated source; the generator version may be recorded only if it is stable and intentionally part of the output contract.

The runtime library must not download UCD data or read source files. Parsing and generation belong to `tools/Unicode.NET.CodeGen`; the library consumes committed generated data.

## UCD File Model

Each non-comment record has the form:

```text
<code point>; <status>; <mapping>; # <name>
```

Whitespace around fields is insignificant. The comment and name are diagnostic metadata, not runtime mapping data.

### Statuses

The parser must retain all four statuses as distinct values:

| Status | Meaning | Default simple map | Default full map |
| --- | --- | ---: | ---: |
| `C` | Common mapping shared by simple and full folding | Yes | Yes |
| `F` | Full mapping, possibly one-to-many | No | Yes |
| `S` | Simple alternative where the full mapping expands | Yes | No |
| `T` | Turkic alternative | No | No |

All code points absent from the file have an implicit common identity mapping. The parser need not materialize those identity records.

Examples from Unicode 16.0.0:

```text
0041; C; 0061       # A -> a
00DF; F; 0073 0073  # sharp s -> ss
1E9E; S; 00DF       # capital sharp s -> sharp s for simple folding
0049; T; 0131      # I -> dotless i in Turkic mode
0130; T; 0069      # dotted I -> i in Turkic mode
```

A source code point may have more than one status record. The parsed representation must preserve each record rather than overwriting by key alone.

### Parsed model

Use a scalar/code-point type already established by the library. If a parser-level primitive is needed, it should be a validated integer in `0..0x10FFFF`, excluding surrogate code points `0xD800..0xDFFF` for scalar mappings.

Conceptually:

```csharp
enum CaseFoldStatus : byte
{
    Common,
    Full,
    Simple,
    Turkic
}

readonly record struct CaseFoldRecord(
    int CodePoint,
    CaseFoldStatus Status,
    ImmutableArray<int> Mapping,
    string? Name);
```

The exact type names may follow the repository's eventual public naming, but these properties are required:

- Mapping is a sequence, never a nullable single value.
- `C`, `S`, and `T` records contain exactly one mapping scalar.
- `F` records may contain one or more mapping scalars.
- Source values and mapping values are validated while parsing.
- Blank lines and comments are ignored.
- Malformed records fail with a useful source line number and reason; silent data loss is unacceptable.
- Duplicate `(code point, status)` records are rejected unless the generator explicitly defines and tests a canonical duplicate policy.
- Names are optional and should not inflate generated runtime tables.

## Folding Semantics

### Default simple folding

Simple folding maps one input code point to one output code point:

```text
SimpleFold(c) = C mapping, if present
SimpleFold(c) = S mapping, if present and no C mapping is selected for that status
SimpleFold(c) = c, otherwise
```

Operationally, select all `C` and `S` records, with at most one effective mapping per source. The UCD's `C+S` rule is authoritative. Do not derive folding by calling `char.ToLower`, `Rune.ToLower`, or culture-aware casing APIs.

Simple folding preserves code-point count, but not necessarily UTF-16 `char` count because supplementary scalar values occupy two UTF-16 code units.

### Default full folding

Full folding maps one input code point to a sequence:

```text
FullFold(c) = C mapping, if present
FullFold(c) = F mapping, otherwise if present
FullFold(c) = [c], otherwise
```

Operationally, select `C+F`. Full mappings can expand strings:

- `00DF` -> `0073 0073` (`ß` -> `ss`)
- `FB03` -> `0066 0066 0069` (`ﬃ` -> `ffi`)
- `0130` -> `0069 0307` in default mode

Full folding is the preferred operation for general caseless string matching.

### Turkic folding

`T` rows are an explicit alternative for Turkish and Azerbaijani behavior. They are excluded from default folding. A caller must opt into Turkic behavior through an explicit mode or policy value.

Recommended policy concepts:

```csharp
enum CaseFoldingLocale : byte
{
    Default,
    Turkic
}
```

Do not infer this option from `CultureInfo`, the process locale, machine settings, or user language. For Turkic mode, use `T` mappings where available and the normal default mappings elsewhere:

| Input | Default full | Turkic full |
| --- | --- | --- |
| `0049` (`I`) | `0069` (`i`) | `0131` (dotless i) |
| `0130` (`İ`) | `0069 0307` | `0069` |

The UCD notes that Turkic mappings do not preserve canonical equivalence without additional processing. The API must not claim that Turkic folding is canonical-caseless matching.

### Folding is not normalization

Case folding does not preserve NFC, NFD, or other normalization forms. Keep these operations separate:

- `CaseFold`: apply UCD case-folding mappings.
- `CanonicalCaselessEquals`: apply the documented NFD-before/after folding algorithm.
- `NfkcCaseFold`: compatibility normalization plus folding, only as an explicitly named operation.

Do not hide normalization inside ordinary `CaseFold`. Do not call case folding NFKC case folding.

### Context and special cases

Case folding is context-independent. Do not implement final sigma context rules used by lowercasing.

Important expected results include:

| Input | Default result |
| --- | --- |
| `03A3` (`Σ`) | `03C3` (`σ`) |
| `03C2` (`ς`) | `03C3` (`σ`) |
| `0345` | `03B9` |
| `212A` (`K`) | `006B` (`k`) |
| `2126` (`Ω`) | `03C9` (`ω`) |
| `AB70` | `13A0` |
| `10400` | `10428` |

Cherokee demonstrates that a folded result is not necessarily lowercase. Compatibility-looking mappings present in `CaseFolding.txt` are explicit case-folding data; they do not authorize general compatibility decomposition.

## Public API Shape

The API must be code-point-oriented internally and sequence-capable from its first version. Do not introduce a scalar-only return type that would require a breaking change when full folding is implemented.

A representative shape is:

```csharp
ReadOnlySpan<CodePoint> Fold(
    CodePoint value,
    CaseFoldingMode mode = CaseFoldingMode.Full,
    CaseFoldingLocale locale = CaseFoldingLocale.Default);

string Fold(
    ReadOnlySpan<char> value,
    CaseFoldingMode mode = CaseFoldingMode.Full,
    CaseFoldingLocale locale = CaseFoldingLocale.Default);

bool CaselessEquals(
    ReadOnlySpan<char> left,
    ReadOnlySpan<char> right,
    CaseFoldingMode mode = CaseFoldingMode.Full,
    CaseFoldingLocale locale = CaseFoldingLocale.Default);
```

The final public names may follow the library's established conventions. The contract is more important than the spelling:

- `Simple` and `Full` modes exist from the beginning.
- Full mode returns or writes a sequence even when the current input has a one-scalar result.
- No full-mode call silently falls back to simple mode.
- Unsupported Unicode versions fail explicitly.
- Invalid code points fail explicitly.
- Allocation-sensitive callers have a destination-buffer, enumerator, or equivalent streaming path for full folding.

If implementation is staged and full folding is temporarily unavailable, `Full` must throw a clear `NotSupportedException`. It must not silently change semantics.

### UTF-16 behavior

Use scalar-aware enumeration, such as `System.Text.Rune` or the repository's code-point utility. Never fold the two UTF-16 code units of a surrogate pair independently.

Choose and document one malformed UTF-16 policy. Recommended policy for this scalar library: reject unpaired surrogates with an argument exception. Do not silently replace malformed input unless the API is explicitly a replacement-decoding API.

## Generated Data Representation

The generator should preserve a status-rich parsed model but emit lookup-oriented tables.

Recommended generated contents per Unicode version:

1. A sparse simple map: source scalar -> scalar.
2. A sparse full map: source scalar -> offset/length into a packed scalar pool.
3. A sparse Turkic map: source scalar -> offset/length, or a scalar-only equivalent where all current records are one-to-one.
4. Version metadata and source filename.

Identity is implicit. Do not generate entries for every unlisted scalar.

A compact sequence table can use:

```text
MappingEntry { Source, Offset, Length }
int[] MappingPool
```

Requirements:

- Sort entries by source code point, then use stable mapping order.
- Emit stable formatting and no timestamps.
- Validate every generated mapping as a Unicode scalar.
- Keep generated tables immutable to consumers.
- Ensure lookup is deterministic and does not depend on dictionary iteration order.
- Make the generated shape additive: `FullMap` must be able to sit beside `SimpleMap` without changing runtime dispatch signatures.

The current tool in `tools/Unicode.NET.CodeGen` only downloads the three UCD files. Future work should add separate `download`, `generate`, and `update` boundaries. The library must reference generated output, not the downloader or source files.

## Case Closure

Simple case closure is a code-point-set operation. Given a set `S` and simple fold `f`, closure means adding every scalar that folds to a member of the set:

$$
\operatorname{Closure}(S) = S \cup \{x \mid f(x) \in S\}
$$

Compute to a fixed point if the implementation uses an iterative algorithm. The result must be:

- A superset of the input.
- Idempotent: closing an already closed set changes nothing.
- Terminating over the finite Unicode scalar domain.
- Explicitly parameterized by locale/policy if Turkic closure is supported.

Examples:

- Closure of `{006B}` includes `004B`, `006B`, and `212A`.
- Closure of `{0073}` includes `0053`, `0073`, `017F`, and other simple equivalents.
- Closure of `{03C3}` includes `03A3`, `03C2`, and `03C3`.

Do not call this full case closure. Full mappings such as `00DF -> 0073 0073` and `FB03 -> 0066 0066 0069` cannot be represented by a set of individual code points. Full closure requires sequence-aware matching, a set of folded strings, or an automaton/regex-level operation.

Case folding is many-to-one and loses case information. There is no general inverse function. Closure needs reverse indexing or a scan of the generated simple map; it must not pretend that forward lookup alone is an inverse.

## Correctness Invariants

For every supported Unicode version:

1. Unlisted scalars map to themselves.
2. Simple mappings contain exactly one scalar.
3. Full mappings contain one or more scalars.
4. Every mapping scalar is valid and non-surrogate.
5. Mapping output preserves input order; folding does not reorder neighboring input scalars.
6. Simple folding is idempotent: `SimpleFold(SimpleFold(x)) == SimpleFold(x)`.
7. Full folding is idempotent when applied to the resulting scalar sequence.
8. Default mode never selects a `T` record.
9. Turkic mode selects `T` alternatives where defined.
10. For every `C` record, simple and full results agree.
11. For an `S` record, simple uses `S` while full uses the corresponding `F` mapping.
12. Folding is not assumed to produce normalized output.
13. Case folding is not assumed to produce lowercase output.

The generator should verify these invariants where practical and fail generation on contradictory or malformed UCD data.

## Test Plan

Tests must use small fixtures for parser behavior and the checked-in UCD 16.0.0 file for integration/property coverage. No test should require network access.

### Parser tests

Cover:

- Comments and blank lines.
- Hexadecimal source and mapping values.
- `C`, `F`, `S`, and `T` records retained distinctly.
- Multiple statuses for one source code point.
- Multi-scalar mappings.
- Invalid status, malformed hex, empty mapping, out-of-range scalar, and duplicate status handling.
- Useful line-numbered diagnostics.

### Runtime vectors

Use hexadecimal vectors to avoid source encoding ambiguity:

| Input | Mode | Expected |
| --- | --- | --- |
| `0041` | Simple/default | `0061` |
| `0041` | Full/default | `0061` |
| `00DF` | Simple/default | `00DF` |
| `00DF` | Full/default | `0073 0073` |
| `1E9E` | Simple/default | `00DF` |
| `1E9E` | Full/default | `0073 0073` |
| `0049` | Full/default | `0069` |
| `0049` | Full/Turkic | `0131` |
| `0130` | Full/default | `0069 0307` |
| `0130` | Full/Turkic | `0069` |
| `03A3` | Full/default | `03C3` |
| `03C2` | Full/default | `03C3` |
| `0345` | Full/default | `03B9` |
| `212A` | Full/default | `006B` |
| `FB03` | Full/default | `0066 0066 0069` |
| `AB70` | Full/default | `13A0` |
| `10400` | Full/default | `10428` |

Whole-string tests should include:

- `FullFold("MASSE") == FullFold("Maße")`.
- Simple folding does not expand `00DF`.
- Default `FullFold("İ")` differs from `FullFold("i")`.
- Turkic folding gives the documented `I`/`İ` behavior.
- Supplementary-plane pairs are processed as scalars.
- Malformed UTF-16 follows the chosen documented policy.

### Exhaustive and property tests

For all Unicode scalar values in the selected version, verify idempotence, scalar validity, simple width, and identity for unlisted values. Verify generated tables are sorted, deterministic, and unchanged on a second generation run.

For case closure, verify containment of representative equivalents, termination, and idempotence.

Canonical-caseless and compatibility-caseless tests must be separate from ordinary folding tests. Include `A\u030A` versus `Å` only in the canonical-caseless suite, not as a promise of binary equality from `CaseFold` alone.

## Generation and Version-Update Workflow

When adding or updating a Unicode version:

1. Run the UCD tool's `update` command for the exact release, for example `dotnet run --project tools/Unicode.NET.CodeGen -- update --version 16.0.0`.
2. Confirm `UnicodeData.txt`, `Blocks.txt`, and `CaseFolding.txt` all come from the same release.
3. Parse with strict diagnostics and generate deterministic source.
4. Review the generated diff, especially status conflicts, mapping expansions, and version metadata.
5. Register the generated version in the library's version registry.
6. Run parser tests, generated-table tests, runtime vectors, closure tests, and the full solution test suite.
7. Confirm a second generation run produces no diff.
8. Record any intentional behavior change caused by the new Unicode version.

Do not hand-edit generated tables. Do not make runtime behavior depend on whichever Unicode version happens to be installed on the host.

## Implementation Phases

1. Define validated scalar, mode, locale, record, and sequence types consistent with the library.
2. Implement strict `CaseFolding.txt` parsing with all four statuses preserved.
3. Add deterministic simple-table generation from `C+S`.
4. Add sequence-table generation from `C+F` and explicit Turkic tables.
5. Implement scalar-safe simple and full folding APIs with version selection.
6. Implement simple code-point closure using reverse mappings and fixed-point tests.
7. Add canonical-caseless and compatibility-caseless operations as separate features, only with their required normalization data.
8. Add exhaustive Unicode-versioned validation and update workflow checks.

## Decisions That Future Agents Must Not Reverse Accidentally

- Default folding excludes `T` mappings.
- Full folding is sequence-valued and is the primary default for string caseless matching.
- Simple folding and simple case closure are distinct from full folding and full sequence matching.
- Folding is not lowercasing and does not imply normalization.
- Cherokee and other non-lowercase fold results are valid.
- Unicode data is generated offline and committed; runtime performs no UCD I/O.
- Unicode version is part of the data contract, not an implementation detail.
