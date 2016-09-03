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
    private static readonly string WorkFolder = AppDomain.CurrentDomain.BaseDirectory;

    public Option Option { get; private set; }

    public event EventHandler<DownloadFile> DownloadStarted; 

    public async Task<bool> HasUpdate()
    {
      var release = await GetLatestRelease();
      return !string.Equals(Option.Version, release.TagName, StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task Download()
    {
      var release = await GetLatestRelease();
      Download(release);
    }

    private void Download(Release release)
    {
      var result = new List<string>();
      var assets = release.Assets;
      if (!string.IsNullOrWhiteSpace(Option.DownloadMask))
      {
        try
        {
          assets = assets.Where(a => Regex.IsMatch(a.Name, Option.DownloadMask, RegexOptions.IgnoreCase)).ToList();
        }
        catch (Exception) { }
      }
      if (!assets.Any())
        return;

      var folder = Option.OutputFolder;
      folder = Environment.ExpandEnvironmentVariables(folder);
      if (!Directory.Exists(folder))
      {
        Directory.CreateDirectory(folder);
      }
      foreach (var asset in assets)
      {
        var dl = new WebClient();
        OnDownloadStarted(new DownloadFile(asset, dl));
        dl.DownloadDataAsync(new Uri(asset.BrowserDownloadUrl));
        dl.DownloadDataCompleted += (sender, args) =>
        {
          DlOnDownloadDataCompleted(folder, asset.Name, args);
        };
      }
    }

    private void DlOnDownloadDataCompleted(string folder, string name, DownloadDataCompletedEventArgs args)
    {
      var path = Path.Combine(folder, name);
      File.WriteAllBytes(path, args.Result);
      if (Option.Unpack)
      {
        var archive = new ZipArchive(path);
        if (string.IsNullOrWhiteSpace(Option.UnpackFolder))
          archive.Unpack(folder);
        else
          archive.Unpack(folder, Option.UnpackFolder);
      }
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

    protected virtual void OnDownloadStarted(DownloadFile e)
    {
      DownloadStarted?.Invoke(this, e);
    }
  }
}
