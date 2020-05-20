using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Extensions;
using VKClient.Audio.Base.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKMessenger.Library;

namespace VKMessenger.Views
{
    public class AudioAttachmentUC : UserControl, IHandle<AudioAttachmentStartedPlaying>, IHandle, IHandle<AudioPlayerStateChanged>, IHandle<AudioPlayerClosedEvent>
    {
        public static readonly DependencyProperty StartedPlayingCallbackProperty = DependencyProperty.Register("StartedPlayingCallback", typeof(Action<AudioAttachmentUC>), typeof(AudioAttachmentUC), new PropertyMetadata(new PropertyChangedCallback(AudioAttachmentUC.StartedPlayingCallback_OnChanged)));
        private bool _addingToMyAudios;
        //private bool _settingProgressFromUpdate;
        //private readonly DateTime _lastTimeSetPositionManually;
        private DateTime _lastTimeSetPlayPauseManually;
        private bool _initializedUri;
        internal Border borderPlay;
        internal Border borderPause;
        internal TextBlock textBlockTrack;
        internal TextBlock textBlockArtist;
        internal TextBlock textBlockDuration;
        private bool _contentLoaded;

        private AttachmentViewModel AttachmentVM
        {
            get { return base.DataContext as AttachmentViewModel; }
        }

        private int ContentRestricted
        {
            get
            {
                AttachmentViewModel attachmentVm = this.AttachmentVM;
                if (attachmentVm == null)
                    return 0;
                return attachmentVm.AudioContentRestricted;
            }
        }

        private bool IsContentRestricted
        {
            get
            {
                return this.ContentRestricted > 0;
            }
        }

        public bool AssignedCurrentTrack
        {
            get
            {
                try
                {
                    AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
                    return track != null && track.GetTagId() == this.AttachmentVM.Audio.UniqueId;
                }
                catch
                {
                    return false;
                }
            }
        }

        public Action<AudioAttachmentUC> NotifyStartedPlayingCallback { get; set; }

        public Action<AudioAttachmentUC> StartedPlayingCallback
        {
            get
            {
                return (Action<AudioAttachmentUC>)base.GetValue(AudioAttachmentUC.StartedPlayingCallbackProperty);
            }
            set
            {
                base.SetValue(AudioAttachmentUC.StartedPlayingCallbackProperty, value);
            }
        }

        public bool UseWhiteForeground { get; set; }

        public AudioAttachmentUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler(this.AudioPlayer_OnLoaded));
            EventAggregator.Current.Subscribe(this);
        }

        private static void StartedPlayingCallback_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // ISSUE: explicit reference operation
            ((AudioAttachmentUC)d).NotifyStartedPlayingCallback = e.NewValue as Action<AudioAttachmentUC>;
        }

        private void LayoutRoot_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
            if (this.IsContentRestricted)
                AudioHelper.ShowContentRestrictedMessage(this.ContentRestricted);
            else if (((UIElement)this.borderPlay).Visibility == Visibility.Visible)
                this.Play(e);
            else
                this.Pause(e);
        }

        private void AudioPlayer_OnLoaded(object sender, RoutedEventArgs e)
        {
            double num = 0.0;
            AttachmentViewModel attachmentVm = this.AttachmentVM;
            int result;
            if ((attachmentVm != null ? attachmentVm.Audio : null) != null && int.TryParse(this.AttachmentVM.Audio.duration, out result))
            {
                this.textBlockDuration.Text = (UIStringFormatterHelper.FormatDuration(result));
                num = ((FrameworkElement)this.textBlockDuration).ActualWidth;
                Canvas.SetLeft((UIElement)this.textBlockDuration, base.Width - num - 16.0);
            }
            this.textBlockTrack.CorrectText(base.Width - Canvas.GetLeft((UIElement)this.textBlockTrack));
            this.textBlockArtist.CorrectText(base.Width - Canvas.GetLeft((UIElement)this.textBlockTrack) - num - (num > 0.0 ? 16.0 : 0.0));
            this.UpdateUIState();
        }

        private void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            this.UpdateUIState();
        }

        private void UpdateUIState()
        {
            try
            {
                bool flag = false;
                TimeSpan timeSpan;
                if (this.AssignedCurrentTrack)
                {
                    if (BGAudioPlayerWrapper.Instance.PlayerState == Microsoft.Phone.BackgroundAudio.PlayState.Playing)
                        flag = true;
                    timeSpan = BGAudioPlayerWrapper.Instance.Track.Duration;
                    double totalSeconds1 = timeSpan.TotalSeconds;
                    timeSpan = BGAudioPlayerWrapper.Instance.Position;
                    double totalSeconds2 = timeSpan.TotalSeconds;
                    if (totalSeconds1 != 0.0)
                    {
                        double num = totalSeconds2 / totalSeconds1;
                    }
                }
                timeSpan = DateTime.Now - this._lastTimeSetPlayPauseManually;
                if (timeSpan.TotalMilliseconds >= 7000.0)
                {
                    this.borderPlay.Visibility = (!flag ? Visibility.Visible : Visibility.Collapsed);
                    this.borderPause.Visibility = (flag ? Visibility.Visible : Visibility.Collapsed);
                }
                //timeSpan = DateTime.Now -this._lastTimeSetPositionManually;
                //if (timeSpan.TotalMilliseconds < 2000.0)
                //    return;
                //this._settingProgressFromUpdate = true;
                //this._settingProgressFromUpdate = false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("AudioAttachmentUC.UpdateUIState failed", ex);
            }
        }

        private void Play(GestureEventArgs e)
        {
            this.SetPlayPauseVisibilityManually(false);
            Logger.Instance.Info("AudioAttachmentUC.play_Tap, AssignedCurrentTrack = " + this.AssignedCurrentTrack.ToString());
            this.NotifyPlaying();
            if (!this.AssignedCurrentTrack)
            {
                AttachmentViewModel dc = this.DataContext as AttachmentViewModel;
                if (dc != null)
                {
                    AudioTrack track = AudioTrackHelper.CreateTrack(this.AttachmentVM.Audio);
                    Logger.Instance.Info("Starting track for uri: {0}", dc.ResourceUri);
                    BGAudioPlayerWrapper.Instance.Track = track;
                    if (!this._initializedUri)
                    {
                        Logger.Instance.Info("AudioAttachmentUC initializedUri = false, getting updated audio info");
                        AudioService instance = AudioService.Instance;
                        List<string> ids = new List<string>();
                        ids.Add(dc.MediaOwnerId.ToString() + "_" + dc.MediaId);
                        Action<BackendResult<List<AudioObj>, ResultCode>> callback = (Action<BackendResult<List<AudioObj>, ResultCode>>)(res =>
                        {
                            if (res.ResultCode != ResultCode.Succeeded)
                                return;
                            this._initializedUri = true;
                            AudioObj audio = res.ResultData.FirstOrDefault<AudioObj>();
                            if (audio == null)
                                return;
                            if (audio.url != dc.ResourceUri)
                                return;
                            Logger.Instance.Info("Received different uri for audio: {0}", audio.url);
                            dc.ResourceUri = audio.url;
                            dc.Audio = audio;
                            this.Dispatcher.BeginInvoke(delegate
                            {
                                if (!this.AssignedCurrentTrack && BGAudioPlayerWrapper.Instance.Track != null || (BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Paused || AudioTrackHelper.GetPositionSafe().TotalMilliseconds >= 1E-05))
                                    return;
                                BGAudioPlayerWrapper.Instance.Track = AudioTrackHelper.CreateTrack(audio);
                                this.Play();
                            });
                        });
                        instance.GetAudio(ids, callback);
                    }
                }
            }
            this.Play();
        }

        private void Play()
        {
            try
            {
                BGAudioPlayerWrapper.Instance.Volume = 1.0;
                BGAudioPlayerWrapper.Instance.Play();
                EventAggregator.Current.Publish(new AudioAttachmentStartedPlaying()
                {
                    UC = this
                });
            }
            catch
            {
            }
        }

        private void NotifyPlaying()
        {
            if (this.NotifyStartedPlayingCallback != null)
            {
                this.NotifyStartedPlayingCallback(this);
            }
            else
            {
                if (this.AttachmentVM == null || this.AttachmentVM.Audio == null)
                    return;
                List<AudioObj> tracks = new List<AudioObj>();
                tracks.Add(this.AttachmentVM.Audio);
                int audioSource = (int)CurrentMediaSource.AudioSource;
                PlaylistManager.SetAudioAgentPlaylist(tracks, (StatisticsActionSource)audioSource);
            }
        }

        private void Pause(GestureEventArgs e)
        {
            BGAudioPlayerWrapper.Instance.Pause();
            this.SetPlayPauseVisibilityManually(true);
        }

        private void SetPlayPauseVisibilityManually(bool showPlay)
        {
            ((UIElement)this.borderPlay).Visibility = (showPlay ? Visibility.Visible : Visibility.Collapsed);
            ((UIElement)this.borderPause).Visibility = (showPlay ? Visibility.Collapsed : Visibility.Visible);
            this._lastTimeSetPlayPauseManually = DateTime.Now;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        public void Handle(AudioAttachmentStartedPlaying message)
        {
            if (message.UC == this || this.AssignedCurrentTrack)
                return;
            this.SetPlayPauseVisibilityManually(true);
        }

        private void AudioAddToMyAudios_OnTap(object sender, RoutedEventArgs e)
        {
            if (this._addingToMyAudios)
                return;
            this._addingToMyAudios = true;
            AudioService.Instance.AddAudio(this.AttachmentVM.MediaOwnerId, this.AttachmentVM.MediaId, (Action<BackendResult<long, ResultCode>>)(res =>
            {
                this._addingToMyAudios = false;
                string messageStr = res.ResultCode == ResultCode.Succeeded ? CommonResources.Audio_AudioIsAdded : CommonResources.Error;
                Execute.ExecuteOnUIThread((Action)(() => new VKClient.Common.UC.GenericInfoUC().ShowAndHideLater(messageStr, null)));
            }));
        }

        public void Handle(AudioPlayerStateChanged message)
        {
            this.UpdateUIState();
        }

        public void Handle(AudioPlayerClosedEvent message)
        {
            ((UIElement)this.borderPlay).Visibility = Visibility.Visible;
            ((UIElement)this.borderPause).Visibility = Visibility.Collapsed;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AudioAttachmentUC.xaml", UriKind.Relative));
            this.borderPlay = (Border)base.FindName("borderPlay");
            this.borderPause = (Border)base.FindName("borderPause");
            this.textBlockTrack = (TextBlock)base.FindName("textBlockTrack");
            this.textBlockArtist = (TextBlock)base.FindName("textBlockArtist");
            this.textBlockDuration = (TextBlock)base.FindName("textBlockDuration");
        }
    }
}
