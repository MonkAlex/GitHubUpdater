using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.ViewModel;
using Ookii.Dialogs.Wpf;

namespace GitHubUpdater.WPF.Command
{
  public class GetRepositoryFilesCommand : BaseCommand
  {
    private UpdateViewModel model;

    private ProgressDialog dialog;

    private void UpdateDialogTexts(string text, string description)
    {
      dialog?.ReportProgress((int) (model.Percent*100), text, description);
    }

    public override void Execute(object parameter)
    {
      base.Execute(parameter);
      Execute();
    }

    public async Task Execute()
    {
      try
      {
        var download = new DownloadUpdate(model.Option);
        var hasUpdate = await download.HasUpdate();
        if (!hasUpdate)
        {
          this.Debug("Updates not found.");
          App.Shutdown();
          return;
        }

        model.ProgressState = ProgressState.Normal;
        model.State = ConvertState.Started;
        var token = new CancellationTokenSource();

        if (!model.Option.Silent)
        {
          ExecuteInUI(download, token);
        }

        if (model.Option.Silent)
        {
          await InternalExecute(download, token.Token);
        }
      }
      catch (Exception ex)
      {
        this.Debug(ex);
        model.ProgressState = ProgressState.Error;
      }
      if (model.Option.Silent)
        ProgressOnRunWorkerCompleted(this, null);
    }

    private void ExecuteInUI(DownloadUpdate download, CancellationTokenSource token)
    {
      var taskDialog = new TaskDialog();
      taskDialog.WindowTitle = download.ProductName;
      taskDialog.Content = "Перед обновлением закройте приложение.";
      if (string.IsNullOrWhiteSpace(model.Option.Version))
      {
        taskDialog.MainInstruction = string.Format("Установить {0} версии {1}?",
          download.ProductName, download.Version);
      }
      else
      {
        taskDialog.MainInstruction = string.Format("Установить обновление для {2}{3}с {1} до {0}?",
          download.Version, model.Option.Version, download.ProductName, Environment.NewLine);
        Version oldVersion;
        if (Version.TryParse(model.Option.Version, out oldVersion))
        {
          Version newVersion;
          if (Version.TryParse(download.Version, out newVersion))
          {
            if (oldVersion.CompareTo(newVersion) > 0)
            {
              taskDialog.Content += Environment.NewLine;
              taskDialog.Content += "Доступная версия ниже установленной.";
            }
          }
        }
      }
      var yes = new TaskDialogButton(ButtonType.Yes);
      var no = new TaskDialogButton(ButtonType.No);
      taskDialog.Buttons.Add(yes);
      taskDialog.Buttons.Add(no);
      if (taskDialog.ShowDialog() != yes)
      {
        this.Debug("User cancel update.");
        App.Shutdown();
      }

      dialog = new ProgressDialog();
      dialog.WindowTitle = download.ProductName;
      dialog.DoWork += (s, args) =>
      {
        InternalExecute(download, token.Token).Wait(token.Token);
      };

      dialog.ProgressChanged += (sender, args) =>
      {
        if (dialog.CancellationPending)
          token.Cancel();
      };

      model.PropertyChanged += (s, args) =>
      {
        try
        {
          if (dialog.IsBusy && args.PropertyName == nameof(model.Percent))
            dialog.ReportProgress((int)(model.Percent * 100));
        }
        catch (Exception ex)
        {
          this.Debug(ex);
        }
      };
      dialog.ShowDialog();
      dialog.RunWorkerCompleted += ProgressOnRunWorkerCompleted;
    }

    private void ProgressOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
    {
      if (runWorkerCompletedEventArgs?.Error != null)
        throw runWorkerCompletedEventArgs.Error;
      
      model.State = ConvertState.Completed;
      App.Shutdown();
    }

    private async Task InternalExecute(DownloadUpdate download, CancellationToken token)
    {
      UpdateDialogTexts("Идёт скачивание...", $"Скачивается {download.ProductName} версии {download.Version}");

      foreach (var file in await download.GetFiles())
      {
        this.Debug($"File {file.Name} ({file.Uri}) found.");
        model.Option.ProcessOutputFolder(file);
        var viewModel = new DownloadedFileViewModel(file, model.Option.OutputFolder, model.Option.UnpackRootSubfolder);
        model.DownloadedFiles.Add(viewModel);
      }

      if (model.DownloadedFiles.Count == 0)
        this.Debug("Files\\updates not found.");

      this.Debug("Download started.");

      var taskToLoad = model.DownloadedFiles
        .Select(f => f.Download(new Progress<DownloadProgress>(f.DownloadingHandler), token));
      await Task.WhenAll(taskToLoad).ConfigureAwait(true);
      if (token.IsCancellationRequested)
        App.Shutdown();

      this.Debug("Download completed.");

      if (model.Option.Unpack)
      {
        UpdateDialogTexts("Идёт распаковка...", string.Empty);

        this.Debug($"Unpack to {model.Option.OutputFolder} started.");

        var taskToUnpack = model.DownloadedFiles
          .Select(f => f.Unpack(new Progress<UnpackProgress>(f.UnpackingHandler)));
        await Task.WhenAll(taskToUnpack).WithCancellation(token);
        if (token.IsCancellationRequested)
          App.Shutdown();

        this.Debug("Unpack completed.");
      }

      if (!string.IsNullOrWhiteSpace(model.Option.RunAfterUpdate))
      {
        UpdateDialogTexts("Запуск приложения...", string.Empty);

        if (File.Exists(model.Option.RunAfterUpdate))
        {
          this.Debug($"Run app - {model.Option.RunAfterUpdate}.");
          new Process() { StartInfo = new ProcessStartInfo(model.Option.RunAfterUpdate) }.Start();
        }
        else
        {
          var files = Directory.GetFiles(model.Option.OutputFolder, "*", SearchOption.AllDirectories).ToList();
          try
          {
            var regex = new Regex(model.Option.RunAfterUpdate, RegexOptions.IgnoreCase);
            files = files.Where(a => regex.IsMatch(a)).ToList();
          }
          catch (Exception) { }
          foreach (var file in files)
          {
            this.Debug($"Run app - {file}.");
            new Process() { StartInfo = new ProcessStartInfo(file) }.Start();
          }
        }
      }

      this.Debug($"Update completed.");
      model.ProgressState = ProgressState.None;
    }

    public GetRepositoryFilesCommand(UpdateViewModel model)
    {
      this.model = model;
    }
  }
}