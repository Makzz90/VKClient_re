using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MediaItemsHorizontalUC : UserControl
  {
    private bool _contentLoaded;

    public ProfileMediaViewModelFacade ViewModel
    {
      get
      {
        return this.DataContext as ProfileMediaViewModelFacade;
      }
    }

    public MediaItemsHorizontalUC()
    {
      this.InitializeComponent();
    }

    private void MediaHorizontalAllItemsHeader_OnTap(object sender, RoutedEventArgs routedEventArgs)
    {
      if (this.ViewModel == null)
        return;
      Action headerTapAction = this.ViewModel.MediaHorizontalItemsViewModel.HeaderTapAction;
      if (headerTapAction == null)
        return;
      headerTapAction();
    }

    private void MediaVerticalItemsHeader_OnTap(object sender, RoutedEventArgs routedEventArgs)
    {
      if (this.ViewModel == null)
        return;
      Action headerTapAction = this.ViewModel.MediaVerticalItemsViewModel.HeaderTapAction;
      if (headerTapAction == null)
        return;
      headerTapAction();
    }

    private void MediaHorizontalListItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ViewModel == null)
        return;
      Action<MediaListItemViewModelBase> itemTapAction = this.ViewModel.MediaHorizontalItemsViewModel.ItemTapAction;
      if (itemTapAction == null)
        return;
      MediaListItemViewModelBase itemViewModelBase = ((FrameworkElement) sender).DataContext as MediaListItemViewModelBase;
      if (itemViewModelBase == null)
        return;
      itemTapAction(itemViewModelBase);
    }

    private void MediaVerticalListItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ViewModel == null)
        return;
      Action<MediaListItemViewModelBase> itemTapAction = this.ViewModel.MediaVerticalItemsViewModel.ItemTapAction;
      if (itemTapAction == null)
        return;
      MediaListItemViewModelBase itemViewModelBase = ((FrameworkElement) sender).DataContext as MediaListItemViewModelBase;
      if (itemViewModelBase == null)
        return;
      itemTapAction(itemViewModelBase);
    }

    private void MediaItemsList_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.MediaHorizontalItemsViewModel.LoadMoreItems(e.ContentPresenter.DataContext);
    }

    private void List_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void List_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
    }

    private void List_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    public void Unload()
    {
      this.ViewModel.MediaHorizontalItemsViewModel.Unload();
    }

    public void Reload()
    {
      this.ViewModel.MediaHorizontalItemsViewModel.Reload();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaItemsHorizontalUC.xaml", UriKind.Relative));
    }
  }
}
