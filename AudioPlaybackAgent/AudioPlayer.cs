using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;


namespace AudioPlaybackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        private static volatile bool _classInitialized;

        public AudioPlayer()
        {
            if (!AudioPlayer._classInitialized)
            {
                AudioPlayer._classInitialized = true;
                Deployment.Current.Dispatcher.BeginInvoke((Action)(() => Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(this.AudioPlayer_UnhandledException)));
            }
            BGAudioPlayerWrapper.Instance.BackgroundAppMode = true;
        }

        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (!Debugger.IsAttached)
                return;
            Debugger.Break();
        }

        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            bool flag = true;
            try
            {
                switch (playState)
                {
                    case PlayState.TrackReady:
                        if (player.PlayerState != PlayState.Playing)
                        {
                            flag = false;
                            AudioTrackHelper.PlayCurrentTrack(player, (Action<bool>)(res => this.NotifyComplete()), false);
                            break;
                        }
                        break;
                    case PlayState.TrackEnded:
                        PlaybackSettings settings = PlaylistManager.ReadPlaybackSettings(false);
                        if (settings.Repeat)
                        {
                            player.Stop();
                            player.Position = TimeSpan.FromSeconds(0.0);
                            flag = false;
                            AudioTrackHelper.PlayCurrentTrack(player, (Action<bool>)(res => this.NotifyComplete()), false);
                            break;
                        }
                        bool startedNewCycle;
                        AudioTrack nextTrack = this.GetNextTrack(player, out startedNewCycle, settings);
                        if (nextTrack != null)
                        {
                            if (!startedNewCycle)
                            {
                                player.Track = nextTrack;
                                AudioTrackHelper.PlayCurrentTrack(player, (Action<bool>)(res => this.NotifyComplete()), false);
                                flag = false;
                                break;
                            }
                            this.NotifyComplete();
                            break;
                        }
                        break;
                }
                AudioTrackHelper.BroadcastTrackIfNeeded(player, null, null, false, false);
                if (!flag)
                    return;
                this.NotifyComplete();
            }
            catch
            {
                this.NotifyComplete();
            }
        }

        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            bool flag = true;
            try
            {
                string str1 = "AudioPlaybackAgent.OnUserAction " + action + " track name=" + track.Title ?? "";
                string str2 = !(track.Source != null) ? str1 + ", Source=null" : str1 + ", Source=" + track.Source.ToString();
                bool startedNewCycle;
                switch (action)
                {
                    case UserAction.Stop:
                        player.Stop();
                        break;
                    case UserAction.Pause:
                        player.Pause();
                        break;
                    case UserAction.Play:
                        if (player.PlayerState != PlayState.Playing)
                        {
                            flag = false;
                            AudioTrackHelper.PlayCurrentTrack(player, (Action<bool>)(res => this.NotifyComplete()), false);
                            break;
                        }
                        break;
                    case UserAction.SkipNext:
                        AudioTrack nextTrack = this.GetNextTrack(player, out startedNewCycle, null);
                        player.Track = nextTrack;
                        flag = false;
                        AudioTrackHelper.PlayCurrentTrack(player, (Action<bool>)(res => this.NotifyComplete()), false);
                        break;
                    case UserAction.SkipPrevious:
                        AudioTrack previousTrack = this.GetPreviousTrack(player, out startedNewCycle, null);
                        player.Track = previousTrack;
                        flag = false;
                        AudioTrackHelper.PlayCurrentTrack(player, (Action<bool>)(res => this.NotifyComplete()), false);
                        break;
                    case UserAction.FastForward:
                        player.FastForward();
                        break;
                    case UserAction.Rewind:
                        player.Rewind();
                        break;
                    case UserAction.Seek:
                        player.Position = (TimeSpan)param;
                        break;
                }
                if (!flag)
                    return;
                this.NotifyComplete();
            }
            catch
            {
                if (!flag)
                    return;
                this.NotifyComplete();
            }
        }

        private AudioTrack GetNextTrack(BackgroundAudioPlayer player, out bool startedNewCycle, PlaybackSettings settings = null)
        {
            return this.GetRequiredTrack(player, AudioPlayer.NextTrackType.Next, out startedNewCycle, settings);
        }

        private AudioTrack GetPreviousTrack(BackgroundAudioPlayer player, out bool startedNewCycle, PlaybackSettings settings = null)
        {
            return this.GetRequiredTrack(player, AudioPlayer.NextTrackType.Previous, out startedNewCycle, settings);
        }

        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            string str1 = "AudioPlayer.OnError  " + error.Message + "track " + track.Title;
            string str2 = !(track.Source != null) ? str1 + ", Source=null" : str1 + ", " + track.Source.ToString();
            if (isFatal)
                this.Abort();
            else
                this.NotifyComplete();
        }

        protected override void OnCancel()
        {
        }

        private AudioTrack GetRequiredTrack(BackgroundAudioPlayer player, AudioPlayer.NextTrackType trackType, out bool startedNewCycle, PlaybackSettings settings = null)
        {
            return AudioTrackHelper.GetNextTrack(player, trackType == AudioPlayer.NextTrackType.Next, out startedNewCycle, settings, false);
        }

        private enum NextTrackType
        {
            Previous,
            Next,
        }
    }
}
