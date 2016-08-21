using System;
using System.Threading.Tasks;

namespace GitHubUpdater.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine(string.Join("', '", args));
      if (args.Length < 2)
      {
        Console.WriteLine("Run with repoId and current tagName. RepoId from https://api.github.com/repos/{owner}/{repo}");
        return;
      }

      AsyncMain(int.Parse(args[0]), args[1]).Wait();
    }

    static async Task<bool> AsyncMain(int repositoryId, string tagName)
    {
      var canUpdate = await Shared.DownloadUpdate.HasUpdate(repositoryId, tagName);
      if (canUpdate)
      {
        var files = await Shared.DownloadUpdate.Download(repositoryId);
        foreach (var file in files)
        {
          Console.WriteLine(string.Format("Downloaded: {0}", file));
        }
      }
      return true;
    }
  }
}
