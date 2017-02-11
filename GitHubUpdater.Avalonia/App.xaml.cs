using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Logging.Serilog;
using Avalonia.Themes.Default;
using Avalonia.Markup.Xaml;
using GitHubUpdater.Avalonia.View;
using GitHubUpdater.Avalonia.ViewModel;
using GitHubUpdater.Shared;
using Serilog;

namespace GitHubUpdater.Avalonia
{
  class App : Application
  {

    private static Option option;

    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
      base.Initialize();
    }

    static void Main(string[] args)
    {
      InitializeLogging();
      AppBuilder.Configure<App>()
          .UsePlatformDetect()
          .SetupWithoutStarting();

      /*
      if (!args.Any())
      {
        Current.Exit();
      }
      */

      // Current.DispatcherUnhandledException += (o, args) => o.Error(args.Exception);

      option = Option.CreateFromArgs();
      var viewModel = new UpdateViewModel(option);

      Current.Debug($"App started, silent mode = {option.Silent}. Args - '{string.Join(" ", args)}'");
      Current.OnExit += (o, exitArgs) => Current.Debug("App closed.");

      if (option.Silent)
      {
        viewModel.Start.Execute(Current);
      }
      else
      {
        var window = new Update();
        window.DataContext = viewModel;
        window.Show();
        viewModel.Start.Execute(Current);
        Current.Run(window);
      }
    }

    public static void AttachDevTools(Window window)
    {
#if DEBUG
            DevTools.Attach(window);
#endif
    }

    private static void InitializeLogging()
    {
#if DEBUG
            SerilogLogger.Initialize(new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Trace(outputTemplate: "{Area}: {Message}")
                .CreateLogger());
#endif
    }

    public static void ExceptionHandlerOnHandler(object sender, UnpackExceptionEventArgs e)
    {
      if (option == null || option.Silent)
      {
        sender.Error(e.Exception);
        e.Handled = UpdateExceptionReaction.Abort;
        return;
      }

      /*
      var handled = global::Avalonia.Threading.Dispatcher.UIThread.InvokeTaskAsync(() =>
        AbortRetryIgnore.ShowDialog(Current.Windows.OfType<Window>().LastOrDefault(), e.Exception.Message));
      e.Handled = UpdateExceptionReaction.GetAll().Single(r => r.Value == handled.Value);
      */
    }

    public static void ExceptionHandlerOnHandler(object sender, DownloadExceptionEventArgs e)
    {
      if (option == null || option.Silent)
      {
        sender.Error(e.Exception);
        e.Handled = DownloadExceptionReaction.Abort;
        return;
      }

      /*
      var handled = Current.Dispatcher.Invoke(() =>
        AbortRetryIgnore.ShowDialog(Current.Windows.OfType<Window>().LastOrDefault(), e.Exception.Message));
      e.Handled = DownloadExceptionReaction.GetAll().Single(r => r.Value == handled.Value);
      */
    }
  }
}
