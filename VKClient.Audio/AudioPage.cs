using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Audio.UserControls;
using VKClient.Audio.ViewModels;
using VKClient.Common;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VKClient.Audio
{
    public class AudioPage : PageBase
    {
        private bool _isInitialized;
        private AudioPage.PageMode _pageMode;
        private bool _isInMultiSelectMode;
        private long _albumId;
        private ApplicationBar _appBarAudio;
        private ApplicationBar _appBarMultiselect;
        private ApplicationBar _appBarAlbums;
        private ApplicationBarIconButton _appBarButtonAudioPlayer;
        private ApplicationBarIconButton _appBarButtonAddAudio;
        private ApplicationBarIconButton _appBarButtonSearchAudio;
        private ApplicationBarIconButton _appBarButtonEdit;
        private ApplicationBarIconButton _appBarButtonMoveToAlbum;
        private ApplicationBarIconButton _appBarButtonDelete;
        private ApplicationBarIconButton _appBarButtonCancel;
        private ApplicationBarIconButton _appBarButtonAddNewAlbum;
        private DialogService _dialogService;
        internal Grid LayoutRoot;
        internal GenericHeaderUC ucHeader;
        internal Pivot mainPivot;
        internal PivotItem pivotItemAudio;
        internal AllUC allAudio;
        internal PivotItem pivotItemAlbums;
        internal AlbumsUC allAlbums;
        internal PullToRefreshUC ucPullToRefresh;
        private bool _contentLoaded;

        private AllAudioViewModel ViewModel
        {
            get
            {
                return base.DataContext as AllAudioViewModel;
            }
        }

        public bool IsInMultiSelectMode
        {
            get
            {
                return this._isInMultiSelectMode;
            }
            set
            {
                if (this._isInMultiSelectMode == value)
                    return;
                this._isInMultiSelectMode = value;
                this.UpdateAppBar();
            }
        }

        public bool HaveAtLeastOneItemSelected
        {
            get
            {
                return false;
            }
        }

        public bool IsOwnAudio
        {
            get
            {
                if (this.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId)
                    return !this.CommonParameters.IsGroup;
                return false;
            }
        }

        public AudioPage()
        {
            this._appBarButtonAudioPlayer = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.nowplaying.rest.png", UriKind.Relative), Text = AudioResources.AppBar_NowPlaying };
            this._appBarButtonAddAudio = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative), Text = CommonResources.AppBar_Add };
            this._appBarButtonSearchAudio = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative), Text = AudioResources.AppBar_Search };
            this._appBarButtonEdit = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.manage.rest.png", UriKind.Relative), Text = AudioResources.Edit };
            this._appBarButtonMoveToAlbum = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.movetofolder.rest.png", UriKind.Relative), Text = AudioResources.AddToAlbum };
            this._appBarButtonDelete = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.delete.rest.png", UriKind.Relative), Text = AudioResources.Delete };
            this._appBarButtonCancel = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative), Text = CommonResources.AppBar_Cancel };
            this._appBarButtonAddNewAlbum = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative), Text = AudioResources.AppBar_Add };

            this.InitializeComponent();
            this.ucHeader.OnHeaderTap = new Action(this.HandleHeaderTap);
            this.ucPullToRefresh.TrackListBox(this.allAudio.ListAudios);
            this.ucPullToRefresh.TrackListBox(this.allAlbums.ListAllAlbums);
            this.allAudio.ListAudios.OnRefresh = delegate
            {
                this.ViewModel.AllTracks.LoadData(true, false, null, false);
            };
            this.allAlbums.ListAllAlbums.OnRefresh = delegate
            {
                this.ViewModel.AllAlbumsVM.AllAlbums.LoadData(true, false, null, false);
            };/*
      this.allAudio.AllAudios.SelectionChanged+=(delegate(object s, SelectionChangedEventArgs e)
      {
          this.HandleAudioSelectionChanged(this.allAudio.AllAudios, this.allAudio.AllAudios.SelectedItem, false);
      });*/
            this.allAlbums.AllAlbums.SelectionChanged += (new SelectionChangedEventHandler(this.AllAlbums_SelectionChanged));
        }

        private void HandleHeaderTap()
        {
            if (this.mainPivot.SelectedItem == this.pivotItemAudio)
            {
                this.allAudio.ListAudios.ScrollToTop();
            }
            else
            {
                if (this.mainPivot.SelectedItem != this.pivotItemAlbums)
                    return;
                this.allAlbums.ListAllAlbums.ScrollToTop();
            }
        }

        private void AllAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
            if (longListSelector != null)
            {
                AudioAlbumHeader selectedItem = longListSelector.SelectedItem as AudioAlbumHeader;
                if (selectedItem != null)
                {
                    if (this._pageMode == AudioPage.PageMode.PickAlbum)
                    {
                        ParametersRepository.SetParameterForId("PickedAlbum", selectedItem.Album);
                        Navigator.Current.GoBack();
                    }
                    else
                        Navigator.Current.NavigateToAudio((int)this._pageMode, this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, selectedItem.Album.album_id, 0, selectedItem.Album.title);
                }
            }
            longListSelector.SelectedItem = null;
        }

        private void HandleAudioSelectionChanged(object sender, object selectedItem, bool fromSearch)
        {
            ListBox listBox = sender as ListBox;
            ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
            AudioHeader track = selectedItem as AudioHeader;
            if (listBox != null)
                ((Selector)listBox).SelectedItem = null;
            if (longListSelector != null)
                longListSelector.SelectedItem = null;
            if (track == null)
                return;
            if (this._pageMode == AudioPage.PageMode.PickAudio)
            {
                ParametersRepository.SetParameterForId("PickedAudio", track.Track);
                if (this._albumId != 0L)
                    ((Page)this).NavigationService.RemoveBackEntrySafe();
                Navigator.Current.GoBack();
            }
            else if (listBox != null)
            {
                if (fromSearch)
                    CurrentMediaSource.AudioSource = StatisticsActionSource.search;
                this.NavigateToAudioPlayer(track, ((ItemsControl)listBox).ItemsSource, true);
            }
            else
            {
                if (longListSelector == null)
                    return;
                if (fromSearch)
                    CurrentMediaSource.AudioSource = StatisticsActionSource.search;
                IEnumerable enumerable = !longListSelector.IsFlatList ? this.GetExtendedSelectorGroupedItems(longListSelector.ItemsSource) : longListSelector.ItemsSource;
                this.NavigateToAudioPlayer(track, enumerable, true);
            }
        }

        private IEnumerable GetExtendedSelectorGroupedItems(IList itemsSource)
        {
            if (itemsSource != null)
            {
                foreach (IEnumerable enumerable in (IEnumerable)itemsSource)
                {
                    foreach (object obj in enumerable)
                        yield return obj;
                }
            }
        }

        private void NavigateToAudioPlayer(AudioHeader track, IEnumerable enumerable, bool startPlaying = false)
        {
            if (track == null)
                return;
            if (track.IsContentRestricted)
            {
                track.ShowContentRestrictedMessage();
            }
            else
            {
                List<AudioObj> tracks = new List<AudioObj>();
                IEnumerator enumerator = enumerable.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        AudioHeader current = enumerator.Current as AudioHeader;
                        if (current != null)
                            tracks.Add(current.Track);
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                PlaylistManager.SetAudioAgentPlaylist(tracks, CurrentMediaSource.AudioSource);
                if (!track.TryAssignTrack())
                    return;
                Navigator.Current.NavigateToAudioPlayer(false);
            }
        }

        private void Initialize()
        {
            this._albumId = 0L;
            long exludeAlbumId = 0;
            if (((Page)this).NavigationContext.QueryString.ContainsKey("AlbumId"))
                this._albumId = long.Parse(((Page)this).NavigationContext.QueryString["AlbumId"]);
            this._pageMode = (AudioPage.PageMode)int.Parse(((Page)this).NavigationContext.QueryString["PageMode"]);
            if (((Page)this).NavigationContext.QueryString.ContainsKey("ExcludeAlbumId"))
                exludeAlbumId = long.Parse(((Page)this).NavigationContext.QueryString["ExcludeAlbumId"]);
            base.DataContext = (new AllAudioViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, (uint)this._pageMode > 0U, this._albumId, exludeAlbumId));
            if (this._albumId != 0L)
            {
                ((PresentationFrameworkCollection<object>)((ItemsControl)this.mainPivot).Items).Remove(this.pivotItemAlbums);
                PivotItem pivotItemAudio = this.pivotItemAudio;
                TextBlock textBlock = new TextBlock();
                string str = ((Page)this).NavigationContext.QueryString["AlbumName"];
                textBlock.Text = str;
                double num = 46.0;
                textBlock.FontSize = num;
                FontFamily fontFamily = new FontFamily("Segoe WP SemiLight");
                textBlock.FontFamily = fontFamily;
                pivotItemAudio.Header = textBlock;
            }
            if (this._pageMode != AudioPage.PageMode.PickAlbum)
                return;
            ((PresentationFrameworkCollection<object>)((ItemsControl)this.mainPivot).Items).Remove(this.pivotItemAudio);
        }

        public void SetTitle()
        {
            string str = "";
            if (this._albumId != 0L)
                str = AudioResources.ALBUM;
            switch (this._pageMode)
            {
                case AudioPage.PageMode.Default:
                    str = AudioResources.Audio;
                    break;
                case AudioPage.PageMode.PickAudio:
                    str = AudioResources.AUDIO_CHOOSE;
                    break;
                case AudioPage.PageMode.PickAlbum:
                    str = AudioResources.Albums_Chose;
                    break;
            }
            this.ucHeader.TextBlockTitle.Text = str;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (!this.IsInMultiSelectMode)
                return;
            e.Cancel = true;
            this.IsInMultiSelectMode = false;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                if (this.CommonParameters.UserOrGroupId == 0L)
                    this.CommonParameters.UserOrGroupId = AppGlobalStateManager.Current.LoggedInUserId;
                this.Initialize();
                this.BuildAppBar();
                this.PerformInitialLoad();
                this.allAudio.IsInPickMode = this.CommonParameters.PickMode;
                this.UpdateAppBar();
                this._isInitialized = true;
            }
            this.ProcessInputParameters();
            CurrentMediaSource.AudioSource = this.CommonParameters.IsGroup ? StatisticsActionSource.audios_group : StatisticsActionSource.audios_user;
            FileOpenPickerContinuationEventArgs parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if (parameterForIdAndReset == null || !((IEnumerable<StorageFile>)parameterForIdAndReset.Files).Any<StorageFile>())
                return;
            StorageFile file = ((IEnumerable<StorageFile>)parameterForIdAndReset.Files).First<StorageFile>();
            this.SkipNextNavigationParametersRepositoryClearing = true;
            Navigator.Current.NavigateToAddNewAudio(file);
        }

        private void ProcessInputParameters()
        {
            AudioAlbum pickedAlbum = ParametersRepository.GetParameterForIdAndReset("PickedAlbum") as AudioAlbum;
            if (pickedAlbum == null || !this.IsInMultiSelectMode)
                return;
            List<AudioHeader> headersToMove = this.GetSelectedAudioHeaders();
            this.IsInMultiSelectMode = false;
            this.ViewModel.MoveTracksToAlbum(headersToMove, pickedAlbum, (Action<bool>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!result)
                {
                    ExtendedMessageBox.ShowSafe(CommonResources.GenericErrorText);
                }
                else
                {
                    if (MessageBox.Show(UIStringFormatterHelper.FormatNumberOfSomething(headersToMove.Count, AudioResources.OneAudioMovedFrm, AudioResources.TwoFourAudiosMovedFrm, AudioResources.FiveAudiosMovedFrm, true, pickedAlbum.title, false), AudioResources.MoveAudios, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                        return;
                    Navigator.Current.NavigateToAudio(0, this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, pickedAlbum.album_id, 0, pickedAlbum.title);
                }
            }))));
        }

        private void PerformInitialLoad()
        {
            switch (this._pageMode)
            {
                case AudioPage.PageMode.Default:
                case AudioPage.PageMode.PickAudio:
                    this.ViewModel.AllTracks.LoadData(false, false, null, false);
                    break;
                case AudioPage.PageMode.PickAlbum:
                    this.ViewModel.AllAlbumsVM.AllAlbums.LoadData(false, false, null, false);
                    break;
            }
        }

        private void AllAudios_MultiSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            if (this._dialogService != null && this._dialogService.IsOpen)
                return;
            if (this.mainPivot.SelectedItem == this.pivotItemAudio)
            {
                if (this.IsInMultiSelectMode)
                {
                    this.ApplicationBar = ((IApplicationBar)this._appBarMultiselect);
                    this._appBarButtonMoveToAlbum.IsEnabled = this.HaveAtLeastOneItemSelected;
                    this._appBarButtonDelete.IsEnabled = this.HaveAtLeastOneItemSelected;
                }
                else
                {
                    this.ApplicationBar = ((IApplicationBar)this._appBarAudio);
                    if (!this.IsOwnAudio)
                        this._appBarAudio.Buttons.Remove(this._appBarButtonEdit);
                }
            }
            else if (this.mainPivot.SelectedItem == this.pivotItemAlbums && (this._appBarAlbums.Buttons.Count > 0 || this._appBarAlbums.MenuItems.Count > 0))
            {
                this.ApplicationBar = ((IApplicationBar)this._appBarAlbums);
                if (!this.IsOwnAudio)
                    this._appBarAlbums.Buttons.Remove(this._appBarButtonAddNewAlbum);
            }
            if (this.mainPivot.SelectedItem == this.pivotItemAudio && this._appBarAudio != null && (!this._appBarAudio.Buttons.Contains(this._appBarButtonAddAudio) && this.ViewModel.AlbumId == 0L))
                this._appBarAudio.Buttons.Insert(0, this._appBarButtonAddAudio);
            if (this.mainPivot.SelectedItem == this.pivotItemAudio && this.ViewModel.AlbumId == 0L || (this._appBarAudio == null || !this._appBarAudio.Buttons.Contains(this._appBarButtonAddAudio)))
                return;
            this._appBarAudio.Buttons.Remove(this._appBarButtonAddAudio);
        }

        private void BuildAppBar()
        {
            ApplicationBar applicationBar1 = new ApplicationBar();
            Color appBarBgColor1 = VKConstants.AppBarBGColor;
            applicationBar1.BackgroundColor = appBarBgColor1;
            Color appBarFgColor1 = VKConstants.AppBarFGColor;
            applicationBar1.ForegroundColor = appBarFgColor1;
            this._appBarAudio = applicationBar1;
            this._appBarAudio.Opacity = 0.9;
            if (this.ViewModel.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId && this._albumId == 0L && this._pageMode == AudioPage.PageMode.Default)
            {
                this._appBarButtonAddAudio.Click += (new EventHandler(this.addAudio_Click));
                this._appBarAudio.Buttons.Add(this._appBarButtonAddAudio);
            }
            this._appBarButtonSearchAudio.Click += (new EventHandler(this.searchAudio_Click));
            this._appBarAudio.Buttons.Add(this._appBarButtonSearchAudio);
            this._appBarButtonEdit.Click += (new EventHandler(this._appBarButtonEdit_Click));
            ApplicationBar applicationBar2 = new ApplicationBar();
            Color appBarBgColor2 = VKConstants.AppBarBGColor;
            applicationBar2.BackgroundColor = appBarBgColor2;
            Color appBarFgColor2 = VKConstants.AppBarFGColor;
            applicationBar2.ForegroundColor = appBarFgColor2;
            this._appBarMultiselect = applicationBar2;
            this._appBarMultiselect.Opacity = 0.9;
            this._appBarButtonMoveToAlbum.Click += (new EventHandler(this._appBarButtonMoveToAlbum_Click));
            this._appBarMultiselect.Buttons.Add(this._appBarButtonMoveToAlbum);
            this._appBarButtonDelete.Click += (new EventHandler(this._appBarButtonDelete_Click));
            this._appBarMultiselect.Buttons.Add(this._appBarButtonDelete);
            this._appBarButtonCancel.Click += (new EventHandler(this._appBarButtonCancel_Click));
            this._appBarMultiselect.Buttons.Add(this._appBarButtonCancel);
            ApplicationBar applicationBar3 = new ApplicationBar();
            Color appBarBgColor3 = VKConstants.AppBarBGColor;
            applicationBar3.BackgroundColor = appBarBgColor3;
            Color appBarFgColor3 = VKConstants.AppBarFGColor;
            applicationBar3.ForegroundColor = appBarFgColor3;
            this._appBarAlbums = applicationBar3;
            this._appBarAlbums.Opacity = 0.9;
            this._appBarButtonAudioPlayer.Click += (new EventHandler(this._appBarButtonAudioPlayer_Click));
        }

        private void _appBarButtonAddNewAlbum_Click(object sender, EventArgs e)
        {
            this.ShowEditAlbum(new AudioAlbum());
        }

        private void ShowEditAlbum(AudioAlbum album)
        {
            DialogService dc = new DialogService();
            dc.SetStatusBarBackground = true;
            dc.HideOnNavigation = false;
            EditAlbumUC editAlbum = new EditAlbumUC();
            editAlbum.textBlockCaption.Text = (album.album_id == 0L ? AudioResources.CreateAlbum : AudioResources.EditAlbum);
            dc.Child = (FrameworkElement)editAlbum;
            ((UIElement)editAlbum.buttonSave).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((s, e) =>
            {
                album.title = editAlbum.textBoxText.Text;
                this.ViewModel.AllAlbumsVM.CreateEditAlbum(album);
                dc.Hide();
            }));
            dc.Show((UIElement)this.mainPivot);
        }

        private void _appBarButtonAudioPlayer_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToAudioPlayer(false);
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            this.IsInMultiSelectMode = false;
        }

        private void _appBarButtonDelete_Click(object sender, EventArgs e)
        {
            this.allAudio.DeleteAudios(this.GetSelectedAudioHeaders());
        }

        private List<AudioHeader> GetSelectedAudioHeaders()
        {
            return new List<AudioHeader>();
        }

        private void _appBarButtonMoveToAlbum_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToAudio(2, 0, false, 0, this._albumId, "");
        }

        private void _appBarButtonEdit_Click(object sender, EventArgs e)
        {
            this.IsInMultiSelectMode = true;
        }

        private void addAudio_Click(object sender, EventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            List<string>.Enumerator enumerator = VKConstants.SupportedAudioExtensions.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    string current = enumerator.Current;
                    fileOpenPicker.FileTypeFilter.Add(current);
                }
            }
            finally
            {
                enumerator.Dispose();
            }
            ((IDictionary<string, object>)fileOpenPicker.ContinuationData)["Operation"] = "AudioFromPhone";
            fileOpenPicker.PickSingleFileAndContinue();
        }

        private void searchAudio_Click(object sender, EventArgs e)
        {
            this.EnableSearch();
        }

        private void EnableSearch()
        {
            this._dialogService = new DialogService
            {
                BackgroundBrush = new SolidColorBrush(Colors.Transparent),
                AnimationType = DialogService.AnimationTypes.None,
                HideOnNavigation = false
            };
            AudioSearchDataProvider searchDataProvider = new AudioSearchDataProvider(this.ViewModel.AllTracks.Collection);
            DataTemplate itemTemplate = (DataTemplate)Application.Current.Resources["TrackTemplate"];
            GenericSearchUC searchUC = new GenericSearchUC();
            searchUC.LayoutRootGrid.Margin = (new Thickness(0.0, 77.0, 0.0, 0.0));
            searchUC.Initialize<AudioObj, AudioHeader>(searchDataProvider, delegate(object listBox, object selectedItem)
            {
                this.HandleAudioSelectionChanged(listBox, selectedItem, true);
            }, itemTemplate);
            searchUC.SearchTextBox.TextChanged += (delegate(object s, TextChangedEventArgs ev)
            {
                bool flag = searchUC.SearchTextBox.Text != "";
                this.mainPivot.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
            });
            this._dialogService.Child = searchUC;
            this._dialogService.Show(this.mainPivot);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateAppBar();
            if (this.mainPivot.SelectedItem != this.pivotItemAlbums || this.ViewModel.AllAlbumsVM.AllAlbums.IsLoaded)
                return;
            this.ViewModel.AllAlbumsVM.AllAlbums.LoadData(false, false, null, false);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Audio;component/AudioPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.mainPivot = (Pivot)base.FindName("mainPivot");
            this.pivotItemAudio = (PivotItem)base.FindName("pivotItemAudio");
            this.allAudio = (AllUC)base.FindName("allAudio");
            this.pivotItemAlbums = (PivotItem)base.FindName("pivotItemAlbums");
            this.allAlbums = (AlbumsUC)base.FindName("allAlbums");
            this.ucPullToRefresh = (PullToRefreshUC)base.FindName("ucPullToRefresh");
        }

        private enum PageMode
        {
            Default,
            PickAudio,
            PickAlbum,
        }
    }
}
