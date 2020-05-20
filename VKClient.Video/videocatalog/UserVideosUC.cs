using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

namespace VKClient.Video.VideoCatalog
{
  public class UserVideosUC : UserControl
  {
    internal Grid LayoutRoot;
    internal ExtendedLongListSelector listBoxAdded;
    internal ExtendedLongListSelector listBoxUploaded;
    internal ExtendedLongListSelector listBoxAlbums;
    private bool _contentLoaded;

    private UserVideosViewModel UserVideosVM
    {
      get
      {
        return base.DataContext as UserVideosViewModel;
      }
    }

    public UserVideosUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void gridAdded_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.UserVideosVM.VideoListSource = UserVideosViewModel.CurrentSource.Added;
    }

    private void gridUploaded_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.UserVideosVM.VideoListSource = UserVideosViewModel.CurrentSource.Uploaded;
    }

    private void gridAlbums_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.UserVideosVM.VideoListSource = UserVideosViewModel.CurrentSource.Albums;
    }

    private void addedListBoxLink(object sender, LinkUnlinkEventArgs e)
    {
      this.UserVideosVM.VideosOfOwnerVM.AllVideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void uploadedListBoxLink(object sender, LinkUnlinkEventArgs e)
    {
      this.UserVideosVM.VideosOfOwnerVM.UploadedVideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void albumsListBoxLink(object sender, LinkUnlinkEventArgs e)
    {
      this.UserVideosVM.VideosOfOwnerVM.AlbumsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/UserVideosUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.listBoxAdded = (ExtendedLongListSelector) base.FindName("listBoxAdded");
      this.listBoxUploaded = (ExtendedLongListSelector) base.FindName("listBoxUploaded");
      this.listBoxAlbums = (ExtendedLongListSelector) base.FindName("listBoxAlbums");
    }
  }
}
