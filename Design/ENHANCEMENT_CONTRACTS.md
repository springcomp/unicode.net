# Unicode.NET Enhancement Contracts

This document approves the public contracts for reusable Unicode functionality. It deliberately excludes XSLT policy and serializer behavior.

## XML 1.0 character sets

`Unicode.NET.Xml.XmlCharacterSets` exposes immutable, specification-defined XML 1.0 fifth-edition sets:

```csharp
XmlCharacterSets.Char
XmlCharacterSets.NameStartChar
XmlCharacterSets.NameChar
XmlCharacterSets.Whitespace
```

It also exposes allocation-free scalar predicates:

```csharp
XmlCharacterSets.IsChar(CodePoint value)
XmlCharacterSets.IsNameStartChar(CodePoint value)
XmlCharacterSets.IsNameChar(CodePoint value)
XmlCharacterSets.IsWhitespace(CodePoint value)
```

The sets use literal XML productions, not UCD-derived properties. `Whitespace` is exactly XML whitespace U+0009, U+000A, U+000D, and U+0020. XML 1.1 remains a separate future API. Sets are shared immutable values; predicates allocate nothing. `CodePoint` construction validates its input, and there are no nullable or version arguments. Surrogate code points are not XML characters or name characters.

## Scalar substitution

The approved generic type is `CodePointSubstitutionMap`, with a one-pass operation such as:

```csharp
var map = new CodePointSubstitutionMap(
    IReadOnlyDictionary<CodePoint, string> replacements);
string result = map.Replace(string value);
```

The final member spelling may follow repository conventions, but the contract is fixed: keys are validated Unicode scalar `CodePoint` values; replacement values are non-null strings preserved exactly; unmapped scalars are copied unchanged; mapped scalars may produce zero, one, or many output code points. Replacement text is never recursively remapped. Valid surrogate pairs are consumed as one scalar. Empty input and empty replacement are valid. Null mapping, null input, null replacement, invalid scalar keys, and unpaired UTF-16 surrogates throw documented argument exceptions. The map is immutable after construction. Replacement necessarily allocates output when output differs; no allocation guarantee is made beyond avoiding per-code-point intermediate strings. This API contains no XSLT declarations, precedence, cycle, character-map, or serializer concepts.

## Case folding

Approved public operations:

```csharp
IReadOnlyList<CodePoint> CaseFolding.Fold(
    CodePoint value,
    CaseFoldingMode mode = CaseFoldingMode.Full,
    CaseFoldingLocale locale = CaseFoldingLocale.Default,
    UnicodeVersion? version = null);

string CaseFolding.Fold(
    string value,
    CaseFoldingMode mode = CaseFoldingMode.Full,
    CaseFoldingLocale locale = CaseFoldingLocale.Default,
    UnicodeVersion? version = null);

bool CaseFolding.CaselessEquals(
    ReadOnlySpan<char> left,
    ReadOnlySpan<char> right,
    CaseFoldingMode mode = CaseFoldingMode.Full,
    CaseFoldingLocale locale = CaseFoldingLocale.Default,
    UnicodeVersion? version = null);
```

`Simple` and `Full` are explicit modes. Full mode is sequence-valued and may expand a scalar; simple mode returns exactly one scalar. `Default` locale excludes Turkic (`T`) mappings. `Turkic` is explicit and throws `NotSupportedException` until its tables are implemented; no behavior comes from `CultureInfo`, process locale, or user settings. A null version means `UnicodeVersion.Current` (currently 15.1.0). An unregistered version throws `NotSupportedException`; versions are never silently substituted.

Scalar and string `Fold` reject malformed UTF-16 with `ArgumentException`; valid surrogate pairs are consumed as one scalar. Null string input throws `ArgumentNullException`. Empty input returns an empty string. Folding is context-independent, is not normalization, and never performs casing or collation. String folding allocates one result string; scalar folding may allocate a sequence result for full mappings. `CaselessEquals` is semantically equivalent to comparing folded strings, but may stream folded scalar sequences and must not promise zero allocation until an implementation provides that guarantee. Span inputs have no null state and malformed UTF-16 throws `ArgumentException`.

## Explicit non-goals

Unicode.NET does not provide normalization, collation, titlecasing, locale-tailored case mapping, XML 1.1 sets, an XSLT component model, XSLT character-map compilation, `use-character-maps`, precedence, serializer configuration, or serialization policy. `CaseMapping` remains separate from `CaseFolding`.
