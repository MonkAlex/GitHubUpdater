using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GitHubUpdater.Shared
{
  public class Option
  {
    [Option("repositoryId", Required = true, HelpText = "RepoId from https://api.github.com/repos/{owner}/{repo}.")]
    public int RepositoryId { get; set; }

    [Option("version", Required = false, HelpText = "Current app version. If null, redownload.")]
    public string Version { get; set; }

    [Option("downloadMask", Required = false, HelpText = "Download file mask in regex pattern format. For example - \"win64.zip$\".")]
    public string DownloadMask { get; set; }

    [Option("outputFolder", Required = true, HelpText = "Folder to save update content. Absoulte path, for example - \"C:\\App\\\".")]
    public string OutputFolder { get; set; }

    [Option("unpack", Required = false, HelpText = "Set \"true\", if content packed to zip archive.")]
    public bool Unpack { get; set; }

    [Option("unpackRootSubfolder", Required = false, HelpText = "If archive content in subfolder - set true.")]
    public bool UnpackRootSubfolder { get; set; }

    [Option("runAfterUpdate", Required = false, HelpText = "Set path to exe (bat, cmd) file, who runned after update installed. Supported regex pattern format. For example - \"twitch-gui.exe$\".")]
    public string RunAfterUpdate { get; set; }

    [Option("fromFile", Required = false, HelpText = "Set path to config file with default parameters.")]
    public string FromFile { get; set; }

    [Option("silent", Required = false, HelpText = "Set to hide window.")]
    public bool Silent { get; set; }

    public void ProcessOutputFolder(DownloadFile file)
    {
      if (file == null)
        return;

      var versionSubstring = "%version%";
      if (OutputFolder.Contains(versionSubstring))
        OutputFolder = OutputFolder.Replace(versionSubstring, file.Tag);
    }

    public void Save(string path)
    {
      InitJsonSettings();
      var str = JsonConvert.SerializeObject(this);
      File.WriteAllText(path, str, Encoding.UTF8);
    }

    public static Option Load(string path)
    {
      InitJsonSettings();
      if (File.Exists(path))
      {
        var str = File.ReadAllText(path, Encoding.UTF8);
        return JsonConvert.DeserializeObject<Option>(str);
      }
      throw new FileNotFoundException($"File '{path}' not found.");
    }

    private static void InitJsonSettings()
    {
      JsonConvert.DefaultSettings = () =>
        new JsonSerializerSettings
        {
          Formatting = Formatting.Indented,
          Converters = new List<JsonConverter> {new StringEnumConverter(), new VersionConverter()}
        };
    }

    [HelpOption]
    public string GetUsage()
    {
      return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }

    public static Option CreateFromArgs()
    {
      return CreateFromArgs(Environment.GetCommandLineArgs());
    }

    public static Option CreateFromArgs(params string[] args)
    {
      var options = new Option();
      if (!Parser.Default.ParseArguments(args, options))
      {
        if (args.Length == 2)
          options.FromFile = args[1];
      }

      if (File.Exists(options.FromFile))
      {
        var fromfile = Load(options.FromFile);
        Parser.Default.ParseArguments(args, fromfile);
        return fromfile;
      }
      return options;
    }

    public Option()
    {

    }
  }
}
