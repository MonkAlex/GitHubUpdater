using System;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace GitHubUpdater.Shared
{
  public class DownloadFile
  {
    public Uri Uri { get; }

    public string Name { get; }

    public int Size { get; }

    public async Task<byte[]> Download()
    {
      return await Download(null).ConfigureAwait(false);
    }

    public async Task<byte[]> Download(IProgress<DownloadProgressChangedEventArgs> progress)
    {
      var webClient = new WebClient();
      if (progress != null)
        webClient.DownloadProgressChanged += (sender, args) => progress.Report(args);
      return await webClient.DownloadDataTaskAsync(this.Uri).ConfigureAwait(false);
    }

    public DownloadFile(ReleaseAsset asset)
    {
      this.Uri = new Uri(asset.BrowserDownloadUrl);
      this.Name = asset.Name;
      this.Size = asset.Size;
    }
  }
}