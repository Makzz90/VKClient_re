using Microsoft.Phone.BackgroundAudio;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Audio.Library
{
    public class AudioHeader : ViewModelBase, IHaveUniqueKey, ISearchableItemHeader<AudioObj>, IHandle<AudioPlayerStateChanged>, IHandle, IHandle<AudioTrackEdited>, IHandle<AudioTrackDownloadProgress>
    {
        private DateTime _lastTimeAssignedTrack = DateTime.MinValue;

        public AudioObj Track { get; private set; }

        public long MessageId { get; private set; }

        public bool IsMenuEnabled
        {
            get
            {
                return AppGlobalStateManager.Current.LoggedInUserId == this.Track.owner_id;
            }
        }

        public string UIDuration
        {
            get
            {
                int result = 0;
                if (int.TryParse(this.Track.duration, out result))
                    return UIStringFormatterHelper.FormatDuration(result);
                return "";
            }
        }

        public Visibility IsCachedVisibility//mod
        {
            get
            {
                return VKClient.Audio.Base.AudioCache.AudioCacheManager.Instance.GetLocalFileForUniqueId(this.Track.UniqueId) != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public SolidColorBrush TitleBrush
        {
            get
            {
                //if (this.IsCurrentTrack)
                //    return (SolidColorBrush)Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"];
                //return (SolidColorBrush)Application.Current.Resources["PhoneContrastTitleBrush"];
                return (SolidColorBrush)Application.Current.Resources[this.IsCurrentTrack ? "PhoneSidebarSelectedIconBackgroundBrush" : "PhoneContrastTitleBrush"];
            }
        }

        public SolidColorBrush SubtitleBrush
        {
            get
            {
                if (this.IsCurrentTrack)
                    return (SolidColorBrush)Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"];
                return (SolidColorBrush)Application.Current.Resources["PhoneVKSubtleBrush"];
            }
        }

        public bool IsCurrentTrack
        {
            get
            {
                try
                {
                    AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
                    return track != null && track.GetTagId() == this.Track.UniqueId;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public Visibility IsCurrentTrackVisibility
        {
            get
            {
                return this.IsCurrentTrack.ToVisiblity();
            }
        }
        //
        private bool play_visib = true;

        public Visibility PlayVisibility
        {
            get
            {
                return play_visib.ToVisiblity();
            }
            set
            {
                if (value != this.play_visib.ToVisiblity())
                {
                    play_visib = value == Visibility.Visible ? true : false;
                    this.NotifyPropertyChanged<Visibility>(() => this.PlayVisibility);
                    this.NotifyPropertyChanged<Visibility>(() => this.PauseVisibility);
                }
            }
        }

        public Visibility PauseVisibility
        {
            get
            {
                return (!play_visib).ToVisiblity();
            }

        }

        public double DownloadProgress
        {
            get;
            private set;
        }
        //
        public bool IsContentRestricted
        {
            get
            {
                return this.ContentRestricted > 0;
            }
        }

        private int ContentRestricted
        {
            get
            {
                return this.Track.content_restricted;
            }
        }

        public double TrackOpacity
        {
            get
            {
                return this.IsContentRestricted ? 0.4 : 1.0;
            }
        }

        public bool IsLocalItem
        {
            get
            {
                return this.Track.owner_id == AppGlobalStateManager.Current.LoggedInUserId;
            }
        }

        public string Artist
        {
            get
            {
                return Extensions.ForUI(this.Track.artist);
            }
        }

        public string Title
        {
            get
            {
                return Extensions.ForUI(this.Track.title);
            }
        }

        public AudioHeader(AudioObj track, long messageId = 0)
        {
            this.Track = track;
            this.MessageId = messageId;
            
            //
            if(this.IsCurrentTrack)
            {
                if (BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Playing)
                {
                    this.PlayVisibility = Visibility.Collapsed;
                }
                else
                {
                    this.PlayVisibility = Visibility.Visible;
                }
            }
            else
            {
                this.PlayVisibility = Visibility.Visible;
            }

            if (this.IsCachedVisibility == Visibility.Visible)
            {
                this.DownloadProgress = 1.0;
            }

            EventAggregator.Current.Subscribe(this);
        }

        public void NotifyChanged()
        {
            this.NotifyPropertyChanged<bool>(() => this.IsCurrentTrack);
            this.NotifyPropertyChanged<Visibility>(() => this.IsCurrentTrackVisibility);
            this.NotifyPropertyChanged<double>(() => this.TrackOpacity);
            this.NotifyPropertyChanged<SolidColorBrush>(() => this.TitleBrush);
            this.NotifyPropertyChanged<SolidColorBrush>(() => this.SubtitleBrush);
        }

        public void ShowContentRestrictedMessage()
        {
            AudioHelper.ShowContentRestrictedMessage(this.ContentRestricted);
        }

        public bool TryAssignTrack()
        {
            if (this.ContentRestricted > 0)
                return false;
            if ((DateTime.Now - this._lastTimeAssignedTrack).TotalMilliseconds > 5000.0)
            {
                BGAudioPlayerWrapper.Instance.Track = AudioTrackHelper.CreateTrack(this.Track);
                this._lastTimeAssignedTrack = DateTime.Now;
            }
            return true;
        }

        public string GetKey()
        {
            AudioObj track = this.Track;
            if (track == null)
                return "";
            return track.owner_id.ToString() + "_" + track.id;
        }

        public bool Matches(string searchString)
        {
            return this.Track.title.ToLowerInvariant().Contains(searchString.ToLowerInvariant());
        }

        public void Handle(AudioPlayerStateChanged message)
        {
            this.NotifyChanged();

            if (this.IsCurrentTrack)
            {
                if (BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Playing)
                {
                    this.PlayVisibility = Visibility.Collapsed;
                }
                else
                {
                    this.PlayVisibility = Visibility.Visible;
                }
            }
            else
            {
                this.PlayVisibility = Visibility.Visible;
            }
        }

        public void Handle(AudioTrackEdited message)
        {
            if (message.OwnerId != this.Track.owner_id || message.Id != this.Track.id)
                return;
            this.Track.artist = message.Artist;
            this.Track.title = message.Title;
            this.NotifyPropertyChanged<string>(() => this.Artist);
            this.NotifyPropertyChanged<string>(() => this.Title);
        }

        public void Handle(AudioTrackDownloadProgress message)
        {
            if (message.Id == this.Track.UniqueId)
            {
                this.DownloadProgress = message.Progress / 100.0;
                this.NotifyPropertyChanged<double>(() => this.DownloadProgress);
            }
        }
    }
}
