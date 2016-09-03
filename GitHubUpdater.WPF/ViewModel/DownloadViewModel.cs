using System.Collections.ObjectModel;

namespace GitHubUpdater.WPF.ViewModel
{
  public class DownloadViewModel : PageViewModel
  {
    public ObservableCollection<DownloadedFileViewModel> DownloadedFiles { get; }

    public DownloadViewModel()
    {
      this.Title = "Идёт загрузка...";
      this.DownloadedFiles = new ObservableCollection<DownloadedFileViewModel>();
    }
  }
}