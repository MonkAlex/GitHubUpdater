using System;
using System.Net;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.ViewModel;

namespace GitHubUpdater.WPF.Command
{
  public class DownloadFileCommand : BaseCommand
  {
    private DownloadFile file;
    private DownloadedFileViewModel model;

    public override async void Execute(object parameter)
    {
      base.Execute(parameter);

      var content = await file.Download(new Progress<DownloadProgressChangedEventArgs>(Handler));
    }

    private void Handler(DownloadProgressChangedEventArgs args)
    {
      model.Downloaded = args.ProgressPercentage;
      model.DownloadText = string.Format("{0} / {1}", args.BytesReceived.HumanizeByteSize(), args.TotalBytesToReceive.HumanizeByteSize());
    }

    public DownloadFileCommand(DownloadedFileViewModel model, DownloadFile file)
    {
      this.model = model;
      this.file = file;
    }
  }
}