using System;
using System.IO;
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

    public async Task<bool> Download(string target)
    {
      return await Download(null, target).ConfigureAwait(false);
    }

    public async Task<bool> Download(IProgress<DownloadProgress> progress, string target)
    {
      try
      {
        var webClient = new WebClient();
        if (progress != null)
          webClient.DownloadProgressChanged += (sender, args) =>
          {
            var state = new DownloadProgress
            {
              ProgressPercentage = args.ProgressPercentage,
              BytesReceived = args.BytesReceived,
              TotalBytesToReceive = args.TotalBytesToReceive
            };
            progress.Report(state);
          };
        var content = await webClient.DownloadDataTaskAsync(this.Uri).ConfigureAwait(false);

        using (var targetFile = File.OpenWrite(target))
        {
          await targetFile.WriteAsync(content, 0, content.Length);
        }
      }
      catch (Exception e)
      {
        return false;
      }

      return true;
    }

    public DownloadFile(ReleaseAsset asset)
    {
      this.Uri = new Uri(asset.BrowserDownloadUrl);
      this.Name = asset.Name;
      this.Size = asset.Size;
    }
  }
}