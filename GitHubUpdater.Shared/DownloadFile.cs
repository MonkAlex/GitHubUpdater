using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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

    internal static readonly Lazy<IWebProxy> SystemProxy = new Lazy<IWebProxy>(() =>
    {
      var proxy = WebRequest.GetSystemWebProxy();
      proxy.Credentials = CredentialCache.DefaultCredentials;
      return proxy;
    });

    public async Task<bool> Download(string target, CancellationToken token)
    {
      return await Download(null, target, token).ConfigureAwait(false);
    }

    public async Task<bool> Download(IProgress<DownloadProgress> progress, string target, CancellationToken token)
    {
      try
      {
        var content = await ExceptionHandler.TryExecuteAsync(() => DownloadImpl(progress, token), null, OnExceptionThrowed);
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

      if (File.Exists(target))
      {
        using (var file = new FileStream(target, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
          byte[] onDisk = new byte[file.Length];
          await file.ReadAsync(onDisk, 0, (int)file.Length);
          if (onDisk.SequenceEqual(content))
          {
            this.Debug($"File {target} already exists and no changes found.");
            return true;
          }
        }
      }

      using (var targetFile = File.OpenWrite(target))
      {
        await targetFile.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
      }

      return true;
    }

    private async Task<byte[]> DownloadImpl(IProgress<DownloadProgress> progress, CancellationToken token)
    {
      var webClient = new WebClient { Proxy = SystemProxy.Value };
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
      return await webClient.DownloadDataTaskAsync(Uri).WithCancellation(token).ConfigureAwait(false);
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