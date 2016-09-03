namespace GitHubUpdater.WPF.ViewModel
{
  public class PageViewModel : BaseViewModel
  {
    private string title;

    public string Title
    {
      get { return title; }
      set
      {
        title = value;
        OnPropertyChanged();
      }
    }
  }
}