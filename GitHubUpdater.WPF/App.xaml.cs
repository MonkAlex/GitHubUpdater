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

    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
      var option = Option.CreateFromArgs();
      if (option.HasError)
        return;

      var download = new DownloadUpdate(option);
      foreach (var file in await download.GetFiles())
      {
        downloadViewModel.DownloadedFiles.Add(new DownloadedFileViewModel(file, option.OutputFolder));
      }
      new MainWindow() { DataContext = downloadViewModel }.Show();
    }
  }
}
