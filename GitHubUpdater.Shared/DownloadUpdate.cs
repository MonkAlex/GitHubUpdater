using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace GitHubUpdater.Shared
{
  public static class DownloadUpdate
  {
    private const string UserAgent = "MonkAlex-MangaReader";
    private static readonly string WorkFolder = AppDomain.CurrentDomain.BaseDirectory;

    public async static Task<bool> HasUpdate(int repositoryId, string tagName)
    {
      var release = await GetLatestRelease(repositoryId);
      return !string.Equals(tagName, release.TagName, StringComparison.InvariantCultureIgnoreCase);
    }

    public async static Task<List<string>> Download(int repositoryId)
    {
      var release = await GetLatestRelease(repositoryId);
      return Download(release);
    }

    private static List<string> Download(Release release)
    {
      var result = new List<string>();
      Parallel.ForEach(release.Assets, (asset) =>
      {
        var dl = new WebClient();
        var data = dl.DownloadData(asset.BrowserDownloadUrl);
        var path = Path.Combine(WorkFolder, asset.Name);
        File.WriteAllBytes(path, data);
        result.Add(path);
      });
      return result;
    }

    private static Task<Release> GetLatestRelease(int repositoryId)
    {
      var client = new GitHubClient(new ProductHeaderValue(UserAgent));
      return client.Repository.Release.GetLatest(repositoryId);
    }
  }
}
