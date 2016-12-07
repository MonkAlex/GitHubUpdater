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
    private void App_OnStartup(object sender, StartupEventArgs e)
    {

      var option = Option.CreateFromArgs();
      var viewModel = new UpdateViewModel(option);

      if (option.Silent)
      {
        viewModel.Start.Execute(this);
      }
      else
      {
        var window = new Update();
        window.DataContext = viewModel;
        window.Show();
      }
    }

    public static void ExceptionHandlerOnHandler(object sender, UnpackExceptionEventArgs e)
    {
      var handled = Current.Dispatcher.Invoke(() =>
        AbortRetryIgnore.ShowDialog(Current.Windows.OfType<Window>().LastOrDefault(), e.Exception.Message));
      e.Handled = UpdateExceptionReaction.GetAll().Single(r => r.Value == handled.Value);
    }

    public static void ExceptionHandlerOnHandler(object sender, DownloadExceptionEventArgs e)
    {
      var handled = Current.Dispatcher.Invoke(() =>
        AbortRetryIgnore.ShowDialog(Current.Windows.OfType<Window>().LastOrDefault(), e.Exception.Message));
      e.Handled = DownloadExceptionReaction.GetAll().Single(r => r.Value == handled.Value);
    }
  }
}
