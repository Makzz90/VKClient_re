using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Video.Library;
using VKClient.Video.Localization;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VKClient.Video
{
  public class VideoPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBar _appBar;
    private long _albumId;
    private readonly ApplicationBarIconButton _addVideoButton;
    private readonly ApplicationBarIconButton _searchVideoButton;
    private string _searchQuery;
    private bool _forceAllowVideoUpload;
    private bool _pickMode;
    private bool _loadedAlbums;
    private bool _loadedUploaded;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal Pivot mainPivot;
    internal PivotItem pivotItemVideo;
    internal ExtendedLongListSelector listBoxAllVideos;
    internal PivotItem pivotItemUploadedVideo;
    internal ExtendedLongListSelector listBoxUploadedVideos;
    internal PivotItem pivotItemAlbums;
    internal ExtendedLongListSelector listBoxAlbums;
    internal PullToRefreshUC pullToRefresh;
    private bool _contentLoaded;

    private VideosOfOwnerViewModel ViewModel
    {
      get
      {
        return base.DataContext as VideosOfOwnerViewModel;
      }
    }

    public VideoPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string appBarAdd = CommonResources.AppBar_Add;
      applicationBarIconButton1.Text = appBarAdd;
      this._addVideoButton = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarSearch = VideoResources.AppBar_Search;
      applicationBarIconButton2.Text = appBarSearch;
      this._searchVideoButton = applicationBarIconButton2;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.pivotItemVideo.Header=((object)CommonResources.VideoNew_Added.ToLowerInvariant());
      this.pivotItemUploadedVideo.Header=((object)CommonResources.VideoNew_Uploaded.ToLowerInvariant());
      this.Header.OnHeaderTap = new Action(this.HandleHeaderTap);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxAllVideos);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxUploadedVideos);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxAlbums);
      this.listBoxAllVideos.OnRefresh = (Action)(() => this.ViewModel.AllVideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)null, false));
      this.listBoxUploadedVideos.OnRefresh = (Action)(() => this.ViewModel.UploadedVideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)null, false));
      this.listBoxAlbums.OnRefresh = (Action)(() => this.ViewModel.AlbumsVM.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>)null, false));
      this.BuildAppBar();
      EventAggregator.Current.Subscribe((object)this);
    }

    private void HandleHeaderTap()
    {
      if (this.mainPivot.SelectedItem == this.pivotItemVideo)
        this.listBoxAllVideos.ScrollToTop();
      if (this.mainPivot.SelectedItem != this.pivotItemAlbums)
        return;
      this.listBoxAlbums.ScrollToTop();
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
      this._searchVideoButton.Click+=(new EventHandler(this.SearchVideoTap));
      this._addVideoButton.Click+=(new EventHandler(this._addVideoButton_Click));
      this._appBar.Buttons.Add(this._searchVideoButton);
      this.ApplicationBar = ((IApplicationBar) this._appBar);
    }

    private void _addVideoButton_Click(object sender, EventArgs e)
    {
      FileOpenPicker fileOpenPicker = new FileOpenPicker();
      foreach (string supportedVideoExtension in VKConstants.SupportedVideoExtensions)
        fileOpenPicker.FileTypeFilter.Add(supportedVideoExtension);
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["Operation"] = "VideoFromPhone";
      fileOpenPicker.PickSingleFileAndContinue();
    }

    private void EnableSearch()
    {
        VideosSearchDataProvider searchDataProvider = new VideosSearchDataProvider((IEnumerable<VideoHeader>)this.ViewModel.AllVideosVM.Collection);
        DataTemplate dataTemplate = (DataTemplate)((FrameworkElement)this).Resources["VideoTemplate2"];
        Action<object, object> selectedItemCallback = new Action<object, object>(this.HandleSearchSelectedItem);
        Action<string> textChangedCallback = (Action<string>)(searchString =>
        {
            ((UIElement)this.mainPivot).Visibility=(searchString != "" ? Visibility.Collapsed : Visibility.Visible);
            this._searchQuery = searchString;
        });
        DataTemplate itemTemplate = dataTemplate;
        Thickness? margin = new Thickness?(new Thickness(0.0, 77.0, 0.0, 0.0));
        GenericSearchUC.CreatePopup<VKClient.Common.Backend.DataObjects.Video, VideoHeader>((ISearchDataProvider<VKClient.Common.Backend.DataObjects.Video, VideoHeader>)searchDataProvider, selectedItemCallback, textChangedCallback, itemTemplate, (Func<SearchParamsUCBase>)(() => (SearchParamsUCBase)new SearchParamsVideoUC()), margin).Show((UIElement)this.mainPivot);
    }

    private void HandleSearchSelectedItem(object listBox, object selectedItem)
    {
      VideoHeader selected = selectedItem as VideoHeader;
      CurrentMediaSource.VideoSource = StatisticsActionSource.search;
      CurrentMediaSource.VideoContext = this._searchQuery;
      this.ProcessSelectedVideoHeader(selected);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      long albumId = 0;
      string albumName = "";
      if (((Page) this).NavigationContext.QueryString.ContainsKey("AlbumId"))
        albumId = long.Parse(((Page) this).NavigationContext.QueryString["AlbumId"]);
      if (((Page) this).NavigationContext.QueryString.ContainsKey("AlbumName"))
        albumName = ((Page) this).NavigationContext.QueryString["AlbumName"];
      this._pickMode = bool.Parse(((Page) this).NavigationContext.QueryString["PickMode"]);
      if (!this._isInitialized)
      {
        if (this.CommonParameters.UserOrGroupId == 0L)
          this.CommonParameters.UserOrGroupId = AppGlobalStateManager.Current.LoggedInUserId;
        this._forceAllowVideoUpload = ((Page) this).NavigationContext.QueryString.ContainsKey("ForceAllowVideoUpload") && ((Page) this).NavigationContext.QueryString["ForceAllowVideoUpload"] == bool.TrueString;
        VideosOfOwnerViewModel ofOwnerViewModel = new VideosOfOwnerViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, albumId, this._pickMode);
        // ISSUE: method pointer
        ofOwnerViewModel.GotUploadedAndAlbumsInfoCallback = new Action( this.HideUploadedAndAlbumsIfNeeded);
        base.DataContext = ofOwnerViewModel;
        this.UpdatePageHeaderForAlbum(albumId, albumName);
        this._albumId = albumId;
        ofOwnerViewModel.AllVideosVM.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
        this.UpdateTitle();
        this._isInitialized = true;
      }
      this.HandleInputParams();
      this.UpdateAppBar();
      if (this._pickMode)
        return;
      int num1 = this.CommonParameters.IsGroup ? 1 : 0;
      long userOrGroupId = this.CommonParameters.UserOrGroupId;
      long num2 = num1 != 0 ? -userOrGroupId : userOrGroupId;
      CurrentMediaSource.VideoSource = num1 != 0 ? StatisticsActionSource.videos_group : StatisticsActionSource.videos_user;
      CurrentMediaSource.VideoContext = string.Format("{0}_{1}", num2, albumId);
    }

    private void HideUploadedAndAlbumsIfNeeded()
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            this.mainPivot.IsLocked=(false);
            if (!this.ViewModel.HaveUploadedVideos)
                ((PresentationFrameworkCollection<object>)((ItemsControl)this.mainPivot).Items).Remove((object)this.pivotItemUploadedVideo);
            if (this.ViewModel.HaveAlbums)
                return;
            ((PresentationFrameworkCollection<object>)((ItemsControl)this.mainPivot).Items).Remove((object)this.pivotItemAlbums);
        }));
    }

    private void HandleInputParams()
    {
        FileOpenPickerContinuationEventArgs parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
        if (parameterForIdAndReset == null || !parameterForIdAndReset.Files.Any<StorageFile>())
            return;
        StorageFile storageFile = parameterForIdAndReset.Files.First<StorageFile>();
        string filePath = storageFile.Path;
        if (filePath.StartsWith("C:\\Data\\Users\\DefApps\\APPDATA\\Local\\Packages\\"))
        {
            AddEditVideoViewModel.PickedExternalFile = storageFile;
            filePath = "";
        }
        Navigator.Current.NavigateToAddNewVideo(filePath, this.CommonParameters.IsGroup ? -this.CommonParameters.UserOrGroupId : this.CommonParameters.UserOrGroupId);
    }

    private void UpdateAppBar()
    {
      int num = this.CommonParameters.UserOrGroupId != AppGlobalStateManager.Current.LoggedInUserId && !this._forceAllowVideoUpload || (this._albumId != 0L || this.mainPivot.SelectedItem == this.pivotItemAlbums) ? 0 : (!this._pickMode ? 1 : 0);
      if (num != 0 && !this._appBar.Buttons.Contains(this._addVideoButton))
        this._appBar.Buttons.Insert(0, this._addVideoButton);
      if (num != 0 || !this._appBar.Buttons.Contains(this._addVideoButton))
        return;
      this._appBar.Buttons.Remove(this._addVideoButton);
    }

    private void UpdatePageHeaderForAlbum(long albumId, string albumName)
    {
      if (albumId == 0L)
        return;
      this.pivotItemVideo.Header = albumName;
      this.mainPivot.IsLocked = true;
    }

    private void UpdateTitle()
    {
    }

    private void SearchVideoTap(object sender, EventArgs e)
    {
      this.EnableSearch();
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.AllVideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      this.ProcessSelectedVideoHeader(this.listBoxAllVideos.SelectedItem as VideoHeader);
      this.listBoxAllVideos.SelectedItem = null;
    }

    private void ExtendedLongListSelector_Link_2(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.UploadedVideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ExtendedLongListSelector_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
    {
      this.ProcessSelectedVideoHeader(this.listBoxUploadedVideos.SelectedItem as VideoHeader);
      this.listBoxUploadedVideos.SelectedItem = null;
    }

    private void ProcessSelectedVideoHeader(VideoHeader selected)
    {
      if (selected == null)
        return;
      if (!this.CommonParameters.PickMode)
      {
        Navigator.Current.NavigateToVideoWithComments(selected.VKVideo, selected.VKVideo.owner_id, selected.VKVideo.vid, "");
      }
      else
      {
        ParametersRepository.SetParameterForId("PickedVideo", selected.VKVideo);
        if (this._albumId != 0L)
          ((Page) this).NavigationService.RemoveBackEntrySafe();
        Navigator.Current.GoBack();
      }
    }

    private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.mainPivot.SelectedItem == this.pivotItemAlbums && !this._loadedAlbums)
      {
        this.ViewModel.AlbumsVM.LoadData(false, false,  null, false);
        this._loadedAlbums = true;
      }
      else if (this.mainPivot.SelectedItem == this.pivotItemUploadedVideo && !this._loadedUploaded)
      {
        this.ViewModel.UploadedVideosVM.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
        this._loadedUploaded = true;
      }
      this.UpdateAppBar();
    }

    private void Albums_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      AlbumHeader selectedItem = this.listBoxAlbums.SelectedItem as AlbumHeader;
      if (selectedItem == null)
        return;
      this.listBoxAlbums.SelectedItem = null;
      Navigator.Current.NavigateToVideoAlbum(selectedItem.VideoAlbum.album_id, selectedItem.Title, this.CommonParameters.PickMode, this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup);
    }

    private void Albums_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.AlbumsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Grid_Search_Video_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.mainPivot = (Pivot) base.FindName("mainPivot");
      this.pivotItemVideo = (PivotItem) base.FindName("pivotItemVideo");
      this.listBoxAllVideos = (ExtendedLongListSelector) base.FindName("listBoxAllVideos");
      this.pivotItemUploadedVideo = (PivotItem) base.FindName("pivotItemUploadedVideo");
      this.listBoxUploadedVideos = (ExtendedLongListSelector) base.FindName("listBoxUploadedVideos");
      this.pivotItemAlbums = (PivotItem) base.FindName("pivotItemAlbums");
      this.listBoxAlbums = (ExtendedLongListSelector) base.FindName("listBoxAlbums");
      this.pullToRefresh = (PullToRefreshUC) base.FindName("pullToRefresh");
    }
  }
}
