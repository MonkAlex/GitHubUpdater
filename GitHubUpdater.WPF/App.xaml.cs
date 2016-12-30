using System.Linq;
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
    private static Option option;

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
      App.Current.DispatcherUnhandledException += (o, args) => o.Error(args.Exception);

      option = Option.CreateFromArgs();
      var viewModel = new UpdateViewModel(option);

      this.Debug($"App started, silent mode = {option.Silent}. Args - '{string.Join(" ", e.Args)}'");
      Current.Exit += (o, args) => this.Debug("App closed.");

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
      if (option == null || option.Silent)
      {
        sender.Error(e.Exception);
        e.Handled = UpdateExceptionReaction.Abort;
        return;
      }

      var handled = Current.Dispatcher.Invoke(() =>
        AbortRetryIgnore.ShowDialog(Current.Windows.OfType<Window>().LastOrDefault(), e.Exception.Message));
      e.Handled = UpdateExceptionReaction.GetAll().Single(r => r.Value == handled.Value);
    }

    public static void ExceptionHandlerOnHandler(object sender, DownloadExceptionEventArgs e)
    {
      if (option == null || option.Silent)
      {
        sender.Error(e.Exception);
        e.Handled = DownloadExceptionReaction.Abort;
        return;
      }

      var handled = Current.Dispatcher.Invoke(() =>
        AbortRetryIgnore.ShowDialog(Current.Windows.OfType<Window>().LastOrDefault(), e.Exception.Message));
      e.Handled = DownloadExceptionReaction.GetAll().Single(r => r.Value == handled.Value);
    }
  }
}
