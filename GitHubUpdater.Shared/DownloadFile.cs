using System;
using System.Net;
using Octokit;

namespace GitHubUpdater.Shared
{
  public class DownloadFile
  {
    public Uri Uri { get; }

    public string Name { get; }

    public WebClient WebClient { get; }

    public DownloadFile(ReleaseAsset asset, WebClient webClient)
    {
      this.Uri = new Uri(asset.BrowserDownloadUrl);
      this.Name = asset.Name;
      this.WebClient = webClient;
    }
  }
}