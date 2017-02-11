using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows.Input;
using GitHubUpdater.Shared;
using GitHubUpdater.Avalonia.Command;

namespace GitHubUpdater.Avalonia.ViewModel
{
  public class UpdateViewModel : ProcessViewModel
  {
    public Option Option { get; }

    public ICommand Start { get; }

    public ObservableCollection<DownloadedFileViewModel> DownloadedFiles { get; set; }

    public UpdateViewModel(Option option)
    {
      this.Option = option;
      this.DownloadedFiles = new ObservableCollection<DownloadedFileViewModel>();
      this.DownloadedFiles.CollectionChanged += DownloadedFilesOnCollectionChanged;
      this.Start = new GetRepositoryFilesCommand(this);
    }

    private void DownloadedFilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      if (args.Action == NotifyCollectionChangedAction.Remove ||
          args.Action == NotifyCollectionChangedAction.Replace ||
          args.Action == NotifyCollectionChangedAction.Move)
      {
        foreach (var item in args.OldItems.OfType<DownloadedFileViewModel>())
        {
          item.PropertyChanged -= ItemOnPropertyChanged;
        }
      }

      if (args.Action == NotifyCollectionChangedAction.Add ||
          args.Action == NotifyCollectionChangedAction.Replace ||
          args.Action == NotifyCollectionChangedAction.Move ||
          args.Action == NotifyCollectionChangedAction.Reset)
      {
        foreach (var item in args.NewItems.OfType<DownloadedFileViewModel>())
        {
          item.PropertyChanged += ItemOnPropertyChanged;
        }
      }
    }

    private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (propertyChangedEventArgs.PropertyName == nameof(DownloadedFileViewModel.Downloaded))
      {
        Percent = this.DownloadedFiles.Select(f => f.Downloaded).Average();
      }
    }
  }
}