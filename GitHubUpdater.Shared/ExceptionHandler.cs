using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace GitHubUpdater.Shared
{
  public class UpdateExceptionReaction : Enumeration<UpdateExceptionReaction, string>, IExceptionReaction
  {
    public static UpdateExceptionReaction Abort = new UpdateExceptionReaction(nameof(Abort), "Прекратить");

    public static UpdateExceptionReaction Retry = new UpdateExceptionReaction(nameof(Retry), "Повторить");

    public static UpdateExceptionReaction Ignore = new UpdateExceptionReaction(nameof(Ignore), "Пропустить");

    public UpdateExceptionReaction(string value, string displayName) : base(value, displayName)
    {
    }

    public virtual void HandleException()
    {

    }
  }

  public class DownloadExceptionReaction : Enumeration<DownloadExceptionReaction, string>, IExceptionReaction
  {
    public static DownloadExceptionReaction ChangeUri = new DownloadExceptionReaction(nameof(ChangeUri), "Изменить адрес");

    public static DownloadExceptionReaction Abort = new DownloadExceptionReaction(nameof(Abort), "Прекратить");

    public static DownloadExceptionReaction Retry = new DownloadExceptionReaction(nameof(Retry), "Повторить");

    public static DownloadExceptionReaction Ignore = new DownloadExceptionReaction(nameof(Ignore), "Пропустить");

    public DownloadExceptionReaction(string value, string displayName) : base(value, displayName)
    {
    }

    public virtual void HandleException()
    {
      if (this == ChangeUri)
      {
        throw new NotImplementedException();
      }
    }
  }

  public interface IExceptionEventArgs<T>
    where T : IExceptionReaction
  {
    Exception Exception { get; }

    T Handled { get; set; }
  }

  public interface IExceptionReaction
  {
    string Value { get; }
    string DisplayName { get; }

    void HandleException();
  }
  /*
  public interface IExceptionEventArgs<T, TX> 
    : IExceptionEventArgs<T>
    where T : IExceptionReaction
    where TX : IExceptionEventArgs<T, TX>
  {
    EventHandler<IExceptionEventArgs<T, TX>> Handler { get; }
  }
  */
  public class UnpackExceptionEventArgs : IExceptionEventArgs<UpdateExceptionReaction>
  {
    public Exception Exception { get; }

    public UpdateExceptionReaction Handled { get; set; }

    public UnpackExceptionEventArgs(Exception ex)
    {
      this.Exception = ex;
    }
  }

  public class DownloadExceptionEventArgs : IExceptionEventArgs<DownloadExceptionReaction>
  {
    public Uri Uri { get; }

    public Exception Exception { get; }

    public DownloadExceptionReaction Handled { get; set; }

    public DownloadExceptionEventArgs(Exception ex, Uri uri)
    {
      this.Exception = ex;
      this.Uri = uri;
    }
  }


  public static class ExceptionHandler
  {
    public static void TryExecute(Action action, Func<Exception, IExceptionReaction> args)
    {
      TryExecute(() => { action(); return true; }, false, args);
    }
    
    public static T TryExecute<T>(Func<T> action, T whenIgnored, Func<Exception, IExceptionReaction> args)
    {
      IExceptionReaction handled = UpdateExceptionReaction.Retry;
      while (handled.Value == UpdateExceptionReaction.Retry.Value)
      {
        try
        {
          return action();
        }
        catch (Exception ex)
        {
          handled = args(ex);
          if (handled != null)
          {
            if (handled.Value == UpdateExceptionReaction.Abort.Value)
              throw;
            if (handled.Value != UpdateExceptionReaction.Retry.Value || handled.Value != UpdateExceptionReaction.Ignore.Value)
              handled.HandleException();
          }
          else
          {
            throw;
          }
        }
      }
      return whenIgnored;
    }



    public static async Task<T> TryExecuteAsync<T>(Func<Task<T>> action, Task<T> whenIgnored, Func<Exception, IExceptionReaction> args)
    {
      IExceptionReaction handled = UpdateExceptionReaction.Retry;
      while (handled.Value == UpdateExceptionReaction.Retry.Value)
      {
        try
        {
          return await action();
        }
        catch (Exception ex)
        {
          handled = args(ex);
          if (handled != null)
          {
            if (handled.Value == UpdateExceptionReaction.Abort.Value)
              throw;
            if (handled.Value != UpdateExceptionReaction.Retry.Value || handled.Value != UpdateExceptionReaction.Ignore.Value)
              handled.HandleException();
          }
          else
          {
            throw;
          }
        }
      }
      return await whenIgnored;
    }
/*
    private static TY OnHandler<TX, TY>(TX e) 
      where TX : IExceptionEventArgs<object, TY>
      where TY : IExceptionReaction
    {
      if (e.Handler == null || !e.Handler.GetInvocationList().Any())
      {
        ExceptionDispatchInfo.Capture(e.Exception).Throw();
      }
      else
      {
        e.Handler.Invoke(null, e);
      }
      return e.Handled;
    }*/
  }

}