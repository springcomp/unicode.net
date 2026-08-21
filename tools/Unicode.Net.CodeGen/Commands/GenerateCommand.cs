using System.CommandLine;
using Unicode.NET.CodeGen.Generation;
using Unicode.NET.CodeGen.Parsing;

namespace Unicode.NET.CodeGen.Commands;

/// <summary>
/// Parses cached UCD files and emits generated C# source files into
/// <c>src/Unicode.NET/Generated/</c>.
/// </summary>
public static class GenerateCommand
{
  public static Command Build(Option<string> versionOption)
  {
    var cmd = new Command("generate", "Generate C# source files from cached UCD files.");
    cmd.Options.Add(versionOption);
    cmd.SetAction((parseResult) =>
    {
      string version = parseResult.GetValue(versionOption)!;
      Run(version);
    });
    return cmd;
  }

  public static void Run(string version)
  {
    string cacheDir = DownloadCommand.GetCacheDir(version);
    string outputDir = FindOutputDir();
    Directory.CreateDirectory(outputDir);

    Console.WriteLine($"  [parse]  UnicodeData.txt");
    var unicodeData = UnicodeDataParser.Parse(Path.Combine(cacheDir, "UnicodeData.txt"));

    Console.WriteLine($"  [parse]  Blocks.txt");
    var blocks = BlocksParser.Parse(Path.Combine(cacheDir, "Blocks.txt"));

    Console.WriteLine($"  [parse]  CaseFolding.txt");
    var caseFolding = CaseFoldingParser.Parse(Path.Combine(cacheDir, "CaseFolding.txt"));

    Console.WriteLine($"  [parse]  PropertyValueAliases.txt");
    var pvaPath = Path.Combine(cacheDir, "PropertyValueAliases.txt");
    var gcAliases = PropertyValueAliasesParser.ParseGeneralCategoryAliases(pvaPath);
    var scriptAliases = PropertyValueAliasesParser.ParseScriptAliases(pvaPath);

    Console.WriteLine($"  [parse]  Scripts.txt");
    var scripts = ScriptsParser.Parse(Path.Combine(cacheDir, "Scripts.txt"));

    Console.WriteLine($"  [parse]  ScriptExtensions.txt");
    var scriptExtensions = ScriptExtensionsParser.Parse(Path.Combine(cacheDir, "ScriptExtensions.txt"));

    Console.WriteLine($"  [parse]  PropList.txt");
    var propListRecords = BinaryPropertiesParser.Parse(Path.Combine(cacheDir, "PropList.txt"));

    Console.WriteLine($"  [parse]  DerivedCoreProperties.txt");
    var derivedCoreRecords = BinaryPropertiesParser.Parse(Path.Combine(cacheDir, "DerivedCoreProperties.txt"));

    // Merge: PropList + DerivedCoreProperties (Alphabetic comes from DerivedCoreProperties)
    var allBinaryRecords = propListRecords.Concat(derivedCoreRecords).ToList();

    WriteIfChanged(
        Path.Combine(outputDir, $"PropertyAliases.{version}.g.cs"),
        PropertyAliasesGenerator.Generate(gcAliases, scriptAliases, version));

    WriteIfChanged(
        Path.Combine(outputDir, $"GeneralCategories.{version}.g.cs"),
        GeneralCategoryTableGenerator.Generate(unicodeData, version));

    WriteIfChanged(
        Path.Combine(outputDir, $"UnicodeBlocks.{version}.g.cs"),
        BlocksTableGenerator.Generate(blocks, version));

    WriteIfChanged(
        Path.Combine(outputDir, $"CaseFolding.{version}.g.cs"),
        CaseFoldingTableGenerator.Generate(caseFolding, version));

    WriteIfChanged(
        Path.Combine(outputDir, $"Scripts.{version}.g.cs"),
        ScriptsTableGenerator.Generate(scripts, scriptExtensions, version));

    WriteIfChanged(
        Path.Combine(outputDir, $"BinaryProperties.{version}.g.cs"),
        BinaryPropertiesGenerator.Generate(allBinaryRecords, version));

    // Generate Script enum (version-agnostic)
    string scriptEnumPath = Path.Combine(outputDir, "..", "Script.cs");
    WriteIfChanged(scriptEnumPath, ScriptGenerator.Generate(scripts));
  }

  private static void WriteIfChanged(string path, string content)
  {
    if (File.Exists(path))
    {
      string existing = File.ReadAllText(path);
      if (existing == content)
      {
        Console.WriteLine($"  [unchanged] {Path.GetFileName(path)}");
        return;
      }
    }
    File.WriteAllText(path, content);
    Console.WriteLine($"  [written]  {path}");
  }

  private static string FindOutputDir()
  {
    var cwd = Directory.GetCurrentDirectory();

    var dir = new DirectoryInfo(cwd);
    while (dir is not null)
    {
      if (File.Exists(Path.Combine(dir.FullName, "Unicode.NET.sln")))
      {
        return Path.Combine(dir.FullName, "src", "Unicode.NET", "Generated");
      }
      dir = dir.Parent;
    }

    return Path.Combine(cwd, "Generated");
  }
}
