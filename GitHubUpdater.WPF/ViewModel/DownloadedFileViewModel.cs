using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHubUpdater.Shared;
using GitHubUpdater.Shared.Archive;
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

    public async Task<bool> Download(IProgress<DownloadProgress> args)
    {
      file.ExceptionThrowed += GitHubUpdater.WPF.App.ExceptionHandlerOnHandler;
      var task = file.Download(args, Target);
      task.ContinueWith(t =>
      {
        file.ExceptionThrowed -= GitHubUpdater.WPF.App.ExceptionHandlerOnHandler;
      });

      return await task;
    }

    public void DownloadingHandler(DownloadProgress args)
    {
      Downloaded = args.ProgressPercentage / 100.00;
      DownloadText = string.Format("{0} / {1}",
        args.BytesReceived.HumanizeByteSize(), args.TotalBytesToReceive.HumanizeByteSize());
    }

    public void UnpackingHandler(UnpackProgress args)
    {
      Downloaded = args.ProgressPercentage / 100.00;
      DownloadText = string.Format("{0}", args.LastFile);
    }

    public async Task<bool> Unpack(IProgress<UnpackProgress> args)
    {
      if (File.Exists(Target))
      {
        var ext = Path.GetExtension(Target);
        foreach (var type in Generic.CreateAllTypes<IArchive>(Target).Where(t => t.Extension.Contains(ext)))
        {
          if (type.Test())
          {
            type.ExceptionThrowed += GitHubUpdater.WPF.App.ExceptionHandlerOnHandler;
            var task = Task.Run(() => type.Unpack(TargetFolder, UnpackRootSubfolder, args));
            task.ContinueWith(t =>
            {
              type.ExceptionThrowed -= GitHubUpdater.WPF.App.ExceptionHandlerOnHandler;
            });
            try
            {
              return await task;
            }
            catch (Exception)
            {
              return false;
            }
          }
        }
      }
      return false;
    }

    public DownloadedFileViewModel(DownloadFile file, string targetFolder, bool unpackRootSubfolder)
    {
      this.file = file;
      //this.Unpack = new UnpackCommand(this);
      this.Target = Path.Combine(targetFolder, file.Name);
      this.TargetFolder = targetFolder;
      this.UnpackRootSubfolder = unpackRootSubfolder;
    }

    public string TargetFolder { get; set; }

    public bool UnpackRootSubfolder { get; set; }
  }
}