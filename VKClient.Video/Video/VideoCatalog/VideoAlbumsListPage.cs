using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Video.VideoCatalog
{
    public partial class VideoAlbumsListPage : PageBase
  {
    private readonly ApplicationBarIconButton _addVideoButton = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Add
    };
    private long _ownerId;
    private bool _isInitialized;
    private ApplicationBar _appBar;
    private bool _forceAllowCreateNewAlbum;

    private VideoAlbumsListViewModel VM
    {
      get
      {
        return this.DataContext as VideoAlbumsListViewModel;
      }
    }

    private bool AllowCreateNewAlbum
    {
      get
      {
        if (!this._forceAllowCreateNewAlbum)
          return this._ownerId == AppGlobalStateManager.Current.LoggedInUserId;
        return true;
      }
    }

    public VideoAlbumsListPage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBox.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBox);
      this.listBox.OnRefresh = (Action) (() => this.VM.AlbumsGenCol.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>) null, false));
    }

    private void BuildAppBar()
    {
      this._appBar = new ApplicationBar()
      {
        BackgroundColor = VKConstants.AppBarBGColor,
        ForegroundColor = VKConstants.AppBarFGColor,
        Opacity = 0.9
      };
      this._appBar.Buttons.Add((object) this._addVideoButton);
      this._addVideoButton.Click += new EventHandler(this._addVideoButton_Click);
    }

    private void _addVideoButton_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToCreateEditVideoAlbum(0L, this._ownerId < 0L ? -this._ownerId : 0L, "", (PrivacyInfo) null);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._ownerId = this.CommonParameters.OwnerId;
        this._forceAllowCreateNewAlbum = bool.Parse(this.NavigationContext.QueryString["ForceAllowCreateAlbum"]);
        VideoAlbumsListViewModel albumsListViewModel = new VideoAlbumsListViewModel(this._ownerId, this._forceAllowCreateNewAlbum);
        this.DataContext = (object) albumsListViewModel;
        albumsListViewModel.AlbumsGenCol.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>) null, false);
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      this.ApplicationBar = this.AllowCreateNewAlbum ? (IApplicationBar) this._appBar : (IApplicationBar) null;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.AlbumsGenCol.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }
  }
}
