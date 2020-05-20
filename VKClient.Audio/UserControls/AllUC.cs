using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Audio.ViewModels;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

using Microsoft.Phone.BackgroundAudio;
using VKClient.Audio.Base;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKMessenger.Views;
using System.Windows.Media;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Audio.Base.BLExtensions;

using System.Linq;

namespace VKClient.Audio
{
    public class AllUC : UserControl
    {
        internal ExtendedLongListSelector AllAudios;
        private bool _contentLoaded;

        private AllAudioViewModel VM
        {
            get
            {
                return base.DataContext as AllAudioViewModel;
            }
        }

        public bool IsInPickMode { get; set; }

        public ExtendedLongListSelector ListAudios
        {
            get
            {
                return this.AllAudios;
            }
        }

        public AllUC()
        {
            this.InitializeComponent();
        }

        private void EditTrackItem_Tap(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            AudioHeader dataContext = frameworkElement.DataContext as AudioHeader;
            if (dataContext == null)
                return;
            Navigator.Current.NavigateToEditAudio(dataContext.Track);
        }

        private void DeleteTrackItem_Tap(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            AudioHeader dataContext = frameworkElement.DataContext as AudioHeader;
            if (dataContext == null)
                return;
            this.DeleteAudios(new List<AudioHeader>() { dataContext });
        }

        public void DeleteAudios(List<AudioHeader> list)
        {
            if (!this.AskDeleteAudioConfirmation(list.Count))
                return;
            this.VM.DeleteAudios(list);
        }

        private bool AskDeleteAudioConfirmation(int count)
        {
            return MessageBox.Show(CommonResources.GenericConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.DeleteOneAudioFrm, AudioResources.DeleteTwoFourAudiosFrm, AudioResources.DeleteFiveAudiosFrm, true, null, false), (MessageBoxButton)1) == MessageBoxResult.OK;
        }

        private void AllAudios_Link_2(object sender, LinkUnlinkEventArgs e)
        {
            this.VM.AllTracks.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Audio;component/UserControls/AllUC.xaml", UriKind.Relative));
            this.AllAudios = (ExtendedLongListSelector)base.FindName("AllAudios");
        }
        //
        private void Temp_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string tag = (sender as Grid).Tag.ToString();
            AudioHeader track = this.VM.AllTracks.Collection.First((h) => h.Track.UniqueId == tag);

            if (track.Track.UniqueId == BGAudioPlayerWrapper.Instance.Track.GetTagId())
            {
                if (BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Playing)
                    BGAudioPlayerWrapper.Instance.Pause();
                else
                    BGAudioPlayerWrapper.Instance.Play();
                return;
            }
            else
            {
                this.NavigateToAudioPlayer(track, this.VM.AllTracks.Collection, false);
            }
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {            
            string tag = (sender as Grid).Tag.ToString();
            AudioHeader track = this.VM.AllTracks.Collection.First((h) => h.Track.UniqueId == tag);
            this.NavigateToAudioPlayer(track, this.VM.AllTracks.Collection, true);
        }
        
        private void NavigateToAudioPlayer(AudioHeader track, IEnumerable enumerable, bool need_navigate = false)
        {
            if (track == null)
                return;
            if (track.IsContentRestricted)
            {
                track.ShowContentRestrictedMessage();
            }
            else if (track.Track.UniqueId == BGAudioPlayerWrapper.Instance.Track.GetTagId())
            {
                if (need_navigate)
                    Navigator.Current.NavigateToAudioPlayer(true);
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
                if (need_navigate)
                    Navigator.Current.NavigateToAudioPlayer(true);
                else
                    BGAudioPlayerWrapper.Instance.Play();
            }
        }
    }
}
