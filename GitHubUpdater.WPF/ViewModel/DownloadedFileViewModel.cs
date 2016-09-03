using System;

namespace GitHubUpdater.WPF.ViewModel
{
  public class DownloadedFileViewModel : BaseViewModel
  {
    private double downloaded;
    private string name;
    private string downloadText;
    public Uri Uri { get; }

    public double Downloaded
    {
      get { return downloaded; }
      set
      {
        downloaded = value;
        OnPropertyChanged();
      }
    }

    public string Name
    {
      get { return name; }
      set
      {
        name = value;
        OnPropertyChanged();
      }
    }

    public string DownloadText
    {
      get { return downloadText; }
      set
      {
        downloadText = value;
        OnPropertyChanged();
      }
    }

    public DownloadedFileViewModel(Uri uri)
    {
      this.Uri = uri;
    }

    public DownloadedFileViewModel(string uri) : this(new Uri(uri))
    {
    }
  }
}