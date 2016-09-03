using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.ViewModel;

namespace GitHubUpdater.WPF
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private DownloadViewModel downloadViewModel = new DownloadViewModel();

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      var option = Option.CreateFromArgs();
      if (option.HasError)
        return;

      var download = new DownloadUpdate(option);
      download.DownloadStarted += DownloadOnDownloadStarted;
      new MainWindow() { DataContext = downloadViewModel }.Show();
      Task.Run(() => download.Download());
    }

    private void DownloadOnDownloadStarted(object sender, DownloadFile f)
    {
      var fileModel = new DownloadedFileViewModel(f.Uri);
      fileModel.Name = f.Name;
      f.WebClient.DownloadProgressChanged += (o, args) =>
      {
        fileModel.Downloaded = args.ProgressPercentage;
        fileModel.DownloadText = string.Format("{0} / {1}", args.BytesReceived, args.TotalBytesToReceive);
      };

      Current.Dispatcher.InvokeAsync(() => downloadViewModel.DownloadedFiles.Add(fileModel));
    }
  }
}
