using System.CommandLine;
using Unicode.NET.CodeGen.Commands;

var versionOption = new Option<string>("--version")
{
  Description = "Unicode version to process (e.g. 16.0.0).",
  Required = true,
};

var rootCommand = new RootCommand("Springcomp Unicode UCD generator — downloads UCD files and generates C# source.");

var downloadCmd = DownloadCommand.Build(versionOption);
var generateCmd = GenerateCommand.Build(versionOption);

var updateCmd = new Command("update", "Download UCD files and generate C# source (download + generate).");
updateCmd.Options.Add(versionOption);
updateCmd.SetAction(async (parseResult, ct) =>
{
  string version = parseResult.GetValue(versionOption)!;
  Console.WriteLine($"[update] Unicode {version}");
  Console.WriteLine("[step 1/2] Download");
  await DownloadCommand.RunAsync(version);
  Console.WriteLine("[step 2/2] Generate");
  GenerateCommand.Run(version);
  Console.WriteLine("[update] Done.");
});

rootCommand.Subcommands.Add(downloadCmd);
rootCommand.Subcommands.Add(generateCmd);
rootCommand.Subcommands.Add(updateCmd);

return await rootCommand.Parse(args).InvokeAsync();
