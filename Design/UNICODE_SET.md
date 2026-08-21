# Unicode Set Design

## Purpose

This document defines the implementation contract for immutable Unicode code-point ranges and sets in this repository. It is intended to guide future agents implementing the core value types, mutable construction helpers, set algebra, generated property tables, XML character sets, case closure, and tests.

The set API is a reusable data structure. It must not depend on a particular Unicode property, regular-expression engine, XML version, or case-folding policy.

## Normative Sources and Scope

The conceptual model is based on the Unicode Standard code-point model, UAX #44 data-file conventions, and ICU UnicodeSet prior art. The library's set operations are mathematical operations over an explicitly selected integer universe; they do not infer semantics from the host runtime's Unicode version.

Relevant repository references are listed in `AGENTS.md`, especially:

- The Unicode Standard, Chapter 2, General Structure.
- The Unicode Standard, Chapter 3, Conformance.
- UAX #44, Unicode Character Database.
- UAX #18, Unicode Regular Expressions.
- ICU UnicodeSet documentation.

This design covers:

- Inclusive code-point ranges.
- Immutable finite sets represented as canonical ranges.
- Membership, enumeration, equality, hashing, and set algebra.
- Builders and generated-data consumers.

This design does not cover normalization, grapheme segmentation, collation, regular-expression parsing, or Unicode property derivation itself.

## Terminology and Domain

### Code points and scalar values

A Unicode code point is an integer in `0..0x10FFFF`. Surrogate code points `0xD800..0xDFFF` are code points but are not Unicode scalar values. The set layer is code-point-oriented and therefore must be able to represent ranges containing surrogate code points when its selected universe requires them.

Do not silently replace code points with UTF-16 code units. A range such as `U+D800..U+DFFF` is one contiguous code-point range even though those values are not valid scalar values for encoding as Unicode text.

The core `CodePoint` type and any scalar-specific type must define their validation rules separately. `CodePointSet` must use the repository's validated code-point type and must not call UTF-16 encoding as part of set operations.

### Set universe

The default universe for `CodePointSet` is every Unicode code point:

```text
U = [0x000000, 0x10FFFF]
```

`Complement()` means `U \ S`. It does not mean the set of scalar values, assigned characters, printable characters, or values supported by the current .NET runtime.

If a future API needs a scalar-only complement, it must expose that universe explicitly rather than changing the meaning of the existing `Complement()` operation. A future overload may use a `CodePointSet universe` or a named universe value.

### Ranges

A `CodePointRange` is an immutable inclusive interval `[Start, End]` where:

```text
0 <= Start <= End <= 0x10FFFF
```

The empty interval is not representable as a range. Empty sets are represented by zero ranges.

## Canonical Representation

`CodePointSet` is immutable and stores a sorted array or equivalent read-only storage of `CodePointRange` values. Every stored range must satisfy all of these properties:

1. Ranges are sorted by ascending `Start`.
2. Ranges are disjoint.
3. Ranges are not adjacent.
4. Every range is valid and inclusive.
5. No range is empty.
6. The representation contains no redundant identity, sentinel, or null entries.

For example, these inputs produce one canonical set:

```text
[U+0041..U+0045], [U+0044..U+0050], [U+0051..U+005A]
=> [U+0041..U+005A]
```

Canonicalization is part of the type invariant, not merely an optimization. Therefore construction order must not affect equality, hashing, enumeration, or subsequent operation results.

The implementation may use an array internally because it is compact, cache-friendly, and safe to share after construction. It must not expose a mutable array or allow a caller to mutate the set through a returned span or collection.

## Value-Type Contracts

### `CodePointRange`

Conceptual API:

```csharp
readonly record struct CodePointRange
{
    CodePoint Start { get; }
    CodePoint End { get; }

    bool Contains(CodePoint value);
    bool Overlaps(CodePointRange other);
    bool IsAdjacentTo(CodePointRange other);
}
```

The final spelling may follow repository conventions, but the behavior is required:

- Construction rejects an end before the start.
- Construction rejects values outside the code-point domain.
- `Contains` includes both endpoints.
- `Overlaps` is true only when the inclusive intervals share at least one code point.
- `IsAdjacentTo` is true when one range ends immediately before the other begins, with overflow-safe boundary handling.
- Equality and hashing are structural over `Start` and `End`.
- `ToString()` is stable and diagnostic, for example `U+0041..U+005A` or `U+0041` for a singleton.

Do not implement adjacency by adding one to `End` without handling `0x10FFFF`; the maximum code point has no adjacent successor in the default universe.

### `CodePointSet`

Conceptual API:

```csharp
sealed class CodePointSet : IReadOnlyCollection<CodePoint>
{
    static CodePointSet Empty { get; }
    static CodePointSet All { get; }

    int RangeCount { get; }
    bool IsEmpty { get; }
    bool Contains(CodePoint value);
    IEnumerable<CodePointRange> Ranges { get; }

    CodePointSet Union(CodePointSet other);
    CodePointSet Intersect(CodePointSet other);
    CodePointSet Subtract(CodePointSet other);
    CodePointSet Complement();
}
```

The exact public API may use `IReadOnlyList<CodePointRange>`, a range enumerator, or a repository-specific collection abstraction. Required semantics:

- `Empty` contains no values.
- `All` contains every code point from `U+000000` through `U+10FFFF`.
- Null arguments are rejected consistently with the repository's public API policy.
- Operations never mutate either operand.
- Operations return canonical sets, including when one operand is empty or already canonical.
- Equality compares canonical ranges, not construction history.
- Hashing is stable for equal sets and must include range boundaries in order.

`IReadOnlyCollection<CodePoint>` enumeration must be ascending. A separate range enumeration is required so callers can inspect or serialize the compact representation without expanding every member.

## Construction

### `CodePointSetBuilder`

Construction from many values or ranges should use a mutable builder and produce an immutable snapshot:

```csharp
sealed class CodePointSetBuilder
{
    void Add(CodePoint value);
    void Add(CodePointRange range);
    void AddRange(IEnumerable<CodePointRange> ranges);
    CodePointSet Build();
}
```

Builder requirements:

- `Add` accepts singleton values.
- `Add` and `AddRange` validate inputs at the boundary.
- `Build` sorts ranges by start and coalesces all overlap and adjacency.
- A builder may be reused after `Build`; later mutations must not change an already-built set.
- `Build` of no values returns `CodePointSet.Empty`.
- Repeated values and duplicate ranges are harmless.
- Enumeration of an input sequence should not be required to be sorted.

The builder may sort a temporary list, use an interval tree, or use another implementation. The resulting immutable representation and behavior are fixed by this document.

For generated or hand-authored large tables, prefer adding ranges directly rather than adding every code point individually.

## Set Algebra

All operations are over the default universe `U` and preserve canonical representation.

### Union

```text
A union B = values present in A or B
```

Use a linear two-pointer merge over sorted ranges when both operands are canonical. Merge overlapping and adjacent output ranges as they are emitted.

Expected identities:

```text
A union Empty = A
A union A = A
A union Complement(A) = All
```

### Intersection

```text
A intersect B = values present in both A and B
```

Use a linear sweep. For current ranges `a` and `b`, emit `[max(a.Start, b.Start), min(a.End, b.End)]` when the bounds overlap, then advance the range whose end is smaller.

Expected identities:

```text
A intersect Empty = Empty
A intersect All = A
A intersect A = A
```

### Subtraction

```text
A subtract B = values present in A and absent from B
```

Use a linear sweep that can split one left-hand range into multiple output ranges. Avoid integer overflow when advancing past `0x10FFFF`.

Expected identities:

```text
A subtract Empty = A
A subtract A = Empty
A subtract All = Empty
```

### Complement

```text
Complement(A) = U subtract A
```

Complement must include the boundaries correctly:

```text
Complement(Empty) = All
Complement(All) = Empty
Complement(Complement(A)) = A
```

For a range `[start, end]`, emit the gap before it only when `start > 0`, and emit the gap after it only when `end < 0x10FFFF`.

## Membership and Enumeration

### Membership

`Contains` must use binary search over canonical ranges or an equivalent logarithmic lookup. It must not enumerate the set. Complexity should be `O(log r)` for `r = RangeCount`.

Boundary cases are mandatory:

- `U+000000` and `U+10FFFF`.
- Singleton ranges.
- Values immediately before and after a range.
- Surrogate values when represented.
- Empty and full sets.

### Range enumeration

Range enumeration is the primary representation-level operation. It must be allocation-safe for callers that only need intervals and must preserve ascending order.

### Code-point enumeration

Enumerating `CodePointSet.All` must not eagerly allocate approximately 1.1 million `CodePoint` objects. Implement lazy range-by-range enumeration or an equivalent struct enumerator. The enumerator must advance one code point at a time only as requested.

If `CodePoint` is a value type, boxing and iterator allocations should still be avoided where practical, but correctness and non-eager behavior take priority over speculative micro-optimization.

Do not expose a mutable iterator cursor that can modify the set. Multiple enumerators must be independent.

## UTF-16 Boundary

Set membership is code-point membership, not string membership. The set layer must not interpret a surrogate pair as two independent user-visible characters, and it must not decode strings implicitly.

String or UTF-16 APIs that test membership must use the repository's scalar/code-point enumeration policy. They must document what happens for:

- A valid surrogate pair.
- An unpaired high surrogate.
- An unpaired low surrogate.

The recommended policy for this scalar-oriented library is to reject malformed UTF-16 explicitly. A code-point set itself can still represent surrogate code points because set algebra operates over code points, not encoded text validity.

## Generated and Hand-Authored Data

`CodePointSet` must be usable by both generated Unicode data and fixed specification-defined sets.

### Generated Unicode properties

The UCD tooling should emit sorted, coalesced ranges and construct sets without adding every member individually. Generated output must:

- Identify its Unicode version.
- Be deterministic and timestamp-free.
- Contain no overlapping or adjacent ranges.
- Validate every range against the code-point domain.
- Use stable ordering.
- Keep generated storage immutable to library consumers.

General-category and block generation must preserve the distinction between a set of code points and a property lookup. A property table may expose `CodePointSet` values, but `CodePointSet` must not know how a property was sourced.

### XML character sets

XML character sets are specification-defined ranges. They must be transcribed from the relevant XML specification and built from literal ranges, not inferred from a host runtime or a possibly different UCD release. XML 1.0 and XML 1.1 sets must remain separate named definitions when their rules differ.

### Case closure

Simple case closure can use `CodePointSet` because simple folding is one-to-one per input code point. Full case folding is sequence-valued and must not be represented as ordinary membership closure over individual code points.

A closure operation must return a new canonical set, preserve all input members, terminate over the finite universe, and document its folding mode and Unicode version.

## Correctness Invariants

For every `CodePointSet` instance:

1. Every stored range is valid and inclusive.
2. Stored ranges are sorted strictly by start.
3. Stored ranges neither overlap nor touch.
4. Every member of the set is in `U`.
5. Every value in a stored range is a member of the set.
6. No value outside all stored ranges is a member.
7. Range enumeration is ascending and canonical.
8. Code-point enumeration is ascending and complete.
9. Operations do not mutate their operands.
10. Equal sets have equal hashes.
11. Construction order does not affect equality or hash.
12. `A.Intersect(B)` equals `B.Intersect(A)`.
13. `A.Union(B)` equals `B.Union(A)`.
14. `A.Subtract(B)` contains no member of `B`.
15. Double complement returns the original canonical set.

The builder and every set operation should enforce or preserve these invariants. Debug-only assertions are useful, but correctness must not depend solely on assertions that disappear in release builds.

## Complexity Targets

Let `r` and `s` be the number of canonical ranges in two operands:

| Operation | Target complexity | Extra space |
|---|---:|---:|
| `Contains` | `O(log r)` | `O(1)` |
| `Union` | `O(r + s)` | `O(r + s)` |
| `Intersect` | `O(r + s)` | `O(min(r, s))` to `O(r + s)` |
| `Subtract` | `O(r + s)` | `O(r + s)` |
| `Complement` | `O(r)` | `O(r)` |
| Range enumeration | `O(r)` | `O(1)` plus enumerator state |
| Code-point enumeration | `O(n)` for `n` yielded members | Lazy; no eager `O(n)` materialization |

These are targets for the canonical range representation. A future specialized representation may improve them, but it must retain the same observable semantics and must not make common operations depend on Unicode version or host culture.

## Test Plan

Tests must be deterministic and must not require network access.

### Range tests

Cover:

- Valid singleton and multi-value ranges.
- Reversed, out-of-domain, and invalid construction.
- Inclusive `Contains` boundaries.
- Overlap, non-overlap, and adjacency.
- Maximum-boundary adjacency at `0x10FFFF`.
- Structural equality, hashing, and diagnostic formatting.

### Builder tests

Cover:

- Empty build.
- Singleton values.
- Unsorted input.
- Duplicate values and ranges.
- Overlapping ranges.
- Adjacent ranges, including `[0, 5]` plus `[6, 10]`.
- Reuse after `Build` without changing a prior result.
- Rejection of invalid inputs.

### Algebra tests

Use small explicit sets and compare each operation with a reference implementation over a small bounded universe. Also cover the real boundaries `0x000000`, `0x00D7FF`, `0x00E000`, and `0x10FFFF`.

Required identities include:

```text
A union Empty = A
A intersect All = A
A subtract Empty = A
A subtract A = Empty
Complement(Empty) = All
Complement(All) = Empty
Complement(Complement(A)) = A
```

Test ranges that split during subtraction, such as:

```text
A = [10..20]
B = [12..15] union [18..25]
A subtract B = [10..11] union [16..17]
```

Test commutativity of union and intersection, associativity where practical, distributive identities, and De Morgan identities against a bounded reference model.

### Enumeration and performance tests

Verify that:

- Range enumeration returns canonical ascending ranges.
- Code-point enumeration returns every member exactly once.
- Empty enumeration yields no values.
- `All` begins with `U+000000` and ends with `U+10FFFF`.
- A test or code inspection guard prevents eager materialization of `All`.
- Membership remains correct for sets with many disjoint ranges.

### Consumer integration tests

Add focused tests for generated general-category sets, Unicode blocks, XML sets, and simple case closure once those services are implemented. Each consumer must verify both representative values and range boundaries rather than only testing a handful of ASCII characters.

## Implementation Phases

1. Confirm the repository's `CodePoint` validation and UTF-16 policy.
2. Implement and test `CodePointRange`.
3. Implement canonical immutable storage for `CodePointSet`, including `Empty` and `All`.
4. Implement `CodePointSetBuilder` with sorting and coalescing.
5. Implement binary-search membership and lazy range/code-point enumeration.
6. Implement linear-sweep union, intersection, subtraction, and complement.
7. Add structural equality, hashing, formatting, and allocation-sensitive tests.
8. Update UCD generators and XML definitions to emit or consume canonical ranges.
9. Add integration tests for general categories, blocks, XML character sets, and case closure.

Do not hand-optimize before the invariants and algebra tests are passing. Do not introduce a tree or bitmap representation unless profiling demonstrates a need and the canonical range semantics remain visible at the API boundary.

## Decisions That Future Agents Must Not Reverse Accidentally

- The default set universe is all Unicode code points, including surrogate code points.
- `CodePointSet` is immutable; builders are the mutable construction boundary.
- Stored ranges are sorted, disjoint, and coalesced across adjacency.
- Complement is relative to `0..0x10FFFF`, not scalar values or assigned characters.
- Membership is logarithmic over ranges and never scans all members.
- Enumeration is ascending; full-domain enumeration is lazy.
- Set algebra never mutates its operands and always returns canonical sets.
- Generated and hand-authored data must use ranges, not per-code-point expansion.
- UTF-16 decoding belongs at the string/code-point boundary, not inside mathematical set operations.
- Unicode properties, XML rules, and case-folding policy remain consumers of the generic set abstraction.
