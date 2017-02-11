using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using GitHubUpdater.Shared;
using GitHubUpdater.Avalonia.ViewModel;

namespace GitHubUpdater.Avalonia.Command
{
  public class GetRepositoryFilesCommand : BaseCommand
  {
    private UpdateViewModel model;

    public override async void Execute(object parameter)
    {
      base.Execute(parameter);

      try
      {
        model.ProgressState = ProgressState.Normal;
        model.State = ConvertState.Started;

        var download = new DownloadUpdate(model.Option);
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
        Application.Current.Exit();
    }

    public GetRepositoryFilesCommand(UpdateViewModel model)
    {
      this.model = model;
    }
  }
}