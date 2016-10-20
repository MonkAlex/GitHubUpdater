using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubUpdater.Launcher
{
  class Program
  {
    static void Main(string[] args)
    {
      var directory = AppDomain.CurrentDomain.BaseDirectory;
      var versions = Directory.GetDirectories(directory);
      if (!versions.Any())
      {
        OpenWebsite(ExitCodes.AnyVersionNotFound);
        return;
      }

      var lastVersion = versions
        .Select(v => new {path = v, version = new Version(Path.GetFileName(v))})
        .OrderBy(c => c.version)
        .First()
        .path;

      InitVersion(lastVersion, args);
    }

    private static void OpenWebsite(ExitCodes code)
    {
      Process.Start(@"https://github.com/MonkAlex/GitHubUpdater/releases/latest");
      var iCode = ExitCodes.WebsiteOpened | code;
      Close(iCode);
    }

    private static void Close(ExitCodes code)
    {
      Environment.Exit((int)code);
    }

    private static void InitVersion(string lastVersion, string[] args)
    {
      var canBeStarted = Directory.GetFiles(lastVersion, "*.exe");
      if (!canBeStarted.Any())
      {
        OpenWebsite(ExitCodes.AnyExeFileNotFound);
        return;
      }

      if (canBeStarted.Length == 1)
      {
        var process = Process.Start(canBeStarted[0], string.Join(" ", args));
        process.WaitForInputIdle();
        Close(ExitCodes.None);
      }

      // TODO : Докрутить какую то фигню, чтобы сама запускала консольный или WPF варианты.
    }
  }
}
