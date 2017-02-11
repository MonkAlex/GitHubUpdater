using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GitHubUpdater.Avalonia.ViewModel;

namespace GitHubUpdater.Avalonia.View
{
  public class Update : Window
  {
    public Update()
    {
      this.InitializeComponent();
      App.AttachDevTools(this);
      this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, EventArgs e)
    {
      var oldValue = e.OldValue as IProcess;
      if (oldValue != null)
        oldValue.StateChanged -= OnStateChanged;

      var value = e.NewValue as IProcess;
      if (value != null)
        value.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(object sender, ConvertState convertState)
    {
      if (convertState != ConvertState.Completed)
        return;

      Close();
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
