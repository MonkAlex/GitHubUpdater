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
        Close(OpenWebsite(ExitCodes.AnyVersionNotFound));
      }

      var lastVersion = versions
        .Select(v => new {path = v, version = new Version(Path.GetFileName(v))})
        .OrderByDescending(c => c.version)
        .First();

      Shared.Log.Debug(typeof(Program), $"Version {lastVersion.version} selected.");

      var selfupdate = $"--fromFile=\"{directory}default.config\" " +
                       $"--version=\"{lastVersion.version}\" " +
                       $"--silent " +
                       $"--outputFolder=\"{directory}";
      var selfThread = new Thread(() =>
      {
        InitVersion(lastVersion.path, new[] {selfupdate});
        Shared.Log.Debug(typeof(Program), $"Selfupdate runned...");
      });
      selfThread.Start();
      var code = InitVersion(lastVersion.path, args);
      Close(code, selfThread);
    }

    private static ExitCodes OpenWebsite(ExitCodes code)
    {
      Process.Start(@"https://github.com/MonkAlex/GitHubUpdater/releases/latest");
      return ExitCodes.WebsiteOpened | code;
    }

    private static void Close(ExitCodes code, Thread thread = null)
    {
      if (thread != null)
        thread.Join();

      Environment.Exit((int) code);
    }

    private static ExitCodes InitVersion(string lastVersion, string[] args)
    {
      try
      {
        var canBeStarted = Directory.GetFiles(lastVersion, "*.exe");
        if (!canBeStarted.Any())
        {
          return OpenWebsite(ExitCodes.AnyExeFileNotFound);
        }

        if (canBeStarted.Length == 1)
        {
          var process = Process.Start(canBeStarted[0], string.Join(" ", args));
          process.WaitForInputIdle();
          return ExitCodes.None;
        }

        // TODO : Докрутить какую то фигню, чтобы сама запускала консольный или WPF варианты.
      }
      catch (Exception ex)
      {
        Shared.Log.Error(typeof(Program), ex);
      }
      return ExitCodes.None;
    }
  }
}
