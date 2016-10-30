using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
        var viewModel = new DownloadedFileViewModel(file, model.Option.OutputFolder, model.Option.UnpackFolder);
        model.DownloadedFiles.Add(viewModel);
      }

      var taskToLoad = model.DownloadedFiles
        .Select(f => f.Download(new Progress<DownloadProgress>(f.DownloadingHandler)));
      await Task.WhenAll(taskToLoad);
      
      var taskToUnpack = model.DownloadedFiles
        .Select(f => f.Unpack(new Progress<UnpackProgress>(f.UnpackingHandler)));
      await Task.WhenAll(taskToUnpack);

      model.ProgressState = ProgressState.None;
      model.State = ConvertState.Completed;
    }

    public GetRepositoryFilesCommand(UpdateViewModel model)
    {
      this.model = model;
    }
  }
}