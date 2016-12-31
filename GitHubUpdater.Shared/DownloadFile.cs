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

    public string Tag { get; }

    public event EventHandler<DownloadExceptionEventArgs> ExceptionThrowed;

    public async Task<bool> Download(string target)
    {
      return await Download(null, target).ConfigureAwait(false);
    }

    public async Task<bool> Download(IProgress<DownloadProgress> progress, string target)
    {
      try
      {
        var content = await ExceptionHandler.TryExecuteAsync(() => DownloadImpl(progress), null, OnExceptionThrowed);
        if (content == null)
          return false;

        return await ExceptionHandler.TryExecuteAsync(() => SaveImpl(content, target), false, OnExceptionThrowed);
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

    private async Task<bool> SaveImpl(byte[] content, string target)
    {
      var folder = Path.GetDirectoryName(target);
      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

      using (var targetFile = File.OpenWrite(target))
      {
        await targetFile.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
      }

      return true;
    }

    private async Task<byte[]> DownloadImpl(IProgress<DownloadProgress> progress)
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
      return await webClient.DownloadDataTaskAsync(Uri).ConfigureAwait(false);
    }

    public DownloadFile(ReleaseAsset asset, string tag)
    {
      Uri = new Uri(asset.BrowserDownloadUrl);
      Name = asset.Name;
      Size = asset.Size;
      Tag = tag;
    }
  }
}