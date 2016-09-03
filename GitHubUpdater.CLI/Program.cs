using System;
using System.Linq;
using System.Threading.Tasks;
using GitHubUpdater.Shared;
using GitHubUpdater.Shared.Archive;

namespace GitHubUpdater.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      var option = Option.CreateFromArgs();
      if (option.HasError)
      {
        Console.WriteLine(option.GetUsage());
        return;
      }
      AsyncMain(option).Wait();
    }

    static async Task<bool> AsyncMain(Option option)
    {
      var du = new DownloadUpdate(option);
      var canUpdate = await du.HasUpdate();
      if (canUpdate)
      {
        //var files = await du.Download();
        //foreach (var file in files)
        //{
        //  Console.WriteLine(string.Format("Downloaded: {0}", file));
        //}
      }
      return true;
    }
  }
}
