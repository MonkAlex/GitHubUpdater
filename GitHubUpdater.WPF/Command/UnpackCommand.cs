using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHubUpdater.Shared;
using GitHubUpdater.Shared.Archive;
using GitHubUpdater.WPF.ViewModel;

namespace GitHubUpdater.WPF.Command
{
  public class UnpackCommand : BaseCommand
  {
    private DownloadedFileViewModel model;

    public override bool CanExecute(object parameter)
    {
      return base.CanExecute(parameter) && model.Downloaded == 100;
    }

    public override void Execute(object parameter)
    {
      base.Execute(parameter);

      if (File.Exists(model.Target))
      {
        var ext = Path.GetExtension(model.Target);
        foreach (var type in Generic.CreateAllTypes<IArchive>(model.Target).Where(t => t.Extension.Contains(ext)))
        {
          if (type.Test())
          {
            type.Unpack(model.TargetFolder, model.Subfolder);
            break;
          }
        }
      }
    }

    public UnpackCommand(DownloadedFileViewModel model)
    {
      this.model = model;
      model.PropertyChanged += ModelOnPropertyChanged;
    }

    private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      if (args.PropertyName == nameof(model.Downloaded))
        OnCanExecuteChanged();
    }
  }
}
