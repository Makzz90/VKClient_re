using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Video.Library;

namespace VKClient.Video
{
  public class PickVideoAlbumPage : PageBase
  {
    private bool _isInitialized;
    private long _videoId;
    private long _videoOwnerId;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    internal ExtendedLongListSelector listBoxAlbums;
    private bool _contentLoaded;

    private VideosOfOwnerViewModel VM
    {
      get
      {
        return base.DataContext as VideosOfOwnerViewModel;
      }
    }

    public PickVideoAlbumPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._videoId = long.Parse(((Page) this).NavigationContext.QueryString["VideoId"]);
      this._videoOwnerId = long.Parse(((Page) this).NavigationContext.QueryString["VideoOwnerId"]);
      VideosOfOwnerViewModel ofOwnerViewModel = new VideosOfOwnerViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, 0, false);
      ofOwnerViewModel.ShowAddedAlbum = true;
      base.DataContext = ofOwnerViewModel;
      ofOwnerViewModel.AlbumsVM.LoadData(false, false,  null, false);
      this._isInitialized = true;
    }

    private void Albums_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.AlbumsVM.LoadMoreIfNeeded(e.ContentPresenter);
    }

    private void Albums_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      AlbumHeader selectedItem = this.listBoxAlbums.SelectedItem as AlbumHeader;
      if (selectedItem == null)
        return;
      this.listBoxAlbums.SelectedItem = null;
      this.AddVideoToAlbum(selectedItem.VideoAlbum.id);
    }

    private void AddVideoToAlbum(long album_id)
    {
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/PickVideoAlbumPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.listBoxAlbums = (ExtendedLongListSelector) base.FindName("listBoxAlbums");
    }
  }
}
