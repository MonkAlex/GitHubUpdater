using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHubUpdater.Shared;
using GitHubUpdater.Shared.Archive;
using GitHubUpdater.WPF.ViewModel;

namespace GitHubUpdater.WPF.Command
{
  public class GetRepositoryFilesCommand : BaseCommand
  {
    private UpdateViewModel model;

    public override async void Execute(object parameter)
    {
      base.Execute(parameter);

      model.ProgressState = ProgressState.Normal;
      model.State = ConvertState.Started;

      var download = new DownloadUpdate(model.Option);
      foreach (var file in await download.GetFiles())
      {
        model.Option.ProcessOutputFolder(file);
        var viewModel = new DownloadedFileViewModel(file, model.Option.OutputFolder, model.Option.UnpackRootSubfolder);
        model.DownloadedFiles.Add(viewModel);
      }
      /*
      var taskToLoad = model.DownloadedFiles
        .Select(f => f.Download(new Progress<DownloadProgress>(f.DownloadingHandler)));
      await Task.WhenAll(taskToLoad);
      */
      var taskToUnpack = model.DownloadedFiles
        .Select(f => f.Unpack(new Progress<UnpackProgress>(f.UnpackingHandler)));
      await Task.WhenAll(taskToUnpack);

      if (File.Exists(model.Option.RunAfterUpdate))
      {
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
          new Process() { StartInfo = new ProcessStartInfo(file) }.Start();
        }
      }

      model.ProgressState = ProgressState.None;
      model.State = ConvertState.Completed;
    }

    public GetRepositoryFilesCommand(UpdateViewModel model)
    {
      this.model = model;
    }
  }
}