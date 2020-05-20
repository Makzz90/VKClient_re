using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.UC;
using VKClient.Video.Library;
using VKClient.Video.Localization;
using VKClient.Video.VideoCatalog;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VKClient.Common.VideoCatalog
{
    public partial class VideoCatalogPage : PageBase
  {
    private readonly ApplicationBarIconButton _searchVideoButton = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
      Text = VideoResources.AppBar_Search
    };
    private readonly ApplicationBarIconButton _addVideoButton = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Add
    };
    private UCPool _virtControlsPool = new UCPool();
    private bool _isInitialized;
    private ApplicationBar _appBar;
    private string _searchQuery;

    public VideoCatalogViewModel VM
    {
      get
      {
        return this.DataContext as VideoCatalogViewModel;
      }
    }

    public VideoCatalogPage()
    {
      this.InitializeComponent();
      this.virtPanel.CountOfItemsBeforeLoadMore = 40;
      this.ucHeader.OnHeaderTap = (Action) (() =>
      {
        if (this.pivot.SelectedItem == this.pivotItemCatalog)
          this.virtPanel.ScrollToBottom(false);
        else if (this.VM.UserVideosVM.VideoListSource == UserVideosViewModel.CurrentSource.Added)
          this.ucMyVideos.listBoxAdded.ScrollToTop();
        else if (this.VM.UserVideosVM.VideoListSource == UserVideosViewModel.CurrentSource.Uploaded)
        {
          this.ucMyVideos.listBoxUploaded.ScrollToTop();
        }
        else
        {
          if (this.VM.UserVideosVM.VideoListSource != UserVideosViewModel.CurrentSource.Albums)
            return;
          this.ucMyVideos.listBoxAlbums.ScrollToTop();
        }
      });
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.virtPanel);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.ucMyVideos.listBoxAdded);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.ucMyVideos.listBoxUploaded);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.ucMyVideos.listBoxAlbums);
      this.virtPanel.OnRefresh = (Action) (() => this.VM.LoadData(true));
      this.ucMyVideos.listBoxAdded.OnRefresh = (Action) (() => this.VM.UserVideosVM.VideosOfOwnerVM.AllVideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false));
      this.ucMyVideos.listBoxUploaded.OnRefresh = (Action) (() => this.VM.UserVideosVM.VideosOfOwnerVM.UploadedVideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false));
      this.ucMyVideos.listBoxAlbums.OnRefresh = (Action) (() => this.VM.UserVideosVM.VideosOfOwnerVM.AlbumsVM.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>) null, false));
      this.virtPanel.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scroll), false);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.virtPanel);
      this.scroll.BindViewportBoundsTo((FrameworkElement) this.stackPanel);
      this.BuildAppBar();
      this.virtPanel.CreateVirtItemFunc = (Func<object, IVirtualizable>) (obj => (IVirtualizable) new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>) (() =>
      {
        UserControlVirtualizable controlVirtualizable = (UserControlVirtualizable) null;
        if (obj is ListHeaderViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<ListHeaderUC>();
        if (obj is CategoryMoreFooter)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<CategoryMoreFooterUC>();
        if (obj is CatalogItemViewModel)
        {
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<CatalogItemUC>();
          (controlVirtualizable as CatalogItemUC).GridLayoutRoot.Background = (Brush) (Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush);
        }
        if (obj is CatalogItemsHorizontalViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<CatalogItemsHorizontalExtUC>();
        if (obj is DividerSpaceUpViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<DividerSpaceUpUC>();
        if (obj is DividerSpaceDownViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<DividerSpaceDownUC>();
        if (obj is CatalogItemTwoInARowViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<CatalogItemTwoInARowUC>();
        controlVirtualizable.DataContext = obj;
        return controlVirtualizable;
      }), (Func<double>) (() =>
      {
        if (obj is ListHeaderViewModel)
          return 84.0;
        if (obj is CategoryMoreFooter)
          return 64.0;
        if (obj is CatalogItemViewModel)
          return 128.0;
        if (obj is CatalogItemsHorizontalViewModel)
          return 242.0;
        if (obj is DividerSpaceUpViewModel || obj is DividerSpaceDownViewModel)
          return 8.0;
        return obj is CatalogItemTwoInARowViewModel ? 138.0 : 0.0;
      }), (Action<UserControlVirtualizable>) (ucv => this._virtControlsPool.AddBackToPool(ucv)), 0.0, false));
    }

    private void BuildAppBar()
    {
      this._appBar = new ApplicationBar()
      {
        BackgroundColor = VKConstants.AppBarBGColor,
        ForegroundColor = VKConstants.AppBarFGColor,
        Opacity = 0.9
      };
      this._searchVideoButton.Click += new EventHandler(this.SearchVideoTap);
      this._appBar.Buttons.Add((object) this._searchVideoButton);
      this._addVideoButton.Click += new EventHandler(this._addVideoButton_Click);
      this.ApplicationBar = (IApplicationBar) this._appBar;
    }

    private void _addVideoButton_Click(object sender, EventArgs e)
    {
      if (this.VM.UserVideosVM.VideoListSource == UserVideosViewModel.CurrentSource.Albums)
        Navigator.Current.NavigateToCreateEditVideoAlbum(0L, 0L, "", (PrivacyInfo) null);
      else
        this.PickVideoFileUsingSystemPicker();
    }

    private void PickVideoFileUsingSystemPicker()
    {
      FileOpenPicker fileOpenPicker = new FileOpenPicker();
      foreach (string supportedVideoExtension in VKConstants.SupportedVideoExtensions)
        fileOpenPicker.FileTypeFilter.Add(supportedVideoExtension);
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["Operation"] = "VideoFromPhone";
      fileOpenPicker.PickSingleFileAndContinue();
    }

    private void SearchVideoTap(object sender, EventArgs e)
    {
      this.EnableSearch();
    }

    private void EnableSearch()
    {
      VideosSearchDataProvider searchDataProvider = new VideosSearchDataProvider((IEnumerable<VideoHeader>) this.VM.UserVideosVM.VideosOfOwnerVM.AllVideosVM.Collection.ToList<VideoHeader>());
      DataTemplate dataTemplate = (DataTemplate) this.ucMyVideos.Resources["ItemTemplate"];
      Action<object, object> selectedItemCallback = new Action<object, object>(this.HandleSearchSelectedItem);
      Action<string> textChangedCallback = (Action<string>) (searchString =>
      {
        this.pivot.Visibility = searchString != "" ? Visibility.Collapsed : Visibility.Visible;
        this._searchQuery = searchString;
      });
      DataTemplate itemTemplate = dataTemplate;
      Thickness? margin = new Thickness?(new Thickness(0.0, 77.0, 0.0, 0.0));
      GenericSearchUC.CreatePopup<VKClient.Common.Backend.DataObjects.Video, VideoHeader>((ISearchDataProvider<VKClient.Common.Backend.DataObjects.Video, VideoHeader>) searchDataProvider, selectedItemCallback, textChangedCallback, itemTemplate, (Func<SearchParamsUCBase>) (() => (SearchParamsUCBase) new SearchParamsVideoUC()), margin).Show((UIElement) this.pivot);
    }

    private void HandleSearchSelectedItem(object listBox, object selectedItem)
    {
      VideoHeader selected = selectedItem as VideoHeader;
      CurrentMediaSource.VideoSource = StatisticsActionSource.search;
      CurrentMediaSource.VideoContext = this._searchQuery;
      this.ProcessSelectedVideoHeader(selected);
    }

    private void ProcessSelectedVideoHeader(VideoHeader selected)
    {
      if (selected == null)
        return;
      Navigator.Current.NavigateToVideoWithComments(selected.VKVideo, selected.VKVideo.owner_id, selected.VKVideo.vid, "");
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        VideoCatalogViewModel catalogViewModel = new VideoCatalogViewModel();
        this.DataContext = (object) catalogViewModel;
        catalogViewModel.LoadData(true);
        this._isInitialized = true;
      }
      this.HandleInputParams();
    }

    private void HandleInputParams()
    {
      FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
      if (continuationEventArgs == null || !((IEnumerable<StorageFile>) continuationEventArgs.Files).Any<StorageFile>())
        return;
      StorageFile storageFile = ((IEnumerable<StorageFile>) continuationEventArgs.Files).First<StorageFile>();
      string filePath = storageFile.Path;
      if (filePath.StartsWith("C:\\Data\\Users\\DefApps\\APPDATA\\Local\\Packages\\"))
      {
        AddEditVideoViewModel.PickedExternalFile = storageFile;
        filePath = "";
      }
      Navigator.Current.NavigateToAddNewVideo(filePath, AppGlobalStateManager.Current.LoggedInUserId);
    }

    private void pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
    {
    }

    private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.pivot.SelectedItem == this.pivotItemMyVideos)
        this.VM.UserVideosVM.LoadIfNeeded();
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      bool flag = this.pivot.SelectedItem == this.pivotItemMyVideos;
      if (flag && !this._appBar.Buttons.Contains((object) this._addVideoButton))
      {
        this._appBar.Buttons.Insert(0, (object) this._addVideoButton);
      }
      else
      {
        if (flag || !this._appBar.Buttons.Contains((object) this._addVideoButton))
          return;
        this._appBar.Buttons.Remove((object) this._addVideoButton);
      }
    }

  }
}
