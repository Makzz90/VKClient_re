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
    public partial class GroupVideosPage : PageBase
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

        private GroupVideosViewModel VM
        {
            get
            {
                return this.DataContext as GroupVideosViewModel;
            }
        }

        public GroupVideosPage()
        {
            this.InitializeComponent();
            this.BuildAppBar();
            this.ucHeader.Title = CommonResources.Profile_Videos.ToUpperInvariant();
            this.ucHeader.OnHeaderTap = (Action)(() => this.virtPanel.ScrollToBottom(false));
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.virtPanel);
            this.virtPanel.OnRefresh = (Action)(() => this.VM.VideosGenCol.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)null, false));
            this.virtPanel.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.scroll), false);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.virtPanel);
            this.scroll.BindViewportBoundsTo((FrameworkElement)this.stackPanel);
            this.virtPanel.CreateVirtItemFunc = (Func<object, IVirtualizable>)(obj => (IVirtualizable)new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() =>
            {
                UserControlVirtualizable controlVirtualizable = (UserControlVirtualizable)null;
                if (obj is OwnerHeaderWithSubscribeViewModel)
                    controlVirtualizable = (UserControlVirtualizable)this._virtControlsPool.GetFromPool<OwnerHeaderWithSubscribeUC>();
                if (obj is SectionHeaderViewModel)
                    controlVirtualizable = (UserControlVirtualizable)this._virtControlsPool.GetFromPool<SectionHeaderUC>();
                if (obj is AlbumsListHorizontalViewModel)
                    controlVirtualizable = (UserControlVirtualizable)this._virtControlsPool.GetFromPool<CatalogItemsHorizontalExtUC>();
                if (obj is VideoHeader)
                {
                    controlVirtualizable = (UserControlVirtualizable)this._virtControlsPool.GetFromPool<CatalogItemUC>();
                    (controlVirtualizable as CatalogItemUC).GridLayoutRoot.Background = (Brush)(Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush);
                }
                controlVirtualizable.DataContext = obj;
                return controlVirtualizable;
            }), (Func<double>)(() =>
            {
                if (obj is OwnerHeaderWithSubscribeViewModel)
                    return 68.0;
                if (obj is SectionHeaderViewModel)
                    return 80.0;
                if (obj is AlbumsListHorizontalViewModel)
                    return 242.0;
                return obj is VideoHeader ? 128.0 : 0.0;
            }), (Action<UserControlVirtualizable>)(uc => this._virtControlsPool.AddBackToPool(uc)), 0.0, false));
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
            this._addVideoButton.Click += new EventHandler(this._addVideoButton_Click);
            if (this._appBar.Buttons.Count <= 0)
                return;
            this.ApplicationBar = (IApplicationBar)this._appBar;
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
            ((IDictionary<string, object>)fileOpenPicker.ContinuationData)["Operation"] = (object)"VideoFromPhone";
            fileOpenPicker.PickSingleFileAndContinue();
        }

        private void SearchVideoTap(object sender, EventArgs e)
        {
            this.EnableSearch();
        }

        private void EnableSearch()
        {
            VideosSearchDataProvider searchDataProvider = new VideosSearchDataProvider((IEnumerable<VideoHeader>)this.VM.VideosGenCol.Collection.ToList<VideoHeader>());
            DataTemplate dataTemplate = (DataTemplate)this.Resources["ItemTemplate"];
            Action<object, object> selectedItemCallback = new Action<object, object>(this.HandleSearchSelectedItem);
            Action<string> textChangedCallback = (Action<string>)(searchString =>
            {
                this.gridContent.Visibility = searchString != "" ? Visibility.Collapsed : Visibility.Visible;
                this._searchQuery = searchString;
            });
            DataTemplate itemTemplate = dataTemplate;
            Thickness? margin = new Thickness?(new Thickness(0.0, 77.0, 0.0, 0.0));
            GenericSearchUC.CreatePopup<VKClient.Common.Backend.DataObjects.Video, VideoHeader>((ISearchDataProvider<VKClient.Common.Backend.DataObjects.Video, VideoHeader>)searchDataProvider, selectedItemCallback, textChangedCallback, itemTemplate, (Func<SearchParamsUCBase>)(() => (SearchParamsUCBase)new SearchParamsVideoUC()), margin).Show((UIElement)this.gridContent);
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
                this.DataContext = (object)groupVideosViewModel;
                groupVideosViewModel.PropertyChanged += new PropertyChangedEventHandler(this.Vm_PropertyChanged);
                groupVideosViewModel.VideosGenCol.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)null, false);
                this._isInitialized = true;
            }
            this.HandleInputParams();
        }

        private void HandleInputParams()
        {
            FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if (continuationEventArgs == null || !((IEnumerable<StorageFile>)continuationEventArgs.Files).Any<StorageFile>())
                return;
            StorageFile storageFile = ((IEnumerable<StorageFile>)continuationEventArgs.Files).First<StorageFile>();
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
            if (num != 0 && !this._appBar.Buttons.Contains((object)this._addVideoButton))
                this._appBar.Buttons.Insert(0, (object)this._addVideoButton);
            if (num == 0 && this._appBar.Buttons.Contains((object)this._addVideoButton))
                this._appBar.Buttons.Remove((object)this._addVideoButton);
            if (this._appBar.Buttons.Count <= 0)
                return;
            this.ApplicationBar = (IApplicationBar)this._appBar;
        }
    }
}
