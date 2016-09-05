using System;
using System.Windows.Input;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.Command;

namespace GitHubUpdater.WPF.ViewModel
{
  public class DownloadedFileViewModel : BaseViewModel
  {
    private double downloaded;
    private DownloadFile file;
    private string downloadText;

    public Uri Uri { get { return file.Uri; } }

    public double Downloaded
    {
      get { return downloaded; }
      set
      {
        downloaded = value;
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

    public string Name { get { return file.Name; } }

    public ICommand Download { get; }

    public DownloadedFileViewModel(DownloadFile file)
    {
      this.file = file;
      this.Download = new DownloadFileCommand(this, file);
    }
  }
}