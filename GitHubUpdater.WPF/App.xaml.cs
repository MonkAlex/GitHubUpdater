using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GitHubUpdater.Shared;
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

      e.Handled = Current.Dispatcher.Invoke(() => AbortRetryIgnore<UpdateExceptionReaction>(e.Exception));
    }

    public static void ExceptionHandlerOnHandler(object sender, DownloadExceptionEventArgs e)
    {
      if (option == null || option.Silent)
      {
        sender.Error(e.Exception);
        e.Handled = DownloadExceptionReaction.Abort;
        return;
      }

      e.Handled = Current.Dispatcher.Invoke(() => AbortRetryIgnore<DownloadExceptionReaction>(e.Exception));
    }

    private static T AbortRetryIgnore<T>(Exception ex) where T : Enumeration<T, string>, IExceptionReaction
    {
      var reactions = new List<T>();
      reactions.AddRange(DownloadExceptionReaction.GetAll().OfType<T>());
      reactions.AddRange(UpdateExceptionReaction.GetAll().OfType<T>());

      using (var task = new TaskDialog())
      {
        task.WindowTitle = "Error";
        task.MainIcon = TaskDialogIcon.Error;
        task.Content = ex.Message;
        var exceptionString = ex.ToString();
        task.ExpandedInformation = exceptionString;
        task.FooterIcon = TaskDialogIcon.Information;
        task.Footer = "<a href=\"\">Скопировать в буфер детали исключения</a>";
        task.EnableHyperlinks = true;
        task.HyperlinkClicked += (s, a) => Clipboard.SetText(exceptionString);
        var abort = new TaskDialogButton() { Text = "Отмена" };
        task.Buttons.Add(abort);
        var retry = new TaskDialogButton() { Text = "Повторить" };
        task.Buttons.Add(retry);
        var ignore = new TaskDialogButton() { Text = "Продолжить" };
        task.Buttons.Add(ignore);
        var showed = task.ShowDialog();
        if (showed == abort)
          return reactions.Single(r => r.Value == UpdateExceptionReaction.Abort.Value);
        if (showed == ignore)
          return reactions.Single(r => r.Value == UpdateExceptionReaction.Ignore.Value);
        return reactions.Single(r => r.Value == UpdateExceptionReaction.Retry.Value);
      }
    }
  }
}
