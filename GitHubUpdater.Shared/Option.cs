using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace GitHubUpdater.Shared
{
  public class Option
  {
    [Option("repositoryId", Required = true, HelpText = "RepoId from https://api.github.com/repos/{owner}/{repo}.")]
    public int RepositoryId { get; set; }

    [Option("version", Required = false, HelpText = "Current app version. If null, redownload.")]
    public string Version { get; set; }

    [Option("mask", Required = false, HelpText = "Download file mask in regex pattern format. For example - \"win64.zip$\".")]
    public string DownloadMask { get; set; }

    [Option("outputFolder", Required = true, HelpText = "Folder to save update content. Absoulte path, for example - \"C:\\App\\\".")]
    public string OutputFolder { get; set; }

    [Option("unpack", Required = false, DefaultValue = false, HelpText = "Set \"true\", if content packed to zip archive.")]
    public bool Unpack { get; set; }

    [Option("unpackFolder", Required = false, HelpText = "If archive content in subfolder - set this name.")]
    public string UnpackFolder { get; set; }

    [ParserState]
    public IParserState ParserState { get; set; }

    public bool HasError { get { return this.ParserState != null && this.ParserState.Errors.Any(); } }

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
      Parser.Default.ParseArguments(args, options);
      return options;
    }

    public Option()
    {
      
    }
  }
}
