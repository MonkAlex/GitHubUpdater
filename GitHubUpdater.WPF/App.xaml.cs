using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.View;
using GitHubUpdater.WPF.ViewModel;

namespace GitHubUpdater.WPF
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
      var window = new Update();

      var option = Option.CreateFromArgs();
      if (option.HasError)
      {
        var text = new StringBuilder();
        foreach (var error in option.ParserState.Errors)
        {
          if (error.BadOption.ShortName.HasValue)
          {
            text.Append('-');
            text.Append(error.BadOption.ShortName);
          }
          if (!string.IsNullOrWhiteSpace(error.BadOption.LongName))
          {
            text.Append("--");
            text.Append(error.BadOption.LongName);
          }
          if (error.ViolatesRequired)
            text.Append(" is requred");
          if (error.ViolatesFormat)
            text.Append(" bad formated");
          if (error.ViolatesMutualExclusiveness)
            text.Append(" is violates mutual exclusiveness");
          text.AppendLine();
        }
        MessageBox.Show(window, text.ToString(), "Command not parsed", MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(-1);
      }
      /*
            var download = new DownloadUpdate(option);
            foreach (var file in await download.GetFiles())
            {
              downloadViewModel.DownloadedFiles.Add(new DownloadedFileViewModel(file, option.OutputFolder, option.UnpackFolder));
            }
            */

      ExceptionHandler.Handler += ExceptionHandlerOnHandler;

      var viewModel = new UpdateViewModel(option);
      window.DataContext = viewModel;
      window.Show();
    }

    private void ExceptionHandlerOnHandler(object sender, ExceptionEventArgs args)
    {
      var button = AbortRetryIgnore.ShowDialog(Current.Windows.OfType<Window>().LastOrDefault(), args.Exception.Message);
      args.Handled = button;
    }
  }
}
