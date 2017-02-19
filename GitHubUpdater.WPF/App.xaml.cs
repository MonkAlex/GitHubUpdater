using System.Linq;
using System.Threading;
using System.Windows;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.View;
using GitHubUpdater.WPF.ViewModel;
using Ookii.Dialogs.Wpf;

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
      if (!e.Args.Any())
      {
        MessageBox.Show("Run from launcher.");
        Current.Shutdown(0);
      }

      Current.DispatcherUnhandledException += (o, args) => o.Error(args.Exception);

      option = Option.CreateFromArgs();
      var viewModel = new UpdateViewModel(option);

      this.Debug($"App started, silent mode = {option.Silent}. Args - '{string.Join(" ", e.Args)}'");
      Current.Exit += (o, args) => this.Debug("App closed.");

      viewModel.Start.Execute(this);
    }

    public new static void Shutdown()
    {
      var app = (App)Current;
      app.ShutdownInternal();
    }

    private void ShutdownInternal()
    {
      if (Dispatcher.CheckAccess())
        Shutdown(0);
      else
        Dispatcher.Invoke(() => Shutdown(0));
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
