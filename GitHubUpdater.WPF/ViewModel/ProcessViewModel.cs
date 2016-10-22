using System;
using System.Windows;
using System.Windows.Shell;

namespace GitHubUpdater.WPF.ViewModel
{
  public class ProcessViewModel : BaseViewModel, IProcess
  {
    private double percent;
    private ConvertState state;
    private string status;
    private ProgressState progressState;
    private TaskbarItemProgressState taskbarItemProgressState;
    private string title;

    public event EventHandler<ConvertState> StateChanged;

    public double Percent
    {
      get { return percent; }
      set
      {
        if (value > 1 || value < 0)
          throw new ArgumentOutOfRangeException(nameof(Percent), "Select percent from 0 to 1.");

        percent = value;
        OnPropertyChanged();
      }
    }

    public bool IsIndeterminate
    {
      get { return this.ProgressState == ProgressState.Indeterminate; }
    }

    public ProgressState ProgressState
    {
      get { return progressState; }
      set
      {
        progressState = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(IsIndeterminate));
        Enum.TryParse(value.ToString(), out taskbarItemProgressState);
        OnPropertyChanged(nameof(TaskbarItemProgressState));
      }
    }

    public TaskbarItemProgressState TaskbarItemProgressState { get { return taskbarItemProgressState; } }

    public string Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged();
      }
    }

    public ConvertState State
    {
      get { return state; }
      set
      {
        state = value;
        OnPropertyChanged();
        OnStateChanged(value);
      }
    }

    public string Title
    {
      get { return title; }
      set
      {
        title = value;
        OnPropertyChanged();
      }
    }

    public ProcessViewModel()
    {
      this.Percent = 0;
      this.ProgressState = ProgressState.Indeterminate;
      this.Status = string.Empty;
      this.State = ConvertState.None;
    }

    protected virtual void OnStateChanged(ConvertState newState)
    {
      if (Application.Current.Dispatcher.CheckAccess())
        StateChanged?.Invoke(this, newState);
      else
        Application.Current.Dispatcher.InvokeAsync(() => StateChanged?.Invoke(this, newState));
    }
  }

  public enum ConvertState
  {
    None,
    Started,
    Completed
  }

  /// <summary>
  /// Определяет состояние индикатора хода выполнения на панели задач Windows.
  /// </summary>
  public enum ProgressState
  {
    None,
    Indeterminate,
    Normal,
    Error,
    Paused,
  }

  public interface IProcess
  {
    double Percent { get; set; }

    ProgressState ProgressState { get; set; }

    string Status { get; set; }

    ConvertState State { get; set; }

    event System.EventHandler<ConvertState> StateChanged;

    string Title { get; set; }
  }
}