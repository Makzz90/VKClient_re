using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Utils;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Audio.ViewModels
{
    public class AudioPlayerViewModel : ViewModelBase, IHandle<AudioPlayerStateChanged>, IHandle, IHandle<AudioPlayerClosedEvent>, IHandle<AudioTrackEdited>
    {
        private static List<string> _addedTracks = new List<string>();
        private string _artwork = "";
        private string _trackIdOfArtwork = "";
        private TimeSpan _manualPositionThreshold = new TimeSpan(0, 0, 1);
        private DateTime _lastTimePlayerStateSetManually = DateTime.MinValue;
        private TimeSpan _manualStateThreshold = new TimeSpan(0, 0, 1);
        private DateTime _lastTimeOnStateChanged = DateTime.MinValue;
        private string _fetchingArtworkForTrackid = "";
        // private static AudioPlayerViewModel _instance;
        //private string _nextTrack1Str;
        //private string _nextTrack2Str;
        //private string _nextTrack3Str;
        private bool _haveNextTrack;
        private bool _havePreviousTrack;
        private DispatcherTimer _localTimer;
        private bool _isStopped;
        private double _manualPositionSeconds;
        private DateTime _lastTimeManualPositionSet;
        private bool _addingTrack;
        private PlayState _manualPlayState;
        private AudioObj _current;

        public string CurrentTrackStr
        {
            get
            {
                string artistName = this.ArtistName;
                string trackName = this.TrackName;
                if (!string.IsNullOrWhiteSpace(this.TrackName))
                    return artistName + " — " + trackName;
                return artistName;
            }
        }

        private bool IsStopped
        {
            get
            {
                return this._isStopped;
            }
            set
            {
                this._isStopped = value;
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.CurrentTrackVisibility));
            }
        }

        public Visibility CurrentTrackVisibility
        {
            get
            {
                if (this._isStopped || string.IsNullOrWhiteSpace(this.CurrentTrackStr))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }
        /*
        public string NextTrack1Str
        {
            get
            {
                if (!string.IsNullOrEmpty(this._nextTrack1Str))
                    return this._nextTrack1Str;
                return " ";
            }
            set
            {
                this._nextTrack1Str = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.NextTrack1Str));
            }
        }

        public string NextTrack2Str
        {
            get
            {
                if (!string.IsNullOrEmpty(this._nextTrack2Str))
                    return this._nextTrack2Str;
                return " ";
            }
            set
            {
                this._nextTrack2Str = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.NextTrack2Str));
            }
        }

        public string NextTrack3Str
        {
            get
            {
                if (!string.IsNullOrEmpty(this._nextTrack3Str))
                    return this._nextTrack3Str;
                return " ";
            }
            set
            {
                this._nextTrack3Str = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.NextTrack3Str));
            }
        }
        */
        public bool HaveNextTrack
        {
            get
            {
                return this._haveNextTrack;
            }
            set
            {
                this._haveNextTrack = value;
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.HaveNextTrack));
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.ForwardOpacity));
            }
        }

        public bool HavePreviousTrack
        {
            get
            {
                return this._havePreviousTrack;
            }
            set
            {
                this._havePreviousTrack = value;
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.HavePreviousTrack));
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.RevOpacity));
            }
        }

        public string Artwork
        {
            get
            {
                AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
                if (track == null || !(track.GetTagId() == this._trackIdOfArtwork))
                    return "";
                return this._artwork;
            }
        }

        public double RemainingSeconds
        {
            get
            {
                if (BGAudioPlayerWrapper.Instance.Track == null)
                    return 0.0;
                return this.Duration.TotalSeconds;
            }
        }

        public double ForwardOpacity
        {
            get
            {
                return !this.HaveNextTrack ? 0.6 : 1.0;
            }
        }

        public double RevOpacity
        {
            get
            {
                return !this.HavePreviousTrack ? 0.6 : 1.0;
            }
        }

        public string RemainingStr
        {
            get
            {
                if (BGAudioPlayerWrapper.Instance.Track == null)
                    return "";
                int durationSeconds = (int)Math.Round((this.Duration - this.Position).TotalSeconds);
                if (this.Position > this.Duration)
                    durationSeconds = 0;
                return string.Format("-{0}", UIStringFormatterHelper.FormatDuration(durationSeconds));
            }
        }

        public bool PreventPositionUpdates { get; set; }

        public double PositionSeconds
        {
            get
            {
                if (BGAudioPlayerWrapper.Instance.Track == null)
                    return 0.0;
                if (DateTime.Now - this._lastTimeManualPositionSet < this._manualPositionThreshold)
                    return this._manualPositionSeconds;
                return this.Position.TotalSeconds;
            }
            set
            {
                if (this.PreventPositionUpdates)
                    return;
                this._manualPositionSeconds = value;
                this.Position = TimeSpan.FromSeconds(value);
                this._lastTimeManualPositionSet = DateTime.Now;
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.PositionSeconds));
            }
        }

        public string PositionStr
        {
            get
            {
                if (BGAudioPlayerWrapper.Instance.Track == null)
                    return "";
                return UIStringFormatterHelper.FormatDuration((int)Math.Round(this.Position.TotalSeconds));
            }
        }

        public string TrackName
        {
            get
            {
                AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
                if (track == null)
                    return "";
                return track.Title ?? "";
            }
        }

        public string ArtistName
        {
            get
            {
                AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
                if (track == null)
                    return "";
                return track.Artist ?? "";
            }
        }

        private TimeSpan Position
        {
            get
            {
                try
                {
                    return BGAudioPlayerWrapper.Instance.Position;
                }
                catch (Exception)
                {
                    return new TimeSpan();
                }
            }
            set
            {
                try
                {
                    BGAudioPlayerWrapper.Instance.Position = value;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("AudioPlayerViewModel.PositionSet Failed to set position", ex);
                }
            }
        }

        private TimeSpan Duration
        {
            get
            {
                return new TimeSpan(0, 0, BGAudioPlayerWrapper.Instance.Track.GetTagDuration());
            }
        }

        public bool CanAddTrack
        {
            get
            {
                AudioObj fromBackgroundAudio = this.GetCurrentTrackFromBackgroundAudio();
                if (fromBackgroundAudio == null || fromBackgroundAudio.owner_id == AppGlobalStateManager.Current.LoggedInUserId)
                    return false;
                return !AudioPlayerViewModel._addedTracks.Contains(fromBackgroundAudio.UniqueId);
            }
        }

        public Visibility CanAddVisibility
        {
            get
            {
                if (!this.CanAddTrack)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility AddedVisibility
        {
            get
            {
                if (!this.CanAddTrack)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public double CanAddOpacity
        {
            get
            {
                return !this._addingTrack ? 1.0 : 0.3;
            }
        }

        public Visibility PlayImageVisibility
        {
            get
            {
                if ((!(DateTime.Now - this._lastTimePlayerStateSetManually < this._manualStateThreshold) ? BGAudioPlayerWrapper.Instance.PlayerState : this._manualPlayState) != Microsoft.Phone.BackgroundAudio.PlayState.Playing)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility PauseImageVisibility
        {
            get
            {
                if (this.PlayImageVisibility != Visibility.Visible)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool Shuffle
        {
            get
            {
                return PlaylistManager.ReadPlaybackSettings(true).Shuffle;
            }
            set
            {
                PlaybackSettings settings = PlaylistManager.ReadPlaybackSettings(true);
                settings.Shuffle = value;
                PlaylistManager.WritePlaybackSettings(settings);
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.Shuffle));
                this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>)(() => this.ShuffleBackground));
                //this.UpdateNextTrackInfo();
            }
        }

        public SolidColorBrush ShuffleBackground
        {
            get
            {
                return this.ConvertBoolToBrush(this.Shuffle);
            }
        }

        public bool HaveLyrics
        {
            get
            {
                return (ulong)this.LyricsId > 0UL;
            }
        }

        public long LyricsId
        {
            get
            {
                AudioObj fromBackgroundAudio = this.GetCurrentTrackFromBackgroundAudio();
                if (fromBackgroundAudio != null)
                    return fromBackgroundAudio.lyrics_id;
                return 0;
            }
        }

        public Visibility HaveLyricsVisibility
        {
            get
            {
                if (!this.HaveLyrics)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool Repeat
        {
            get
            {
                return PlaylistManager.ReadPlaybackSettings(true).Repeat;
            }
            set
            {
                PlaybackSettings settings = PlaylistManager.ReadPlaybackSettings(true);
                settings.Repeat = value;
                PlaylistManager.WritePlaybackSettings(settings);
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.Repeat));
                this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>)(() => this.RepeatBackground));
            }
        }

        public SolidColorBrush RepeatBackground
        {
            get
            {
                return this.ConvertBoolToBrush(this.Repeat);
            }
        }

        public bool Broadcast
        {
            get
            {
                return PlaylistManager.ReadPlaybackSettings(true).Broadcast;
            }
            set
            {
                PlaybackSettings settings = PlaylistManager.ReadPlaybackSettings(true);
                if (settings.Broadcast == value)
                    return;
                settings.Broadcast = value;
                PlaylistManager.WritePlaybackSettings(settings);
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.Broadcast));
                if (value)
                    AudioTrackHelper.BroadcastTrackIfNeeded(BackgroundAudioPlayer.Instance, (List<AudioObj>)null, (PlaybackSettings)null, true, true);
                else
                    AudioService.Instance.ResetBroadcast((Action<BackendResult<long, ResultCode>>)(res => { }));
                this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>)(() => this.BroadcastBackground));
            }
        }

        public SolidColorBrush BroadcastBackground
        {
            get
            {
                return this.ConvertBoolToBrush(this.Broadcast);
            }
        }

        public bool AllowPlay
        {
            get
            {
                return true;
            }
        }

        public bool AllowPause
        {
            get
            {
                return true;
            }
        }

        public double MiniPlayerProgressWidth
        {
            get
            {
                TimeSpan timeSpan = this.Position;
                double totalSeconds1 = timeSpan.TotalSeconds;
                timeSpan = this.Duration;
                double totalSeconds2 = timeSpan.TotalSeconds;
                return Math.Round(416.0 * (totalSeconds1 / totalSeconds2), (MidpointRounding)1);
            }
        }

        public AudioPlayerViewModel()
        {
            this.InitializeTimer();
        }

        public void AddTrack()
        {
            if (this._addingTrack)
                return;
            this._addingTrack = true;
            this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.CanAddOpacity));
            try
            {
                List<AudioObj> tracks = PlaylistManager.LoadTracksFromIsolatedStorage(true).Tracks;
                AudioTrack bTrack = BGAudioPlayerWrapper.Instance.Track;
                if (bTrack == null)
                    return;
                AudioObj current = tracks.FirstOrDefault<AudioObj>((Func<AudioObj, bool>)(t => t.UniqueId == bTrack.GetTagId()));
                this.SetInProgress(true, "");
                AudioService.Instance.AddAudio(current.owner_id, current.aid, (Action<BackendResult<long, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._addingTrack = false;
                    this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.CanAddOpacity));
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    AudioPlayerViewModel._addedTracks.Add(current.UniqueId);
                    AudioObj audioObj = StreamUtils.CloneThroughSerialize<AudioObj>(current);
                    audioObj.owner_id = AppGlobalStateManager.Current.LoggedInUserId;
                    audioObj.aid = res.ResultData;
                    this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.CanAddTrack));
                    this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.AddedVisibility));
                    this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.CanAddVisibility));
                    EventAggregator.Current.Publish((object)new AudioTrackAddedRemoved()
                    {
                        Added = true,
                        Audio = audioObj
                    });
                }));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to add track", ex);
            }
        }

        private AudioObj GetCurrentTrackFromBackgroundAudio()
        {
            if (this._current != null)
                return this._current;
            List<AudioObj> tracks = PlaylistManager.LoadTracksFromIsolatedStorage(true).Tracks;
            AudioTrack bTrack = BGAudioPlayerWrapper.Instance.Track;
            if (bTrack == null)
                return (AudioObj)null;
            return tracks.FirstOrDefault<AudioObj>((Func<AudioObj, bool>)(t => t.UniqueId == bTrack.GetTagId()));
        }

        public SolidColorBrush ConvertBoolToBrush(bool bValue)
        {
            if (!bValue)
                return Application.Current.Resources["PhoneAudioPlayerSliderBackgroundBrush"] as SolidColorBrush;
            return Application.Current.Resources["PhoneNewsActionLikedForegroundBrush"] as SolidColorBrush;
        }

        private void InitializeTimer()
        {
            this._localTimer = new DispatcherTimer();
            this._localTimer.Interval = (TimeSpan.FromSeconds(0.5));
            this._localTimer.Tick += (new EventHandler(this._localTimer_Tick));
        }

        private void _localTimer_Tick(object sender, EventArgs e)
        {
            this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.PositionSeconds));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PositionStr));
            this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.RemainingSeconds));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.RemainingStr));
            this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.MiniPlayerProgressWidth));
        }

        public void Activate(bool active)
        {
            if (active)
            {
                this.OnPlayerStateChanged(null);
                EventAggregator.Current.Subscribe(this);
            }
            else
                EventAggregator.Current.Unsubscribe(this);
        }
        /*
        private void UpdateNextTrackInfo()
        {
            List<AudioObj> tracks = PlaylistManager.LoadTracksFromIsolatedStorage(true).Tracks;
            this.GetCurrentTrackFromBackgroundAudio();
            bool startedNewCycle = false;
            BackgroundAudioPlayer instance = BackgroundAudioPlayer.Instance;
            int num1 = 1;
            PlaybackSettings settings = new PlaybackSettings();
            int num2 = this.Shuffle ? 1 : 0;
            settings.Shuffle = num2 != 0;
            int num3 = this.Repeat ? 1 : 0;
            settings.Repeat = num3 != 0;
            int num4 = 1;
            AudioObj nextAudio = AudioTrackHelper.GetNextAudio(instance, num1 != 0, out startedNewCycle, settings, num4 != 0);
            if (nextAudio == null)
                return;
            this.NextTrack1Str = nextAudio.title ?? "";
            if (string.IsNullOrEmpty(nextAudio.artist))
                return;
            this.NextTrack1Str = nextAudio.artist + " — " + this.NextTrack1Str;
        }
        */
        public void Play()
        {
            if (!this.AllowPlay)
                return;
            BGAudioPlayerWrapper.Instance.Play();
            this._lastTimePlayerStateSetManually = DateTime.Now;
            this._manualPlayState = (PlayState)3;
            this.NotifyPlayPauseState();
        }

        public void Pause()
        {
            if (!this.AllowPause)
                return;
            BGAudioPlayerWrapper.Instance.Pause();
            this._lastTimePlayerStateSetManually = DateTime.Now;
            this._manualPlayState = (PlayState)2;
            this.NotifyPlayPauseState();
        }

        public void PreviousTrack()
        {
            this.NextOrPreviousTrack(false);
        }

        public void NextTrack()
        {
            this.NextOrPreviousTrack(true);
        }

        private void NextOrPreviousTrack(bool next)
        {
            bool startedNewCycle = false;
            AudioTrack nextTrack = AudioTrackHelper.GetNextTrack(BackgroundAudioPlayer.Instance, next, out startedNewCycle, null, true);
            if (nextTrack == null)
                return;
            BGAudioPlayerWrapper.Instance.Track = nextTrack;
        }

        public void Handle(AudioPlayerStateChanged message)
        {
            this.OnPlayerStateChanged(null);
        }

        private void OnPlayerStateChanged(AudioPlayerStateChanged args = null)
        {
            this.IsStopped = false;
            PlayState playState = args != null ? args.PlayState : BGAudioPlayerWrapper.Instance.PlayerState;
            this._lastTimeOnStateChanged = DateTime.Now;
            if (playState == Microsoft.Phone.BackgroundAudio.PlayState.Playing)
                this.StartTrackingPosition();
            else
                this.StopTrackingPosition();
            this.OnTrackChanged();
            this.NotifyPlayPauseState();
        }

        public void Handle(AudioPlayerClosedEvent message)
        {
            this.StopTrackingPosition();
            this.IsStopped = true;
        }

        private void NotifyPlayPauseState()
        {
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.PlayImageVisibility));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.PauseImageVisibility));
        }

        private void OnTrackChanged()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.PreventPositionUpdates = true;
                this._current = this.GetCurrentTrackFromBackgroundAudio();
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.TrackName));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.ArtistName));
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.RemainingSeconds));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.RemainingStr));
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.PositionSeconds));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PositionStr));
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.CanAddTrack));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.CanAddVisibility));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.AddedVisibility));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Artwork));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.HaveLyricsVisibility));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.CurrentTrackStr));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.CurrentTrackVisibility));
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.MiniPlayerProgressWidth));
                this.FetchArtwork();
                this.PreventPositionUpdates = false;
                //this.UpdateNextTrackInfo();
                this._current = null;
            }));
        }

        private void FetchArtwork()
        {
            AudioTrack track1 = BGAudioPlayerWrapper.Instance.Track;
            if (track1 == null)
                return;
            string tag = track1.GetTagId();
            if (this._fetchingArtworkForTrackid == tag)
                return;
            this._fetchingArtworkForTrackid = tag;
            AudioService.Instance.GetAlbumArtwork(track1.Artist + " " + track1.Title, (Action<BackendResult<AlbumArtwork, ResultCode>>)(res =>
            {
                this._fetchingArtworkForTrackid = "";
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    AudioTrack track2 = BGAudioPlayerWrapper.Instance.Track;
                    if (track2 == null || !(track2.GetTagId() == tag))
                        return;
                    this._trackIdOfArtwork = tag;
                    this._artwork = res.ResultData.ImageUri;
                    this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Artwork));
                }));
            }));
        }

        private void StopTrackingPosition()
        {
            this._localTimer.Stop();
        }

        private void StartTrackingPosition()
        {
            this._localTimer.Start();
        }

        public void Cleanup()
        {
            this.StopTrackingPosition();
        }

        public void Handle(AudioTrackEdited message)
        {
            AudioObj fromBackgroundAudio = this.GetCurrentTrackFromBackgroundAudio();
            if (fromBackgroundAudio != null && message.OwnerId == fromBackgroundAudio.owner_id && message.Id == fromBackgroundAudio.id)
            {
                fromBackgroundAudio.artist = message.Artist;
                fromBackgroundAudio.title = message.Title;
            }
            AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
            long? nullable1 = track != null ? new long?(track.GetTagOwnerId()) : new long?();
            string str = track != null ? track.GetTagId() : null;
            if (str != null && ((IEnumerable<char>)str).Contains<char>('_'))
                str = str.Substring(str.IndexOf('_') + 1);
            if (track != null)
            {
                long? nullable2 = nullable1;
                long valueOrDefault = nullable2.GetValueOrDefault();
                if ((message.OwnerId == valueOrDefault ? (nullable2.HasValue ? 1 : 0) : 0) != 0 && message.Id.ToString() == str)
                {
                    track.BeginEdit();
                    track.Artist = message.Artist;
                    track.Title = message.Title;
                    track.EndEdit();
                }
            }
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.ArtistName));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.TrackName));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.CurrentTrackStr));
        }
    }
}
