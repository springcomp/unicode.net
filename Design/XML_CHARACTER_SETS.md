# XML Character-Set Contract

## Scope

`XmlCharacterSets` exposes immutable, specification-defined XML 1.0 fifth-edition character sets. It is a reusable XML/XPath/QName validation layer; it is not a Unicode UCD property provider.

Public sets:

```csharp
XmlCharacterSets.Char
XmlCharacterSets.NameStartChar
XmlCharacterSets.NameChar
XmlCharacterSets.Whitespace
```

Public predicates use `CodePoint` values:

```csharp
XmlCharacterSets.IsChar(CodePoint value)
XmlCharacterSets.IsNameStartChar(CodePoint value)
XmlCharacterSets.IsNameChar(CodePoint value)
```

Sets are immutable and use literal XML 1.0 production ranges. `Whitespace` is exactly U+0009, U+000A, U+000D, and U+0020. It is not Unicode `White_Space`. XML 1.1 rules are a separate future contract and must not be merged into these members.

## Validity and boundaries

The sets operate over `CodePoint`, whose domain includes surrogate code points for mathematical set operations. XML `Char`, `NameStartChar`, and `NameChar` predicates return `false` for every surrogate because XML 1.0 productions exclude U+D800..U+DFFF. Supplementary name-start values are limited to U+10000..U+EFFFF; XML `Char` permits through U+10FFFF.

No string decoding occurs in set membership. UTF-16 consumers must enumerate scalar values and reject unpaired surrogates according to the scalar API contract.

## Version and allocation

These sets are specification-defined and do not vary with `UnicodeVersion` or installed .NET Unicode data. Properties return shared immutable instances; reading a set or calling a predicate performs no per-call set allocation. `CodePoint` arguments are passed by value.

## Null, invalid, and unsupported input

The set and predicate APIs accept no nullable arguments. A `CodePoint` is already range-validated by construction; there is no invalid `CodePoint` input. These APIs do not accept Unicode-version arguments and therefore cannot throw for an unsupported version. Invalid UTF-16 is relevant only to separate string-level consumers, which throw `ArgumentException` for an unpaired surrogate.

## Non-goals

This contract does not provide XML parsing, QName parsing, normalization, Unicode `White_Space` resolution, XML 1.1 character rules, or XSLT character-map compilation/serialization.
