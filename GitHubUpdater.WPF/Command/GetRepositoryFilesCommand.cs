using System;
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
        model.DownloadedFiles.Add(new DownloadedFileViewModel(file, model.Option.OutputFolder, model.Option.UnpackFolder));
      }

      var taskToLoad = model.DownloadedFiles
        .Select(f => f.Download(new Progress<DownloadProgressChangedEventArgs>(f.Handler)));
      await Task.WhenAll(taskToLoad);

      foreach (var file in model.DownloadedFiles)
      {
        if (File.Exists(file.Target))
        {
          var ext = Path.GetExtension(file.Target);
          foreach (var type in Generic.CreateAllTypes<IArchive>(file.Target).Where(t => t.Extension.Contains(ext)))
          {
            if (type.Test())
            {
#warning Надо оповещение о распаковке
              type.Unpack(file.TargetFolder, file.Subfolder);
              break;
            }
          }
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