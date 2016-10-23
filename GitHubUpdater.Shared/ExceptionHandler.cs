using System;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace GitHubUpdater.Shared
{
  public enum ExceptionHandle
  {
    Abort,
    Retry,
    Ignore
  }

  public class ExceptionEventArgs
  {
    public Exception Exception { get; }

    public ExceptionHandle? Handled { get; set; }

    public ExceptionEventArgs(Exception ex)
    {
      this.Exception = ex;
    }
  }

  public static class ExceptionHandler
  {
    public static event EventHandler<ExceptionEventArgs> Handler;

    public static void TryExecute(Action action)
    {
      ExceptionHandler<bool>.TryExecute(() => { action(); return true; }, false);
    }
    
    public static ExceptionHandle? OnHandler(ExceptionEventArgs e)
    {
      if (Handler == null || !Handler.GetInvocationList().Any())
      {
        ExceptionDispatchInfo.Capture(e.Exception).Throw();
      }
      else
      {
        Handler.Invoke(null, e);
      }
      return e.Handled;
    }
  }

  public static class ExceptionHandler<T>
  {
    public static T TryExecute(Func<T> action, T whenFailed)
    {
      ExceptionHandle? handled = ExceptionHandle.Retry;
      while (handled == ExceptionHandle.Retry)
      {
        try
        {
          return action();
        }
        catch (Exception ex)
        {
          handled = ExceptionHandler.OnHandler(new ExceptionEventArgs(ex));
          if (handled.HasValue)
          {
            switch (handled.Value)
            {
              case ExceptionHandle.Abort:
                throw;
                break;
              case ExceptionHandle.Retry:
                break;
              case ExceptionHandle.Ignore:
                return whenFailed;
                break;
              default:
                throw new ArgumentOutOfRangeException();
            }
          }
          else
          {
            throw;
          }
        }
      }
      return whenFailed;
    }
  }
}