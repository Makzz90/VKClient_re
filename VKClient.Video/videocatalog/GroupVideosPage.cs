using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.UC;
using VKClient.Common.VideoCatalog;
using VKClient.Video.Library;
using VKClient.Video.Localization;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VKClient.Video.VideoCatalog
{
  public class GroupVideosPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBar _appBar;
    private readonly ApplicationBarIconButton _searchVideoButton;
    private readonly ApplicationBarIconButton _addVideoButton;
    private string _searchQuery;
    private UCPool _virtControlsPool;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal Grid gridContent;
    internal ViewportControl scroll;
    internal StackPanel stackPanel;
    internal MyVirtualizingPanel2 virtPanel;
    private bool _contentLoaded;

    private GroupVideosViewModel VM
    {
      get
      {
        return base.DataContext as GroupVideosViewModel;
      }
    }

    public GroupVideosPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri=(uri1);
      string appBarSearch = VideoResources.AppBar_Search;
      applicationBarIconButton1.Text=(appBarSearch);
      this._searchVideoButton = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri=(uri2);
      string appBarAdd = CommonResources.AppBar_Add;
      applicationBarIconButton2.Text=(appBarAdd);
      this._addVideoButton = applicationBarIconButton2;
      this._virtControlsPool = new UCPool();
      // ISSUE: explicit constructor call
   //   base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucHeader.Title = CommonResources.Profile_Videos.ToUpperInvariant();
      this.ucHeader.OnHeaderTap = (Action) (() => this.virtPanel.ScrollToBottom(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.virtPanel);
      this.virtPanel.OnRefresh = (Action) (() => this.VM.VideosGenCol.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false));
      this.virtPanel.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scroll), false);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.virtPanel);
      this.scroll.BindViewportBoundsTo((FrameworkElement) this.stackPanel);
      this.virtPanel.CreateVirtItemFunc = (Func<object, IVirtualizable>) (obj => (IVirtualizable) new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>) (() =>
      {
        UserControlVirtualizable controlVirtualizable =  null;
        if (obj is OwnerHeaderWithSubscribeViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<OwnerHeaderWithSubscribeUC>();
        if (obj is SectionHeaderViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<SectionHeaderUC>();
        if (obj is AlbumsListHorizontalViewModel)
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<CatalogItemsHorizontalExtUC>();
        if (obj is VideoHeader)
        {
          controlVirtualizable = (UserControlVirtualizable) this._virtControlsPool.GetFromPool<CatalogItemUC>();
          ((Panel) (controlVirtualizable as CatalogItemUC).GridLayoutRoot).Background=((Brush) (Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush));
        }
        ((FrameworkElement) controlVirtualizable).DataContext=(obj);
        return controlVirtualizable;
      }), (Func<double>) (() =>
      {
        if (obj is OwnerHeaderWithSubscribeViewModel)
          return 68.0;
        if (obj is SectionHeaderViewModel)
          return 80.0;
        if (obj is AlbumsListHorizontalViewModel)
          return 242.0;
        return obj is VideoHeader ? 128.0 : 0.0;
      }), (Action<UserControlVirtualizable>) (uc => this._virtControlsPool.AddBackToPool(uc)), 0.0, false));
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor=(appBarBgColor);
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor=(appBarFgColor);
      double num = 0.9;
      applicationBar.Opacity=(num);
      this._appBar = applicationBar;
      this._searchVideoButton.Click+=(new EventHandler(this.SearchVideoTap));
      this._addVideoButton.Click+=(new EventHandler(this._addVideoButton_Click));
      if (this._appBar.Buttons.Count <= 0)
        return;
      this.ApplicationBar=((IApplicationBar) this._appBar);
    }

    private void _addVideoButton_Click(object sender, EventArgs e)
    {
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
      VideosSearchDataProvider searchDataProvider = new VideosSearchDataProvider((IEnumerable<VideoHeader>) this.VM.VideosGenCol.Collection.ToList<VideoHeader>());
      DataTemplate dataTemplate = (DataTemplate) base.Resources["ItemTemplate"];
      Action<object, object> selectedItemCallback = new Action<object, object>(this.HandleSearchSelectedItem);
      Action<string> textChangedCallback = (Action<string>) (searchString =>
      {
        ((UIElement) this.gridContent).Visibility=(searchString != "" ? Visibility.Collapsed : Visibility.Visible);
        this._searchQuery = searchString;
      });
      DataTemplate itemTemplate = dataTemplate;
      Thickness? margin = new Thickness?(new Thickness(0.0, 77.0, 0.0, 0.0));
      GenericSearchUC.CreatePopup<VKClient.Common.Backend.DataObjects.Video, VideoHeader>((ISearchDataProvider<VKClient.Common.Backend.DataObjects.Video, VideoHeader>) searchDataProvider, selectedItemCallback, textChangedCallback, itemTemplate, (Func<SearchParamsUCBase>) (() => (SearchParamsUCBase) new SearchParamsVideoUC()), margin).Show((UIElement) this.gridContent);
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
        GroupVideosViewModel groupVideosViewModel = new GroupVideosViewModel(this.CommonParameters.OwnerId);
        base.DataContext=(groupVideosViewModel);
        groupVideosViewModel.PropertyChanged += new PropertyChangedEventHandler(this.Vm_PropertyChanged);
        groupVideosViewModel.VideosGenCol.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
        this._isInitialized = true;
      }
      this.HandleInputParams();
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
      Navigator.Current.NavigateToAddNewVideo(filePath, this.CommonParameters.OwnerId);
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanUploadVideo"))
        return;
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      int num = this.VM.CanUploadVideo ? 1 : 0;
      if (num != 0 && !this._appBar.Buttons.Contains(this._addVideoButton))
        this._appBar.Buttons.Insert(0, this._addVideoButton);
      if (num == 0 && this._appBar.Buttons.Contains(this._addVideoButton))
        this._appBar.Buttons.Remove(this._addVideoButton);
      if (this._appBar.Buttons.Count <= 0)
        return;
      this.ApplicationBar=((IApplicationBar) this._appBar);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/GroupVideosPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.gridContent = (Grid) base.FindName("gridContent");
      this.scroll = (ViewportControl) base.FindName("scroll");
      this.stackPanel = (StackPanel) base.FindName("stackPanel");
      this.virtPanel = (MyVirtualizingPanel2) base.FindName("virtPanel");
    }
  }
}
