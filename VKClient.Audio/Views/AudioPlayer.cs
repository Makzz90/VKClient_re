using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Library;
using VKClient.Audio.UserControls;
using VKClient.Audio.ViewModels;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Localization;

namespace VKClient.Audio.Views
{
    public class AudioPlayer : PageBase
    {
        private bool _isInitialized;
        private ApplicationBarIconButton _addNewAppBarButton;
        //private bool _subscribed;
        private bool _applyPositionFromVM;
        private ApplicationBar _appBar;
        //internal Grid LayoutRoot;
        //internal TextBlock textBlockNowPlayingLabel;
        internal Path music_icon;
        internal Slider slider;
        //internal TextBlock textBlockNextLabel;
        private bool _contentLoaded;
        //
        internal Grid ArtPlace;

        private AudioPlayerViewModel VM
        {
            get
            {
                return base.DataContext as AudioPlayerViewModel;
            }
        }

        public AudioPlayer()
        {
            ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
            Uri uri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
            applicationBarIconButton.IconUri = uri;
            string barAddToMyAudios = CommonResources.Audio_AppBar_AddToMyAudios;
            applicationBarIconButton.Text = barAddToMyAudios;
            this._addNewAppBarButton = applicationBarIconButton;
            this._applyPositionFromVM = true;
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = appBarBgColor;
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = appBarFgColor;
            this._appBar = applicationBar;
            //base.\u002Ector();
            this.InitializeComponent();
            this.SetupAppBar();
            //this.textBlockNowPlayingLabel.Text = (CommonResources.AudioPlayer_NowPlaying.ToUpperInvariant());
            //this.textBlockNextLabel.Text = (CommonResources.AudioPlayer_Next.ToUpperInvariant());
            ((Control)this._progressBar).Foreground = ((Brush)(Application.Current.Resources["PhoneAudioPlayerForeground2Brush"] as SolidColorBrush));
            base.Loaded += (new RoutedEventHandler(this.AudioPlayer_Loaded));
        }

        private void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!this._subscribed)
                this.VM.PropertyChanged += new PropertyChangedEventHandler(this.VM_PropertyChanged);
            this.slider.Value = this.VM.PositionSeconds;
        }

        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanAddTrack")
                this.UpdateAppBar();
            if (!(e.PropertyName == "PositionSeconds") || !this._applyPositionFromVM)
                return;
            (this.slider).Value = this.VM.PositionSeconds;
        }

        private void Slider_ManipulationStarted_1(object sender, ManipulationStartedEventArgs e)
        {
            this._applyPositionFromVM = false;
        }

        private void Slider_ManipulationCompleted_1(object sender, ManipulationCompletedEventArgs e)
        {
            this.VM.PositionSeconds = (this.slider).Value;
            this._applyPositionFromVM = true;
        }

        private void UpdateAppBar()
        {
        }

        private void SetupAppBar()
        {
            this._addNewAppBarButton.Click += (new EventHandler(this.addNew_Click));
            this._appBar.Buttons.Add(this._addNewAppBarButton);
        }

        private void addNew_Click(object sender, EventArgs e)
        {
            this.VM.AddTrack();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                AudioPlayerViewModel audioPlayerViewModel = new AudioPlayerViewModel();
                audioPlayerViewModel.PreventPositionUpdates = true;
                base.DataContext = audioPlayerViewModel;
                audioPlayerViewModel.PreventPositionUpdates = false;
                //int num = ((Page)this).NavigationContext.QueryString["startPlaying"] == bool.TrueString ? 1 : 0;
                this._isInitialized = true;
            }
            this.VM.Activate(true);
            this.UpdateAppBar();
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this.VM.Activate(false);
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            base.OnRemovedFromJournal(e);
            this.VM.Cleanup();
        }

        private void playImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.Play();
        }

        private void pauseImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.Pause();
        }

        private void RevButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.PreviousTrack();
        }

        private void ForwardButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.NextTrack();
        }

        private void Shuffle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.Shuffle = !this.VM.Shuffle;
        }

        private void Repeat_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.Repeat = !this.VM.Repeat;
        }

        private void Broadcast_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.Broadcast = !this.VM.Broadcast;
        }

        private void SongText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            long lyricsId = this.VM.LyricsId;
            if (lyricsId == 0L)
                return;
            DialogService dialogService = new DialogService();
            dialogService.SetStatusBarBackground = true;
            dialogService.HideOnNavigation = false;
            LyricsUC ucLyrics = new LyricsUC();
            ucLyrics.textBlockNowPlayingTitle.Text = (this.VM.CurrentTrackStr.ToUpperInvariant());
            LyricsUC lyricsUc = ucLyrics;
            dialogService.Child = lyricsUc;
            dialogService.Show(null);
            AudioService.Instance.GetLyrics(lyricsId, delegate(BackendResult<Lyrics, ResultCode> res)
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    Execute.ExecuteOnUIThread(delegate
                    {
                        ucLyrics.textBlockLyrics.Text = res.ResultData.text;
                    });
                }
            });
        }

        private void Add_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.AddTrack();
        }

        private void Next_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DialogService expr_12 = new DialogService();
            expr_12.SetStatusBarBackground = true;
            expr_12.HideOnNavigation = false;
            PlaylistUC ucPlaylist = new PlaylistUC();
            expr_12.Child = ucPlaylist;
            expr_12.Opened += delegate(object s, EventArgs ev)
            {
                PlaylistViewModel vm = new PlaylistViewModel();
                vm.Shuffle = this.VM.Shuffle;
                ucPlaylist.DataContext = (vm);
                //Action _9__2 = null;
                vm.Audios.LoadData(false, false, delegate(BackendResult<List<AudioObj>, ResultCode> res)
                {
                    Action arg_1F_0;
                    //if ((arg_1F_0 = _9__2) == null)
                    //{
                        arg_1F_0 = (/*_9__2 =*/ delegate
                        {
                            IEnumerable<AudioHeader> arg_2F_0 = vm.Audios.Collection;
                            Func<AudioHeader, bool> arg_2F_1 = new Func<AudioHeader, bool>((i) => { return i.IsCurrentTrack; });

                            AudioHeader audioHeader = Enumerable.FirstOrDefault<AudioHeader>(arg_2F_0, arg_2F_1);
                            if (audioHeader != null)
                            {
                                int num = vm.Audios.Collection.IndexOf(audioHeader);
                                if (num > 0)
                                {
                                    audioHeader = vm.Audios.Collection[num - 1];
                                }
                                ucPlaylist.AllAudios.ScrollTo(audioHeader);
                            }
                        });
                    //}
                    Execute.ExecuteOnUIThread(arg_1F_0);
                }, false);
            };
            expr_12.Show(null);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Audio;component/Views/AudioPlayer.xaml", UriKind.Relative));
            //this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            //this.textBlockNowPlayingLabel = (TextBlock)base.FindName("textBlockNowPlayingLabel");
            this.music_icon = (Path)base.FindName("music_icon");
            this.slider = (Slider)base.FindName("slider");
            //this.textBlockNextLabel = (TextBlock)base.FindName("textBlockNextLabel");
            //
            this.ArtPlace = (Grid)base.FindName("ArtPlace");
        }

        private void ArtPlace_Loaded(object sender, RoutedEventArgs e)
        {
            this.ArtPlace.Height = this.ArtPlace.ActualWidth;
            this.music_icon.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
