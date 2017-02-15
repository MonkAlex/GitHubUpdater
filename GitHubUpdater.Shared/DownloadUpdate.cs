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

    public string ProductName { get; private set; }

    public string Version { get; private set; }

    public async Task<bool> HasUpdate()
    {
      try
      {
        var release = await GetLatestRelease().ConfigureAwait(false);
        return !string.Equals(Option.Version, release.TagName, StringComparison.InvariantCultureIgnoreCase);
      }
      catch (Exception ex)
      {
        this.Debug(ex);
        return false;
      }
    }

    public async Task<IQueryable<DownloadFile>> GetFiles()
    {
      try
      {
        var release = await GetLatestRelease().ConfigureAwait(false);
        if (!string.Equals(Option.Version, release.TagName, StringComparison.InvariantCultureIgnoreCase))
          return GetFiles(release);
        this.Debug("New version not found.");
      }
      catch (Exception ex)
      {
        this.Debug(ex);
      }
      return Enumerable.Empty<DownloadFile>().AsQueryable();
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
        catch (Exception ex)
        {
          this.Debug(ex);
        }
      }

      return assets.Select(a => new DownloadFile(a, release.TagName));
    }

    private async Task<Release> GetLatestRelease()
    {
      var client = new GitHubClient(new ProductHeaderValue(string.Format("MonkAlex-{0}-{1}", Option.RepositoryId, Option.Version)));
      var repo = await client.Repository.Get(Option.RepositoryId);
      this.ProductName = repo.Name;
      var release = await client.Repository.Release.GetLatest(Option.RepositoryId);
      this.Version = release.TagName;
      return release;
    }

    public DownloadUpdate(Option option)
    {
      this.Option = option;
    }
  }
}
