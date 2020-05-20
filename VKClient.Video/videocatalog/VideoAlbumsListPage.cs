using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
  public class VideoAlbumsListPage : PageBase
  {
    private long _ownerId;
    private bool _isInitialized;
    private readonly ApplicationBarIconButton _addVideoButton;
    private ApplicationBar _appBar;
    private bool _forceAllowCreateNewAlbum;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    private VideoAlbumsListViewModel VM
    {
      get
      {
        return base.DataContext as VideoAlbumsListViewModel;
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
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
      Uri uri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton.IconUri=(uri);
      string appBarAdd = CommonResources.AppBar_Add;
      applicationBarIconButton.Text=(appBarAdd);
      this._addVideoButton = applicationBarIconButton;
      // ISSUE: explicit constructor call
     // base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBox.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBox);
      this.listBox.OnRefresh = (Action) (() => this.VM.AlbumsGenCol.LoadData(true, false,  null, false));
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._appBar = applicationBar;
      this._appBar.Buttons.Add(this._addVideoButton);
      this._addVideoButton.Click+=(new EventHandler(this._addVideoButton_Click));
    }

    private void _addVideoButton_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToCreateEditVideoAlbum(0, this._ownerId < 0L ? -this._ownerId : 0, "",  null);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._ownerId = this.CommonParameters.OwnerId;
        this._forceAllowCreateNewAlbum = bool.Parse(((Page) this).NavigationContext.QueryString["ForceAllowCreateAlbum"]);
        VideoAlbumsListViewModel albumsListViewModel = new VideoAlbumsListViewModel(this._ownerId, this._forceAllowCreateNewAlbum);
        base.DataContext = albumsListViewModel;
        albumsListViewModel.AlbumsGenCol.LoadData(true, false,  null, false);
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      this.ApplicationBar = (this.AllowCreateNewAlbum ?  this._appBar :  null);
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.AlbumsGenCol.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/VideoAlbumsListPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
    }
  }
}
