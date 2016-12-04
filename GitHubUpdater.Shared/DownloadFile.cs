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

    public event EventHandler<DownloadExceptionEventArgs> ExceptionThrowed;

    public async Task<bool> Download(string target)
    {
      return await Download(null, target).ConfigureAwait(false);
    }

    public async Task<bool> Download(IProgress<DownloadProgress> progress, string target)
    {
      try
      {
        return await Shared.ExceptionHandler.TryExecuteAsync(() => DownloadImpl(progress, target),
          Task.FromResult(false),
          OnExceptionThrowed);
      }
      catch (Exception)
      {
        return false;
      }
    }

    private IExceptionReaction OnExceptionThrowed(Exception exception)
    {
      var args = new DownloadExceptionEventArgs(exception, Uri);
      ExceptionThrowed?.Invoke(this, args);
      return args.Handled;
    }

    private async Task<bool> DownloadImpl(IProgress<DownloadProgress> progress, string target)
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
      var content = await webClient.DownloadDataTaskAsync(Uri + "1").ConfigureAwait(false);

      using (var targetFile = File.OpenWrite(target))
      {
        await targetFile.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
      }

      return true;
    }

    public DownloadFile(ReleaseAsset asset)
    {
      Uri = new Uri(asset.BrowserDownloadUrl);
      Name = asset.Name;
      Size = asset.Size;
    }
  }
}