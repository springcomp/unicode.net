# Unicode.NET.CodeGen

CLI tool that downloads Unicode Character Database (UCD) files and generates C# source
files for the `Unicode.NET` library.

## Usage

### Download + generate in one step

```sh
dotnet run --project tools/Unicode.NET.CodeGen -- update --version 15.1.0
```

### Download only

```sh
dotnet run --project tools/Unicode.NET.CodeGen -- download --version 15.1.0
```

### Generate only (from cached files)

```sh
dotnet run --project tools/Unicode.NET.CodeGen -- generate --version 15.1.0
```

## Cache

Downloaded UCD files are cached under:

```
tools/Unicode.NET.CodeGen/.cache/{version}/
```

Re-running the same command with the cache populated skips all downloads.

## Output

Generated files are written to `src/Unicode.NET/Generated/`:

| File | Contents |
| --- | --- |
| `GeneralCategories.{version}.g.cs` | One `CodePointSet` per Unicode general category (including `Cn` for unassigned), plus a `GetGeneralCategory(CodePoint)` lookup. |
| `UnicodeBlocks.{version}.g.cs` | Readonly list of `(CodePointRange Range, string Name)` entries for all Unicode blocks. |
| `CaseFolding.{version}.g.cs` | `SimpleMap` dictionary (C+S records) mapping code points to their simple case-fold targets. |

Generation is deterministic — running twice against the same cached files produces byte-identical output.

## How to bump the Unicode version (Maintainer Workflow)

To add/update support for a new Unicode version (e.g. 17.0.0):

1. Run the update tool for the new version:

   ```sh
   dotnet run --project tools/Unicode.NET.CodeGen -- update --version 17.0.0
   ```

2. Inspect the generated files in `src/Unicode.NET/Generated/` and review the git diff for correctness and completeness.

3. Register the new version in `src/Unicode.NET/UnicodeVersion.cs`:
   - Add a new static field (`public static readonly UnicodeVersion V17_0_0 = new(17, 0, 0);`).
   - Add a new entry to the version registry in `TryGetTables`, mapping to the generated version's tables.
   - Update `Latest` if this should be the default.
   - Add the version to `SupportedVersions` in `UnicodeVersionInfo.cs`.

4. Run the full test suite:

   ```sh
   dotnet test Unicode.NET.sln
   ```

   - Fix any tests that assumed the previous latest version.
   - Ensure all tests pass (including optional CLI parser/test projects).

5. Commit the changes with a message like: `Add Unicode {version} support, update version metadata and docs`.

After these steps, the library and CLI will fully support the new Unicode version, with clear provenance metadata.
