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
        return base.DataContext as ProfileMediaViewModelFacade;
      }
    }

    public MediaItemsHorizontalUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void MediaHorizontalAllItemsHeader_OnTap(object sender, RoutedEventArgs routedEventArgs)
    {
      ProfileMediaViewModelFacade viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      IMediaHorizontalItemsViewModel horizontalItemsViewModel = viewModel.MediaHorizontalItemsViewModel;
      if (horizontalItemsViewModel == null)
        return;
      Action headerTapAction = horizontalItemsViewModel.HeaderTapAction;
      if (headerTapAction == null)
        return;
      headerTapAction();
    }

    private void MediaVerticalItemsHeader_OnTap(object sender, RoutedEventArgs routedEventArgs)
    {
      ProfileMediaViewModelFacade viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      IMediaVerticalItemsViewModel verticalItemsViewModel = viewModel.MediaVerticalItemsViewModel;
      if (verticalItemsViewModel == null)
        return;
      Action headerTapAction = verticalItemsViewModel.HeaderTapAction;
      if (headerTapAction == null)
        return;
      headerTapAction();
    }

    private void MediaHorizontalListItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileMediaViewModelFacade viewModel = this.ViewModel;
      Action<MediaListItemViewModelBase> action1;
      if (viewModel == null)
      {
        action1 =  null;
      }
      else
      {
        IMediaHorizontalItemsViewModel horizontalItemsViewModel = viewModel.MediaHorizontalItemsViewModel;
        action1 = horizontalItemsViewModel != null ? horizontalItemsViewModel.ItemTapAction :  null;
      }
      Action<MediaListItemViewModelBase> action2 = action1;
      if (action2 == null)
        return;
      MediaListItemViewModelBase dataContext = ((FrameworkElement) sender).DataContext as MediaListItemViewModelBase;
      if (dataContext == null)
        return;
      action2(dataContext);
    }

    private void MediaVerticalListItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileMediaViewModelFacade viewModel = this.ViewModel;
      Action<MediaListItemViewModelBase> action1;
      if (viewModel == null)
      {
        action1 =  null;
      }
      else
      {
        IMediaVerticalItemsViewModel verticalItemsViewModel = viewModel.MediaVerticalItemsViewModel;
        action1 = verticalItemsViewModel != null ? verticalItemsViewModel.ItemTapAction :  null;
      }
      Action<MediaListItemViewModelBase> action2 = action1;
      if (action2 == null)
        return;
      MediaListItemViewModelBase dataContext = ((FrameworkElement) sender).DataContext as MediaListItemViewModelBase;
      if (dataContext == null)
        return;
      action2(dataContext);
    }

    private void MediaItemsList_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      ProfileMediaViewModelFacade viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      IMediaHorizontalItemsViewModel horizontalItemsViewModel = viewModel.MediaHorizontalItemsViewModel;
      if (horizontalItemsViewModel == null)
        return;
      object dataContext = ((FrameworkElement) e.ContentPresenter).DataContext;
      horizontalItemsViewModel.LoadMoreItems(dataContext);
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
      ProfileMediaViewModelFacade viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      IMediaHorizontalItemsViewModel horizontalItemsViewModel = viewModel.MediaHorizontalItemsViewModel;
      if (horizontalItemsViewModel == null)
        return;
      horizontalItemsViewModel.Unload();
    }

    public void Reload()
    {
      ProfileMediaViewModelFacade viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      IMediaHorizontalItemsViewModel horizontalItemsViewModel = viewModel.MediaHorizontalItemsViewModel;
      if (horizontalItemsViewModel == null)
        return;
      horizontalItemsViewModel.Reload();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaItemsHorizontalUC.xaml", UriKind.Relative));
    }
  }
}
