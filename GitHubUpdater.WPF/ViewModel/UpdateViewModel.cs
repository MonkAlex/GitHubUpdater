using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using GitHubUpdater.Shared;
using GitHubUpdater.WPF.Command;

namespace GitHubUpdater.WPF.ViewModel
{
  public class UpdateViewModel : ProcessViewModel
  {
    public Option Option { get; }

    public ICommand Start { get; }

    public ICollection<DownloadedFileViewModel> DownloadedFiles { get; set; }

    public UpdateViewModel(Option option)
    {
      this.Option = option;
      this.DownloadedFiles = new List<DownloadedFileViewModel>();
      this.Start = new GetRepositoryFilesCommand(this);
    }
  }
}