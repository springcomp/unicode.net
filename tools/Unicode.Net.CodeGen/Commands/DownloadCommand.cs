using System.CommandLine;

namespace Unicode.NET.CodeGen.Commands;

/// <summary>
/// Downloads UCD files for a given Unicode version into the local cache.
/// Files already present are skipped (idempotent).
/// </summary>
public static class DownloadCommand
{
  private static readonly string[] UcdFiles =
  [
      "UnicodeData.txt",
        "SpecialCasing.txt",
        "Blocks.txt",
        "CaseFolding.txt",
        "PropertyValueAliases.txt",
        "Scripts.txt",
        "ScriptExtensions.txt",
        "PropList.txt",
        "DerivedCoreProperties.txt",
    ];

  public static Command Build(Option<string> versionOption)
  {
    var cmd = new Command("download", "Download UCD files for a given Unicode version.");
    cmd.Options.Add(versionOption);
    cmd.SetAction(async (parseResult, ct) =>
    {
      string version = parseResult.GetValue(versionOption)!;
      await RunAsync(version);
    });
    return cmd;
  }

  public static async Task RunAsync(string version)
  {
    string cacheDir = GetCacheDir(version);
    Directory.CreateDirectory(cacheDir);

    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

    foreach (var file in UcdFiles)
    {
      string dest = Path.Combine(cacheDir, file);
      if (File.Exists(dest))
      {
        Console.WriteLine($"  [cached] {file}");
        continue;
      }

      string url = $"https://www.unicode.org/Public/{version}/ucd/{file}";
      Console.WriteLine($"  [download] {url}");

      HttpResponseMessage response;
      try
      {
        response = await http.GetAsync(url);
      }
      catch (HttpRequestException ex)
      {
        Console.Error.WriteLine($"Network error downloading {file}: {ex.Message}");
        throw;
      }

      if (!response.IsSuccessStatusCode)
      {
        string msg = response.StatusCode == System.Net.HttpStatusCode.NotFound
            ? $"Unicode version '{version}' not found at {url} (HTTP 404)."
            : $"HTTP {(int)response.StatusCode} downloading {url}.";
        Console.Error.WriteLine(msg);
        throw new InvalidOperationException(msg);
      }

      await using var fs = File.OpenWrite(dest);
      await response.Content.CopyToAsync(fs);
      Console.WriteLine($"  [saved]  {dest}");
    }
  }

  public static string GetCacheDir(string version)
  {
    string toolsDir = FindToolsDir();
    return Path.Combine(toolsDir, ".cache", version);
  }

  private static string FindToolsDir()
  {
    var cwd = Directory.GetCurrentDirectory();
    if (File.Exists(Path.Combine(cwd, "Unicode.NET.CodeGen.csproj")))
      return cwd;

    var dir = new DirectoryInfo(cwd);
    while (dir is not null)
    {
      var candidate = Path.Combine(dir.FullName, "tools", "Unicode.NET.CodeGen");
      if (Directory.Exists(candidate))
        return candidate;
      dir = dir.Parent;
    }

    return cwd;
  }
}
