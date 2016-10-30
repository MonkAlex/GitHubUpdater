using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHubUpdater.Shared.Archive;
using Octokit;

namespace GitHubUpdater.Shared
{
  public class DownloadUpdate
  {
    public Option Option { get; }

    public async Task<bool> HasUpdate()
    {
      var release = await GetLatestRelease().ConfigureAwait(false);
      return !string.Equals(Option.Version, release.TagName, StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<IQueryable<DownloadFile>> GetFiles()
    {
      var release = await GetLatestRelease().ConfigureAwait(false);
      return GetFiles(release);
    }

    private IQueryable<DownloadFile> GetFiles(Release release)
    {
      var assets = release.Assets.AsQueryable();
      if (!string.IsNullOrWhiteSpace(Option.DownloadMask))
      {
        try
        {
          var regex = new Regex(Option.DownloadMask, RegexOptions.IgnoreCase);
          assets = assets.Where(a => regex.IsMatch(a.Name));
        }
        catch (Exception) { }
      }

      return assets.Select(a => new DownloadFile(a));
    }

    private Task<Release> GetLatestRelease()
    {
      var client = new GitHubClient(new ProductHeaderValue(string.Format("MonkAlex-{0}-{1}", Option.RepositoryId, Option.Version)));
      return client.Repository.Release.GetLatest(Option.RepositoryId);
    }

    public DownloadUpdate(Option option)
    {
      this.Option = option;
    }
  }
}
