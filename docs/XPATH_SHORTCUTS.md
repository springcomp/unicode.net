# XPath/XSD shortcut sets

`XPathShortcuts` exposes immutable sets used by XPath/XSD regular-expression consumers:

- `Digit(version)` / `IsDigit(value, version)` implement `\d` as Unicode General_Category `Nd`.
- `Space()` / `IsSpace(value)` implement `\s` as exactly XML/XPath whitespace U+0009, U+000A, U+000D, and U+0020.
- `Word(version)` / `IsWord(value, version)` implement the library's documented `\w` complement of punctuation, separator, and other categories.

UCD-derived `Digit` and `Word` are versioned and default to `UnicodeVersion.Current` (15.1.0). `Space` is specification-defined and does not vary by Unicode version. All returned sets are immutable and shared safely.

`Space` is not Unicode's `White_Space` binary property. Consumers needing that broader UCD property must use:

```csharp
var whiteSpace = UnicodeBinaryProperties.GetPropertySet(
    BinaryProperty.White_Space, UnicodeVersion.Current);
```

The helpers operate on `CodePoint` values and perform no UTF-16 decoding or allocation. `CodePoint` values representing surrogates are ordinary set-membership queries; string consumers must decode UTF-16 with the library's scalar policy before calling them.
