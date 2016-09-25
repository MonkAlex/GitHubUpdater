using System;
using System.IO;
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
    private string target;

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

    public string Target
    {
      get { return target; }
      set
      {
        target = value;
        OnPropertyChanged();
      }
    }

    public DownloadedFileViewModel(DownloadFile file, string targetFolder)
    {
      this.file = file;
      this.Download = new DownloadFileCommand(this, file);
      this.Target = Path.Combine(targetFolder, file.Name);
    }
  }
}