using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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

    public string Target
    {
      get { return target; }
      set
      {
        target = value;
        OnPropertyChanged();
      }
    }

    public async Task<bool> Download(IProgress<DownloadProgressChangedEventArgs> args)
    {
      return await file.Download(args, Target);
    }

    public void Handler(DownloadProgressChangedEventArgs args)
    {
      Downloaded = args.ProgressPercentage / 100.00;
      DownloadText = string.Format("{0} / {1}",
        args.BytesReceived.HumanizeByteSize(), args.TotalBytesToReceive.HumanizeByteSize());
    }

    public DownloadedFileViewModel(DownloadFile file, string targetFolder, string unpackFolder)
    {
      this.file = file;
      //this.Unpack = new UnpackCommand(this);
      this.Target = Path.Combine(targetFolder, file.Name);
      this.TargetFolder = targetFolder;
      this.Subfolder = unpackFolder;
    }

    public string TargetFolder { get; set; }

    public string Subfolder { get; set; }
  }
}