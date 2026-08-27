# Unicode Version

## Current Default: Unicode 15.1.0

`UnicodeVersion.Current` returns `15.1.0`.

Unicode 16.0.0 is available for explicit version selection.

## Input Files

| File | URL |
| ------ | ----- |
| `UnicodeData.txt` | <https://www.unicode.org/Public/15.1.0/ucd/UnicodeData.txt> |
| `Blocks.txt` | <https://www.unicode.org/Public/15.1.0/ucd/Blocks.txt> |
| `CaseFolding.txt` | <https://www.unicode.org/Public/15.1.0/ucd/CaseFolding.txt> |
| `SpecialCasing.txt` | <https://www.unicode.org/Public/15.1.0/ucd/SpecialCasing.txt> |
| `PropertyValueAliases.txt` | <https://www.unicode.org/Public/15.1.0/ucd/PropertyValueAliases.txt> |
| `Scripts.txt` | <https://www.unicode.org/Public/15.1.0/ucd/Scripts.txt> |
| `ScriptExtensions.txt` | <https://www.unicode.org/Public/15.1.0/ucd/ScriptExtensions.txt> |
| `PropList.txt` | <https://www.unicode.org/Public/15.1.0/ucd/PropList.txt> |
| `DerivedCoreProperties.txt` | <https://www.unicode.org/Public/15.1.0/ucd/DerivedCoreProperties.txt> |

## SHA-256 Hashes

```
2fc713e6a31a87c4850a37fe2caffa4218180fadb5de86b43a143ddb4581fb86  UnicodeData.txt
443ee0524a775bf021777c296f5b591b5611c8aef6bc922887d27b0bc13892b5  Blocks.txt
4e55acfdc32825a22e87670e9056a3bf94ad7c5400065778e9e10f8314372bcf  CaseFolding.txt
55a477efd933a52cd27e6a9bf70265bb2d8814af31aab07767abc8eb421f27ef  SpecialCasing.txt
4b7411fc592c4985e5f03643aa0bddfdfd45250ff1790d358926614d20e37652  PropertyValueAliases.txt
0eacb65169ae6eb1d399cd70826b3da15fff19f6f586eecf819b70c83b1d9b32  Scripts.txt
fdfd54237a2c0452ba1060571fd1e58fd46aeecdfda7c5b5be1b716dad755cec  ScriptExtensions.txt
05672956317b6296bc2ec3d6cef1f6452b57ff4f2efc6dc55b0a19277d5fcfd1  PropList.txt
f55d0db69123431a7317868725b1fcbf1eab6b265d756d1bd7f0f6d9f9ee108b  DerivedCoreProperties.txt
```

## Regeneration

Download UCD files:

```bash
dotnet run --project tools/Unicode.NET.CodeGen -- download --version 15.1.0
```

Generate C# source:

```bash
dotnet run --project tools/Unicode.NET.CodeGen -- generate --version 15.1.0
```

Verify all tests pass:

```bash
dotnet test Unicode.NET.sln
```

The release also retains the Unicode 16.0.0 casing inputs for explicit selection:

| File | SHA-256 |
| --- | --- |
| `UnicodeData.txt` | `ff58e5823bd095166564a006e47d111130813dcf8bf234ef79fa51a870edb48f` |
| `SpecialCasing.txt` | `8d5de354eef79f2395a54c9c7dcebbaf3d30fc962d0f85611ea97aa973a0c451` |

Generated files written to `src/Unicode.NET/Generated/`:

- `GeneralCategories.15.1.0.g.cs`
- `UnicodeBlocks.15.1.0.g.cs`
- `CaseFolding.15.1.0.g.cs`
- `CaseMapping.15.1.0.g.cs`
- `PropertyAliases.15.1.0.g.cs`
- `Scripts.15.1.0.g.cs`
- `BinaryProperties.15.1.0.g.cs`

Generated enum (version-agnostic):

- `src/Unicode.NET/Script.cs`

## Version Policy

- Unicode versions are registered in `UnicodeVersion.cs`.
- `UnicodeVersion.Current` is updated when a new version becomes the default.
- Adding a new version requires: downloading UCD files, running the generator, implementing the version tables class, and registering it.

## Registered Versions

| Version | Status |
|---------|--------|
| 15.1.0 | Available |
| 16.0.0 | Available |
