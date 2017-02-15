using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.ViewModel;
using Ookii.Dialogs.Wpf;

namespace GitHubUpdater.WPF.Command
{
  public class GetRepositoryFilesCommand : BaseCommand
  {
    private UpdateViewModel model;

    public override async void Execute(object parameter)
    {
      base.Execute(parameter);

      try
      {
        var download = new DownloadUpdate(model.Option);
        var hasUpdate = await download.HasUpdate();
        if (!hasUpdate)
        {
          this.Debug("Updates not found.");
          App.Current.Shutdown(0);
          return;
        }

        if (!model.Option.Silent)
        {
          var window = App.Current.MainWindow;
          var dialog = new Ookii.Dialogs.Wpf.TaskDialog();
          dialog.WindowTitle = download.ProductName;
          dialog.Content = "Перед обновлением закройте приложение.";
          if (string.IsNullOrWhiteSpace(model.Option.Version))
          {
            dialog.MainInstruction = string.Format("Установить {0} версии {1}?",
              download.ProductName, download.Version);
          }
          else
          {
            dialog.MainInstruction = string.Format("Установить обновление для {2}{3}с {1} до {0}?",
              download.Version, model.Option.Version, download.ProductName, Environment.NewLine);
            Version oldVersion;
            if (Version.TryParse(model.Option.Version, out oldVersion))
            {
              Version newVersion;
              if (Version.TryParse(download.Version, out newVersion))
              {
                if (oldVersion.CompareTo(newVersion) > 0)
                {
                  dialog.Content += Environment.NewLine;
                  dialog.Content += "Доступная версия ниже установленной.";
                }
              }
            }
          }
          var yes = new TaskDialogButton(ButtonType.Yes);
          var no = new TaskDialogButton(ButtonType.No);
          dialog.Buttons.Add(yes);
          dialog.Buttons.Add(no);
          if (dialog.ShowDialog(window) != yes)
          {
            this.Debug("User cancel update.");
            App.Current.Shutdown(0);
            return;
          }

          window.Show();
        }

        model.ProgressState = ProgressState.Normal;
        model.State = ConvertState.Started;

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
          .Select(f => f.Download(new Progress<DownloadProgress>(f.DownloadingHandler)));
        await Task.WhenAll(taskToLoad);

        this.Debug("Download completed.");

        if (model.Option.Unpack)
        {
          this.Debug($"Unpack to {model.Option.OutputFolder} started.");

          var taskToUnpack = model.DownloadedFiles
            .Select(f => f.Unpack(new Progress<UnpackProgress>(f.UnpackingHandler)));
          await Task.WhenAll(taskToUnpack);

          this.Debug("Unpack completed.");
        }

        if (!string.IsNullOrWhiteSpace(model.Option.RunAfterUpdate))
        {
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
      catch (Exception ex)
      {
        this.Debug(ex);
        model.ProgressState = ProgressState.Error;
      }
      model.State = ConvertState.Completed;
      if (model.Option.Silent)
        App.Current.Shutdown(0);
    }

    public GetRepositoryFilesCommand(UpdateViewModel model)
    {
      this.model = model;
    }
  }
}