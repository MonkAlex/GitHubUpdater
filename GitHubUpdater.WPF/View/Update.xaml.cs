using System;
using System.Threading.Tasks;
using System.Windows;
using GitHubUpdater.WPF.ViewModel;

namespace GitHubUpdater.WPF.View
{
  /// <summary>
  /// Interaction logic for Update.xaml
  /// </summary>
  public partial class Update : Window
  {
    public Update()
    {
      InitializeComponent();
#warning Может надо тут задавать Owner'a всегда? Пока непонятно.
      if (this.Owner != null)
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    private void Converting_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
  }
}
