using System;
using System.Windows.Input;

namespace GitHubUpdater.WPF.Command
{
  public class BaseCommand : ICommand
  {
    public virtual bool CanExecute(object parameter)
    {
      return true;
    }

    public virtual void Execute(object parameter)
    {

    }

    public event EventHandler CanExecuteChanged;

    protected virtual void OnCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}