using System;
using System.Windows;
using System.Windows.Controls;
using GitHubUpdater.Shared;

namespace GitHubUpdater.WPF.View
{
  /// <summary>
  /// Interaction logic for AbortRetryIgnore.xaml
  /// </summary>
  public partial class AbortRetryIgnore : Window
  {
    public UpdateExceptionReaction Result;

    public AbortRetryIgnore()
    {
      InitializeComponent();
      Result = UpdateExceptionReaction.Abort;
    }

    public static IExceptionReaction ShowDialog(Window window, string text)
    {
      var dialog = new AbortRetryIgnore {Text = {Text = text}, Owner = window };
      dialog.ShowDialog();
      return dialog.Result;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      var text = (sender as Button).Content.ToString();
      UpdateExceptionReaction.TryParse(value:text, result: out Result);
      this.Close();
    }
  }
}
