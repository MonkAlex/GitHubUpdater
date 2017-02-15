using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GitHubUpdater.Avalonia.ViewModel;

namespace GitHubUpdater.Avalonia.View
{
  public class Update : Window, IDisposable
  {
    private readonly IDisposable subToDataContext;

    public Update()
    {
      this.InitializeComponent();
      App.AttachDevTools(this);
      this.Closed += OnClosed;
      subToDataContext = this.GetObservableWithHistory(DataContextProperty).Subscribe(OnDataContextChanged);
    }

    private void OnClosed(object sender, EventArgs eventArgs)
    {
      Dispose();
    }

    private void OnDataContextChanged(Tuple<object, object> sub)
    {
      var oldValue = sub.Item1 as IProcess;
      if (oldValue != null)
        oldValue.StateChanged -= OnStateChanged;

      var value = sub.Item2 as IProcess;
      if (value != null)
        value.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(object sender, ConvertState convertState)
    {
      if (convertState != ConvertState.Completed)
        return;

      Dispose();
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public void Dispose()
    {
      Close();
      subToDataContext?.Dispose();
    }
  }
}
