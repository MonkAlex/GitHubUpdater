using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
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
      if (!args.Any())
      {
        var configs = Directory.GetFiles(directory, "*.config", SearchOption.TopDirectoryOnly);
        var defaultConfig = configs.FirstOrDefault();
        if (configs.Length > 1)
          defaultConfig = configs.FirstOrDefault(c => Path.GetFileName(c) == "default.config");

        if (defaultConfig != null)
          args = new[] { defaultConfig };
      }

      // Переэкранировать параметры.
      args = args.Select(a => a.Contains(" ") ? "\"" + a + "\"" : a).ToArray();
      var formatedArgs = string.Join(" ", args);
      Shared.Log.Debug(typeof(Program), $"App started form {directory}. Args - '{formatedArgs}'");
      var versions = Directory.GetDirectories(directory);
      if (!versions.Any())
      {
        Close(OpenWebsite(ExitCodes.AnyVersionNotFound));
      }

      var lastVersion = versions
        .Select(v => new { path = v, version = new Version(Path.GetFileName(v)) })
        .OrderByDescending(c => c.version)
        .First();

      Shared.Log.Debug(typeof(Program), $"Version {lastVersion.version} selected.");

      var selfupdate = $"--repositoryId=\"66179868\" " +
                       $"--unpack " +
                       $"--version=\"{lastVersion.version}\" " +
                       $"--silent " +
                       $"--outputFolder=\"{directory}\"";
      var selfThread = new Thread(() =>
      {
        InitVersion(lastVersion.path, selfupdate);
        Shared.Log.Debug(typeof(Program), $"Selfupdate runned...");
      });
      selfThread.Start();
      var code = InitVersion(lastVersion.path, formatedArgs);
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

      Environment.Exit((int)code);
    }

    private static ExitCodes InitVersion(string lastVersion, string args)
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
          var process = Process.Start(canBeStarted[0], args);
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
