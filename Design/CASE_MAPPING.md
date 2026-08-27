# Unicode Default Case Mapping

## Purpose

This document is the implementation contract for whole-string Unicode Default
Case Conversion. It defines the deliberately small public surface while keeping
Unicode Character Database (UCD) precedence, mapping expansion, and contextual
rules inside the `CaseMapping` module.

Case mapping is not case folding, normalization, titlecasing, or
locale-tailored casing. The operation is locale-insensitive and follows the
Unicode Default Case Algorithms for the selected Unicode release.

## Public API

The only public operations in this feature are:

```csharp
public static string ToLower(string value, UnicodeVersion? version = null);
public static string ToUpper(string value, UnicodeVersion? version = null);
```

`value` must not be null; both methods throw `ArgumentNullException` for a
null value. A null `version` selects `UnicodeVersion.Current`. A version that
is not registered by the library throws `NotSupportedException`; it must not
silently fall back to another release. `UnicodeVersion.Current` remains
`UnicodeVersion.V15_1_0`; `UnicodeVersion.V16_0_0` may be selected explicitly.

The methods accept a UTF-16 string representing a sequence of Unicode scalar
values. Valid surrogate pairs are decoded and mapped as one scalar. A lone
high or low surrogate is invalid input and throws `ArgumentException`. An empty
string is returned unchanged.

## Normative sources and version integrity

The implementation uses these normative sources:

1. Unicode Standard, Section 3.13, *Default Case Algorithms*.
2. UAX #44, *Unicode Character Database*, for file formats, properties, and
   release/version conventions.
3. The version-matched `UnicodeData.txt`, `SpecialCasing.txt`, and
   `DerivedCoreProperties.txt` files.

All three data files, generated tables, and runtime behavior for a request must
come from one Unicode release. Files from different releases must never be
mixed. The runtime library consumes committed generated data; it does not
download UCD files or read source data at runtime.

`UnicodeData.txt` supplies the ordinary simple uppercase/lowercase mappings.
`SpecialCasing.txt` supplies full mappings and the default (unconditional) and
contextual rules. `DerivedCoreProperties.txt` supplies the properties needed by
the default contextual algorithms, including `Cased` and `Case_Ignorable`.
Absent mappings are identity mappings.

## Default mapping semantics

For each input scalar, the selected operation applies the Unicode default
mapping for the selected version. A mapping is not restricted to one output
scalar: one input scalar may produce one or multiple output scalars.
The result is assembled in input order and encoded as UTF-16.

For mappings with conditions, conditions are evaluated against the **original
input scalar sequence**, not against partially mapped output. Context is
therefore stable even when an earlier mapping expands or changes case. The
implementation must support every default condition specified by the selected
`SpecialCasing.txt` release and Unicode Section 3.13; it must not apply
language-specific conditions.

In particular, lowercase conversion implements the `Final_Sigma` rule:
Greek capital sigma maps to U+03C2 when it is preceded by a cased letter with
only case-ignorable characters between and is not followed by a cased letter
(with case-ignorable characters allowed between); otherwise it maps to
U+03C3. The cased and case-ignorable tests use the original input sequence.

Default mappings are locale-insensitive. Turkish, Azerbaijani, Lithuanian, and
other locale tailorings are out of scope and must not be inferred from the
process, thread, or user culture.

The returned text is **not normalized**. No NFC, NFD, NFKC, NFKD, or other
normalization or compatibility transformation is performed, before or after
case mapping.

## Deliberate boundaries

- **Case mapping** converts text to lower or upper case; **case folding**
  canonicalizes text for caseless matching and has different data and rules.
- **Simple mapping** is one-to-one per scalar; this contract requires full
  string mappings, including one-to-many expansions.
- **Titlecasing** has word-boundary and titlecase behavior and is not exposed.
- **Normalization** composes or decomposes sequences and is not part of this
  operation.
- **Locale tailoring** changes default results for particular languages and is
  not supported by these methods.

No scalar-level, simple-mapping, titlecase, locale, or normalization operation
is added to the public interface by this contract. Changes to `CaseFolding`,
`CaseClosure`, or Xslt.NET are out of scope.
