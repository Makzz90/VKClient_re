using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;

namespace VKClient.Video.VideoCatalog
{
    public partial class UserVideosUC : UserControl
  {

    private UserVideosViewModel UserVideosVM
    {
      get
      {
        return this.DataContext as UserVideosViewModel;
      }
    }

    public UserVideosUC()
    {
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
  }
}
