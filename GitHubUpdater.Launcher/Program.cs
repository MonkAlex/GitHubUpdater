using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubUpdater.Launcher
{
  class Program
  {
    static void Main(string[] args)
    {
      var directory = AppDomain.CurrentDomain.BaseDirectory;
      Shared.Log.Debug(typeof(Program), $"App started form {directory}. Args - '{string.Join(" ", args)}'");
      var versions = Directory.GetDirectories(directory);
      if (!versions.Any())
      {
        OpenWebsite(ExitCodes.AnyVersionNotFound);
        return;
      }

      var lastVersion = versions
        .Select(v => new {path = v, version = new Version(Path.GetFileName(v))})
        .OrderBy(c => c.version)
        .First();

      Shared.Log.Debug(typeof(Program), $"Version {lastVersion.version} selected.");

      var selfupdate = $"--fromFile=\"{directory}default.config\" " +
                       $"--version=\"{lastVersion.version}\" " +
                       $"--silent " +
                       $"--outputFolder=\"{directory}%version%";
      new Thread(() =>
      {
        InitVersion(lastVersion.path, new[] {selfupdate});
        Shared.Log.Debug(typeof(Program), $"Selfupdate runned...");
      }).Start();
      InitVersion(lastVersion.path, args);
    }

    private static void OpenWebsite(ExitCodes code)
    {
      Process.Start(@"https://github.com/MonkAlex/GitHubUpdater/releases/latest");
      var iCode = ExitCodes.WebsiteOpened | code;
      Close(iCode);
    }

    private static void Close(ExitCodes code)
    {
      Environment.Exit((int) code);
    }

    private static void InitVersion(string lastVersion, string[] args)
    {
      try
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
      catch (Exception ex)
      {
        Shared.Log.Error(typeof(Program), ex);
      }
    }
  }
}
